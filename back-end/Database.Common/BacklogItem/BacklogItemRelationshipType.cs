using System;

namespace Raven.Yabt.Database.Common.BacklogItem;

public enum BacklogRelationshipType
{
	Duplicate,
	Related,
	BlockedBy,
	Blocks,
	CausedBy,
	Causes
}

public static class BacklogRelationshipTypeExtension
{
	public static BacklogRelationshipType GetMirroredType(this BacklogRelationshipType type)
		=> type switch
		{
			BacklogRelationshipType.Duplicate => BacklogRelationshipType.Duplicate,
			BacklogRelationshipType.Related => BacklogRelationshipType.Related,
			BacklogRelationshipType.BlockedBy => BacklogRelationshipType.Blocks,
			BacklogRelationshipType.Blocks => BacklogRelationshipType.BlockedBy,
			BacklogRelationshipType.Causes => BacklogRelationshipType.CausedBy,
			BacklogRelationshipType.CausedBy => BacklogRelationshipType.Causes,
			_ => throw new NotImplementedException($"Type {type} not supported"),
		};
}