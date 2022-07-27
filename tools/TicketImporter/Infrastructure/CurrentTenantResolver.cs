using System;

using Raven.Yabt.Domain.Infrastructure;

namespace Raven.Yabt.TicketImporter.Infrastructure;

internal interface ICurrentTenantSetter
{
	void SetCurrentTenantId(string id);
}
	
public class CurrentTenantResolver : ICurrentTenantResolver, ICurrentTenantSetter
{
	private string? _currentTenantId;
		
	public string GetCurrentTenantId()
	{
		if (_currentTenantId == null)
			throw new Exception("Current tenant not set");
		return _currentTenantId;
	}

	public void SetCurrentTenantId(string id) => _currentTenantId = id;
}