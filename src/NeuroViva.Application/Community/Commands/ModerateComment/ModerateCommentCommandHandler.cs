using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Community.Repositories;

namespace NeuroViva.Application.Community.Commands.ModerateComment;

public sealed class ModerateCommentCommandHandler
    : IRequestHandler<ModerateCommentCommand, Result<ModerateCommentResult>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICommunityCommentRepository _commentRepo;
    private readonly IUnitOfWork _uow;

    public ModerateCommentCommandHandler(
        ICurrentUserService currentUser,
        ICommunityCommentRepository commentRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _commentRepo = commentRepo;
        _uow = uow;
    }

    public async Task<Result<ModerateCommentResult>> Handle(
        ModerateCommentCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var comment = await _commentRepo.GetByIdAsync(request.CommentId, cancellationToken);
        if (comment is null)
            return Error.NotFound("comment.not_found", "Comment not found.");

        if (comment.Removed)
            return new ModerateCommentResult();

        comment.Moderate(request.Reason);
        _commentRepo.Update(comment);
        await _uow.SaveChangesAsync(cancellationToken);

        return new ModerateCommentResult();
    }
}
