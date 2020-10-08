namespace Raven.Yabt.Database.Common
{
	public enum BacklogItemType
	{
		Unknown		= 0xFF,
		Feature		= 2^1,
		Bug			= 2^2,
		UserStory	= 2^3,
		Task		= 2^4
	}
}
