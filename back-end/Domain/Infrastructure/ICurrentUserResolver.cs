namespace Raven.Yabt.Domain.Infrastructure;

public interface ICurrentUserResolver
{
	string GetCurrentUserId();
}