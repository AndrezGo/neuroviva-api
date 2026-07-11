using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Application.Common.Services;
using NeuroViva.Domain.Catalog.Repositories;
using NeuroViva.Domain.Content;
using NeuroViva.Domain.Content.Enums;
using NeuroViva.Domain.Content.Repositories;
using NeuroViva.Domain.Patients.Repositories;

namespace NeuroViva.Application.Patients.Queries.GetPatientFeed;

public sealed class GetPatientFeedQueryHandler
    : IRequestHandler<GetPatientFeedQuery, Result<IReadOnlyList<ResourceDto>>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPatientRepository _patientRepo;
    private readonly IPatientDiseaseRepository _patientDiseaseRepo;
    private readonly IResourceRepository _resourceRepo;
    private readonly IChannelRepository _channelRepo;
    private readonly IDiseaseRepository _diseaseRepo;
    private readonly INewsArticleRepository _newsArticleRepo;
    private readonly IGoogleNewsRssService _googleNewsRss;

    public GetPatientFeedQueryHandler(
        ICurrentUserService currentUser,
        IPatientRepository patientRepo,
        IPatientDiseaseRepository patientDiseaseRepo,
        IResourceRepository resourceRepo,
        IChannelRepository channelRepo,
        IDiseaseRepository diseaseRepo,
        INewsArticleRepository newsArticleRepo,
        IGoogleNewsRssService googleNewsRss)
    {
        _currentUser = currentUser;
        _patientRepo = patientRepo;
        _patientDiseaseRepo = patientDiseaseRepo;
        _resourceRepo = resourceRepo;
        _channelRepo = channelRepo;
        _diseaseRepo = diseaseRepo;
        _newsArticleRepo = newsArticleRepo;
        _googleNewsRss = googleNewsRss;
    }

    public async Task<Result<IReadOnlyList<ResourceDto>>> Handle(
        GetPatientFeedQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var patient = await _patientRepo.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (patient is null)
            return Error.NotFound("patient.profile_not_found", "No patient profile linked to this user");

        var patientDiseases = await _patientDiseaseRepo.ListByPatientAsync(patient.Id, cancellationToken);
        var diseaseIds = patientDiseases.Select(pd => pd.DiseaseId).ToList();

        var resources = await _resourceRepo.ListApprovedAsync(request.Type, diseaseIds, cancellationToken);

        var channelIds = resources
            .Where(r => r.ChannelId.HasValue)
            .Select(r => r.ChannelId!.Value)
            .Distinct()
            .ToList();

        var channelNameById = channelIds.Count == 0
            ? new Dictionary<Guid, string>()
            : (await _channelRepo.ListByIdsAsync(channelIds, cancellationToken))
                .ToDictionary(c => c.Id, c => c.Name);

        var dtos = resources.Select(r => new ResourceDto(
            Id: r.Id,
            Title: r.Title,
            Type: r.Type.ToString(),
            Url: r.Url,
            Description: r.Description,
            CreatedAt: r.CreatedAt,
            EmbedUrl: r.Type == ResourceType.Video
                ? YouTubeUrlParser.TryGetEmbedUrl(r.Url)
                : null,
            ChannelId: r.ChannelId,
            ChannelName: r.ChannelId.HasValue && channelNameById.TryGetValue(r.ChannelId.Value, out var cname) ? cname : null,
            SourceName: null,
            PublishedAt: null
        )).ToList();

        if (request.Type == ResourceType.News && diseaseIds.Count > 0)
        {
            foreach (var diseaseId in diseaseIds)
            {
                var disease = await _diseaseRepo.GetByIdAsync(diseaseId, cancellationToken);
                if (disease is null) continue;
                if (!DiseaseSearchTerms.TryGetSearchTerm(disease.Slug, out var searchTerm)) continue;

                var lastFetched = await _newsArticleRepo.GetLastFetchedAtAsync(diseaseId, cancellationToken);
                if (lastFetched is null || lastFetched < DateTime.UtcNow.AddHours(-6))
                {
                    var rawItems = await _googleNewsRss.SearchAsync(searchTerm, cancellationToken);
                    if (rawItems.Count > 0)
                    {
                        var toUpsert = rawItems
                            .Select(x => NewsArticle.Create(
                                diseaseId,
                                x.Title,
                                x.Link,
                                x.SourceName,
                                x.Description,
                                x.PublishedAt,
                                x.ExternalGuid))
                            .ToList();
                        await _newsArticleRepo.UpsertManyAsync(toUpsert, cancellationToken);
                    }
                }
            }

            var since = DateTime.UtcNow.AddDays(-30);
            var newsArticles = await _newsArticleRepo.ListByDiseaseIdsAsync(diseaseIds, since, cancellationToken);

            var newsDtos = newsArticles.Select(a => new ResourceDto(
                Id: a.Id,
                Title: a.Title,
                Type: ResourceType.News.ToString(),
                Url: a.SourceUrl,
                Description: a.Description,
                CreatedAt: a.FetchedAt,
                EmbedUrl: null,
                ChannelId: null,
                ChannelName: null,
                SourceName: a.SourceName,
                PublishedAt: a.PublishedAt));

            var combined = dtos
                .Concat(newsDtos)
                .GroupBy(d => d.Url ?? d.Id.ToString(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderByDescending(d => d.PublishedAt ?? d.CreatedAt)
                .ToList();

            return Result<IReadOnlyList<ResourceDto>>.Success(combined);
        }

        return Result<IReadOnlyList<ResourceDto>>.Success(dtos);
    }
}
