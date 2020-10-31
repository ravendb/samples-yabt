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
			var ticketRes = await GetEntity(backlogItemId);
			if (!ticketRes.IsSuccess)
				return ticketRes.To<BacklogItemCommentReference>();
			var ticket = ticketRes.Value;

			var mentionedUsers = await _mentionedUserResolver.GetMentionedUsers(dto.Message);
			
			var comment = new Comment
				{
					Author = await _userResolver.GetCurrentUserReference(),
					Message = dto.Message,
					MentionedUserIds = mentionedUsers.Any() ? mentionedUsers : null,
				};
			ticket.Comments.Add(comment);

			return DomainResult.Success(ToLastCommentReference(ticket));
		}

		public async Task<IDomainResult<BacklogItemCommentReference>> Update(string backlogItemId, string commentId, CommentAddUpdRequest dto)
		{
			var ticketRes = await GetEntity(backlogItemId);
			if (!ticketRes.IsSuccess)
				return ticketRes.To<BacklogItemCommentReference>();
			var ticket = ticketRes.Value;

			var comment = ticket.Comments.SingleOrDefault(c => c.Id == commentId);
			if (comment == null)
				return DomainResult.NotFound<BacklogItemCommentReference>("Comment not found");

			var originalAuthor = await _userResolver.GetCurrentUserReference();
			if (comment.Author.Id != originalAuthor.Id)
				return DomainResult.Failed<BacklogItemCommentReference>("Cannot edit comments of other users");

			var mentionedUsers = await _mentionedUserResolver.GetMentionedUsers(dto.Message);

			comment.Message = dto.Message;
			comment.MentionedUserIds = mentionedUsers.Any() ? mentionedUsers : null;
			comment.ModifiedDate = DateTime.UtcNow;

			return DomainResult.Success(ToLastCommentReference(ticket));
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

			ticket.Comments.Remove(comment);

			return DomainResult.Success(ToLastCommentReference(ticket, true));
		}

		private static BacklogItemCommentReference ToLastCommentReference(BacklogItem ticket, bool nullCommentId = false) 
			=> new BacklogItemCommentReference
			{
				Id = ticket.Id,
				Name = ticket.Title,
				CommentId = nullCommentId ? null : ticket.Comments.LastOrDefault()?.Id,
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
}
