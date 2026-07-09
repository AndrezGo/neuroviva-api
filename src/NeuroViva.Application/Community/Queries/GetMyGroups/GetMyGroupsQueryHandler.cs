using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Community;
using NeuroViva.Domain.Community.Repositories;
using NeuroViva.Domain.Patients.Repositories;

namespace NeuroViva.Application.Community.Queries.GetMyGroups;

public sealed class GetMyGroupsQueryHandler
    : IRequestHandler<GetMyGroupsQuery, Result<IReadOnlyList<GroupSummaryDto>>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPatientRepository _patientRepo;
    private readonly IPatientDiseaseRepository _patientDiseaseRepo;
    private readonly IGroupRepository _groupRepo;
    private readonly IGroupMemberRepository _groupMemberRepo;
    private readonly IUnitOfWork _uow;

    public GetMyGroupsQueryHandler(
        ICurrentUserService currentUser,
        IPatientRepository patientRepo,
        IPatientDiseaseRepository patientDiseaseRepo,
        IGroupRepository groupRepo,
        IGroupMemberRepository groupMemberRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _patientRepo = patientRepo;
        _patientDiseaseRepo = patientDiseaseRepo;
        _groupRepo = groupRepo;
        _groupMemberRepo = groupMemberRepo;
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<GroupSummaryDto>>> Handle(
        GetMyGroupsQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var userId = _currentUser.UserId.Value;

        var patient = await _patientRepo.GetByUserIdAsync(userId, cancellationToken);
        if (patient is null)
            return Error.NotFound("patient.profile_not_found", "No patient profile linked to this user.");

        var patientDiseases = await _patientDiseaseRepo.ListByPatientAsync(patient.Id, cancellationToken);
        var diseaseIds = patientDiseases.Select(pd => pd.DiseaseId).ToList();

        var groups = await _groupRepo.ListActiveByDiseaseIdsAsync(diseaseIds, cancellationToken);

        var memberJoinedAt = new Dictionary<Guid, DateTime>();

        foreach (var group in groups)
        {
            var existingMember = await _groupMemberRepo.GetAsync(group.Id, userId, cancellationToken);

            if (existingMember is null)
            {
                var newMember = GroupMember.Join(group.Id, userId);
                await _groupMemberRepo.AddAsync(newMember, cancellationToken);
                memberJoinedAt[group.Id] = newMember.JoinedAt;
            }
            else if (existingMember.Status != "active")
            {
                existingMember.Rejoin();
                _groupMemberRepo.Update(existingMember);
                memberJoinedAt[group.Id] = existingMember.JoinedAt;
            }
            else
            {
                memberJoinedAt[group.Id] = existingMember.JoinedAt;
            }
        }

        await _uow.SaveChangesAsync(cancellationToken);

        var dtos = groups.Select(g => new GroupSummaryDto(
            Id: g.Id,
            Name: g.Name,
            Slug: g.Slug,
            Description: g.Description,
            AvatarUrl: g.AvatarUrl,
            DiseaseId: g.DiseaseId,
            JoinedAt: memberJoinedAt.TryGetValue(g.Id, out var jat) ? jat : DateTime.UtcNow
        )).ToList();

        return Result<IReadOnlyList<GroupSummaryDto>>.Success(dtos);
    }
}
