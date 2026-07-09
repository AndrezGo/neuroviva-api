using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Community;
using NeuroViva.Domain.Community.Repositories;
using NeuroViva.Domain.Patients.Repositories;

namespace NeuroViva.Application.Community.Commands.CreatePost;

public sealed class CreatePostCommandHandler
    : IRequestHandler<CreatePostCommand, Result<CreatePostResult>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPatientRepository _patientRepo;
    private readonly IGroupRepository _groupRepo;
    private readonly IGroupMemberRepository _groupMemberRepo;
    private readonly ICommunityPostRepository _postRepo;
    private readonly IUnitOfWork _uow;

    public CreatePostCommandHandler(
        ICurrentUserService currentUser,
        IPatientRepository patientRepo,
        IGroupRepository groupRepo,
        IGroupMemberRepository groupMemberRepo,
        ICommunityPostRepository postRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _patientRepo = patientRepo;
        _groupRepo = groupRepo;
        _groupMemberRepo = groupMemberRepo;
        _postRepo = postRepo;
        _uow = uow;
    }

    public async Task<Result<CreatePostResult>> Handle(
        CreatePostCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var userId = _currentUser.UserId.Value;

        var patient = await _patientRepo.GetByUserIdAsync(userId, cancellationToken);
        if (patient is null)
            return Error.NotFound("patient.profile_not_found", "No patient profile linked to this user.");

        var group = await _groupRepo.GetByIdAsync(request.GroupId, cancellationToken);
        if (group is null || !group.Active)
            return Error.NotFound("group.not_found", "Group not found.");

        var isMember = await _groupMemberRepo.IsActiveMemberAsync(request.GroupId, userId, cancellationToken);
        if (!isMember)
            return Error.Forbidden("You are not a member of this group.");

        var post = CommunityPost.Create(
            authorId: userId,
            content: request.Content,
            visibility: request.Visibility ?? "public",
            patientId: patient.Id,
            diseaseId: group.DiseaseId);

        await _postRepo.AddAsync(post, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new CreatePostResult(post.Id);
    }
}
