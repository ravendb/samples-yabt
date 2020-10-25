using System;
using System.Linq;
using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Domain.BacklogItemServices.CommentCommands.DTOs;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.UserServices.Query;

namespace Raven.Yabt.Domain.BacklogItemServices.CommentCommands
{
	public class BacklogItemCommentCommandService : BaseService<BacklogItem>, IBacklogItemCommentCommandService
	{
		private readonly IUserReferenceResolver _userResolver;
		private readonly IMentionedUserResolver _mentionedUserResolver;

		public BacklogItemCommentCommandService(IAsyncDocumentSession dbSession, 
		                                        IUserReferenceResolver userResolver,
		                                        IMentionedUserResolver mentionedUserResolver) : base(dbSession)
		{
			_userResolver = userResolver;
			_mentionedUserResolver = mentionedUserResolver;
		}

		public async Task<IDomainResult<BacklogItemCommentReference>> Create(string backlogItemId, CommentAddUpdRequest dto)
		{
			var fullId = GetFullId(backlogItemId);

			var ticket = await DbSession.LoadAsync<BacklogItem>(fullId);
			if (ticket == null)
				return DomainResult.NotFound<BacklogItemCommentReference>();

			var mentionedUsers = await _mentionedUserResolver.GetMentionedUsers(dto.Message);
			
			var comment = new Comment
				{
					Author = await _userResolver.GetCurrentUserReference(),
					Message = dto.Message,
					MentionedUserIds = mentionedUsers
				};
			ticket.Comments.Add(comment);

			return DomainResult.Success(ToLastCommentReference(ticket));
		}

		public async Task<IDomainResult<BacklogItemCommentReference>> Update(string backlogItemId, string commentId, CommentAddUpdRequest dto)
		{
			var fullId = GetFullId(backlogItemId);

			var ticket = await DbSession.LoadAsync<BacklogItem>(fullId);
			if (ticket == null)
				return DomainResult.NotFound<BacklogItemCommentReference>();

			var comment = ticket.Comments.SingleOrDefault(c => c.Id == commentId);
			if (comment == null)
				return DomainResult.NotFound<BacklogItemCommentReference>("Comment not found");

			var originalAuthor = await _userResolver.GetCurrentUserReference();
			if (comment.Author.Id != originalAuthor.Id)
				return DomainResult.Error<BacklogItemCommentReference>("Cannot edit comments of other users");

			var mentionedUsers = await _mentionedUserResolver.GetMentionedUsers(dto.Message);

			comment.Message = dto.Message;
			comment.MentionedUserIds = mentionedUsers;
			comment.ModifiedDate = DateTime.UtcNow;

			return DomainResult.Success(ToLastCommentReference(ticket));
		}

		public async Task<IDomainResult<BacklogItemCommentReference>> Delete(string backlogItemId, string commentId)
		{
			var fullId = GetFullId(backlogItemId);

			var ticket = await DbSession.LoadAsync<BacklogItem>(fullId);
			if (ticket == null)
				return DomainResult.NotFound<BacklogItemCommentReference>();

			var comment = ticket.Comments.SingleOrDefault(c => c.Id == commentId);
			if (comment == null)
				return DomainResult.NotFound<BacklogItemCommentReference>("Comment not found");

			ticket.Comments.Remove(comment);

			return DomainResult.Success(ToLastCommentReference(ticket, true));
		}

		private static BacklogItemCommentReference ToLastCommentReference(BacklogItem ticket, bool nullCommentId = false) => new BacklogItemCommentReference
		{
			Id = ticket.Id,
			Name = ticket.Title,
			CommentId = nullCommentId ? null : ticket.Comments.LastOrDefault()?.Id
		};
/*
		public async Task<IDomainResult<BacklogItemCommentReference>> Create(string backlogItemId, CommentAddRequest dto)
		{
			var ticketRes = await GetEntity(backlogItemId);
			if (!ticketRes.IsSuccess)
				return IDomainResult<BacklogItemCommentReference>.ReturnFrom(ticketRes);

			var comment = new Comment
				{
					Author = await _userResolver.GetCurrentUserReference(),
					Message = dto.Message
				};
			ticket.Comments.Add(comment);

			return DomainResult.Success(ToLastCommentReference(ticket));
		}

		private async Task<IDomainResult<BacklogItem>> GetEntity(string id)
		{
			var fullId = GetFullId(id);

			var ticket = await DbSession.LoadAsync<BacklogItem>(fullId);
			if (ticket == null)
				return DomainResult.NotFound<BacklogItem>("Backlog item not found");
			
			return DomainResult.Success(ticket);
		}
*/
	}
}
