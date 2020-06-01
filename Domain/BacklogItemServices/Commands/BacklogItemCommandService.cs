using System;
using System.Linq;
using System.Threading.Tasks;

using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Models.BacklogItem;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.UserServices;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands
{
	public class BacklogItemCommandService : BaseService<BacklogItem>, IBacklogItemCommandService
	{
		private readonly IUserReferenceResolver _userResolver;

		public BacklogItemCommandService(IAsyncDocumentSession dbSession, IUserReferenceResolver userResolver) : base(dbSession)
		{
			_userResolver = userResolver;
		}

		public async Task<BacklogItemReference> Create<T>(T dto) where T : BacklogItemAddUpdRequest
		{
			BacklogItem ticket = dto switch
			{
				BugAddUpdRequest bug		 => await ConvertDtoToEntity<BacklogItemBug, BugAddUpdRequest>(bug),
				UserStoryAddUpdRequest story => await ConvertDtoToEntity<BacklogItemUserStory, UserStoryAddUpdRequest>(story),
				_ => throw new ArgumentException("Incorrect type", nameof(dto)),
			};

			await DbSession.StoreAsync(ticket);

			return ticket.GetReference();
		}

		public async Task<BacklogItemReference?> Delete(string id)
		{
			var ticket = await DbSession.LoadAsync<BacklogItem>(GetFullId(id));
			if (ticket == null)
				return null;

			DbSession.Delete(ticket);

			return ticket.GetReference();
		}

		public async Task<BacklogItemReference?> Update<T>(string id, T dto) where T : BacklogItemAddUpdRequest
		{
			if (dto == null)
				throw new ArgumentNullException(nameof(dto));

			var entity = await DbSession.LoadAsync<BacklogItem>(GetFullId(id));
			if (entity == null)
				return null;

			entity = dto switch
			{
				BugAddUpdRequest bug			=> await ConvertDtoToEntity (bug,	entity as BacklogItemBug),
				UserStoryAddUpdRequest story	=> await ConvertDtoToEntity (story,	entity as BacklogItemUserStory),
				_ => throw new ArgumentException("Incorrect type", nameof(dto)),
			};

			return entity.GetReference();
		}

		public async Task<BacklogItemReference?> AssignToUser(string backlogItemId, string userId)
		{
			var backlogItem = await DbSession.LoadAsync<BacklogItem>(GetFullId(backlogItemId));
			if (backlogItem == null)
				return null;

			if (userId == null)
			{
				backlogItem.Assignee = null;
			}
			else
			{
				var userRef = await _userResolver.GetReferenceById(userId);
				if (userRef == null)
					return null;

				backlogItem.Assignee = userRef;
			}

			backlogItem.Modifications.Add(new BacklogItemHistoryRecord
				{
					ActionedBy = await _userResolver.GetCurrentUserReference(),
					Summary = "Assigned user"
				});

			return backlogItem.GetReference();
		}

		private async Task<TModel> ConvertDtoToEntity<TModel, TDto>(TDto dto, TModel? entity = null)
			where TModel : BacklogItem, new()
			where TDto : BacklogItemAddUpdRequest
		{
			if (entity == null)
				entity = new TModel();

			entity.Title = dto.Title;
			entity.Assignee = dto.AssigneeId != null ? await _userResolver.GetReferenceById(dto.AssigneeId) : null;
			entity.Modifications.Add(new BacklogItemHistoryRecord
				{
					ActionedBy = await _userResolver.GetCurrentUserReference(),
					Summary = entity.Modifications?.Any() == true ? "Modified" : "Created"
				});

			if (dto.CustomFields != null)
				entity.CustomFields = dto.CustomFields;
			else
				entity.CustomFields.Clear();

			if (dto.RelatedItems != null)
				entity.RelatedItems = dto.RelatedItems;
			else
				entity.RelatedItems.Clear();

			// entity.CustomProperties = dto.CustomProperties;	TODO: De-serialise custom properties

			if (dto is BugAddUpdRequest bugDto && entity is BacklogItemBug bugEntity)
			{
				bugEntity.Severity = bugDto.Severity;
				bugEntity.Priority = bugDto.Priority;
				bugEntity.StepsToReproduce = bugDto.StepsToReproduce;
			}
			else if (dto is UserStoryAddUpdRequest storyDto && entity is BacklogItemUserStory storyEntity)
			{
				storyEntity.AcceptanceCriteria = storyDto.AcceptanceCriteria;
			}

			return entity;
		}
	}
}
