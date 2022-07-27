namespace Raven.Yabt.Domain.Infrastructure;

public interface ICurrentTenantResolver
{
	string GetCurrentTenantId();
}