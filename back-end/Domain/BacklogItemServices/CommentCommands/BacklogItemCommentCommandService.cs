using System;
using System.Linq;
using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.UserServices.Query;

namespace Raven.Yabt.Domain.BacklogItemServices.CommentCommands;

public class BacklogItemCommentCommandService : BaseService<BacklogItem>, IBacklogItemCommentCommandService
{
	private readonly IUserReferenceResolver _userResolver;
	private readonly IMentionedUserResolver _mentionedUserResolver;

	public BacklogItemCommentCommandService(IAsyncTenantedDocumentSession dbSession, 
	                                        IUserReferenceResolver userResolver,
	                                        IMentionedUserResolver mentionedUserResolver) : base(dbSession)
	{
		_userResolver = userResolver;
		_mentionedUserResolver = mentionedUserResolver;
	}

	public async Task<IDomainResult<BacklogItemCommentReference>> Create(string backlogItemId, string message)
	{
		var ticketRes = await GetEntity(backlogItemId);
		if (!ticketRes.IsSuccess)
			return ticketRes.To<BacklogItemCommentReference>();
		var ticket = ticketRes.Value;

		var mentionedUsers = await _mentionedUserResolver.GetMentionedUsers(message);
		var currentUser = await _userResolver.GetCurrentUserReference();
			
		var comment = new Comment
		{
			Author = currentUser,
			Message = message,
			MentionedUserIds = mentionedUsers.Any() ? mentionedUsers : null,
		};
		ticket.Comments.Add(comment);

		ticket.AddHistoryRecord(currentUser, "Added a comment");
			
		return DomainResult.Success(GetCommentReference(ticket.Id, comment.Id, message));
	}

	public async Task<IDomainResult<BacklogItemCommentReference>> Update(string backlogItemId, string commentId, string message)
	{
		var ticketRes = await GetEntity(backlogItemId);
		if (!ticketRes.IsSuccess)
			return ticketRes.To<BacklogItemCommentReference>();
		var ticket = ticketRes.Value;

		var comment = ticket.Comments.SingleOrDefault(c => c.Id == commentId);
		if (comment == null)
			return DomainResult.NotFound<BacklogItemCommentReference>("Comment not found");

		var currentUser = await _userResolver.GetCurrentUserReference();
		if (comment.Author.Id != currentUser.Id)
			return DomainResult.Unauthorized<BacklogItemCommentReference>("Cannot edit comments of other users");

		var mentionedUsers = await _mentionedUserResolver.GetMentionedUsers(message);

		comment.Message = message;
		comment.MentionedUserIds = mentionedUsers.Any() ? mentionedUsers : null;
		comment.LastModified = DateTime.UtcNow;

		ticket.AddHistoryRecord(currentUser, "Updated a comment");

		return DomainResult.Success(GetCommentReference(ticket.Id, commentId, message));
	}

	public async Task<IDomainResult<BacklogItemCommentReference>> Delete(string backlogItemId, string commentId)
	{
		var ticketRes = await GetEntity(backlogItemId);
		if (!ticketRes.IsSuccess)
			return ticketRes.To<BacklogItemCommentReference>();
		var ticket = ticketRes.Value;

		var comment = ticket.Comments.SingleOrDefault(c => c.Id == commentId);
		if (comment == null)
			return DomainResult.NotFound<BacklogItemCommentReference>("Comment not found");

		var currentUser = await _userResolver.GetCurrentUserReference();
		if (comment.Author.Id != currentUser.Id)
			return DomainResult.Unauthorized<BacklogItemCommentReference>("Cannot delete comments of other users");

		ticket.Comments.Remove(comment);

		ticket.AddHistoryRecord(currentUser, "Deleted a comment");

		return DomainResult.Success(GetCommentReference(ticket.Id, null, comment.Message));
	}

	private static BacklogItemCommentReference GetCommentReference(string ticketId, string? commentId, string commentMessage) 
		=> new()
		{
			Id = ticketId.GetShortId(),
			Name = commentMessage.Length > 20
				? commentMessage.Substring(0, 17) + "..." 
				: commentMessage,
			CommentId = commentId,
		};

	private async Task<IDomainResult<BacklogItem>> GetEntity(string id)
	{
		var fullId = GetFullId(id);

		var ticket = await DbSession.LoadAsync<BacklogItem>(fullId);
		return ticket == null 
			? DomainResult.NotFound<BacklogItem>("Backlog item not found") 
			: DomainResult.Success(ticket);
	}
}