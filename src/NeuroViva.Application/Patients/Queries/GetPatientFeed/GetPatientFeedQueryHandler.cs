using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
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

    public GetPatientFeedQueryHandler(
        ICurrentUserService currentUser,
        IPatientRepository patientRepo,
        IPatientDiseaseRepository patientDiseaseRepo,
        IResourceRepository resourceRepo)
    {
        _currentUser = currentUser;
        _patientRepo = patientRepo;
        _patientDiseaseRepo = patientDiseaseRepo;
        _resourceRepo = resourceRepo;
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

        var dtos = resources.Select(r => new ResourceDto(
            Id: r.Id,
            Title: r.Title,
            Type: r.Type.ToString(),
            Url: r.Url,
            Description: r.Description,
            CreatedAt: r.CreatedAt,
            EmbedUrl: r.Type == ResourceType.Video
                ? YouTubeUrlParser.TryGetEmbedUrl(r.Url)
                : null
        )).ToList();

        return Result<IReadOnlyList<ResourceDto>>.Success(dtos);
    }
}
