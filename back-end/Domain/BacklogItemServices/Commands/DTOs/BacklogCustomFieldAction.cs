using System.Buffers;
using System.Text.Json;

using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;

public class BacklogCustomFieldAction
{
	public string CustomFieldId { get; init; } = null!;

	public JsonElement? Value  { get; init; }
		
	public object ObjValue 
	{ 
		init => Value = JsonElementFromObject(value);
	}
	
	public ListActionType ActionType  { get; init; }

	/// <summary>
	///		Deserialize a <see cref="JsonElement"/> to an object of the specified type <typeparam name="T" />  
	/// </summary>
	/// <remarks> Taken from https://stackoverflow.com/a/61047681/968003 </remarks>
	public object? GetValue<T>()
	{
		if (Value is null) return null;
			
		var bufferWriter = new ArrayBufferWriter<byte>();
		using (var writer = new Utf8JsonWriter(bufferWriter))
		{
			Value.Value.WriteTo(writer);
		}

		return JsonSerializer.Deserialize<T>(bufferWriter.WrittenSpan);
	}
		
	/// <summary>
	///		Converts an object to <see cref="System.Text.Json.JsonElement"/>
	/// </summary>
	/// <remarks>
	///		See https://stackoverflow.com/a/67003925/968003.
	///		Note that in .NET 6 they've added SerializeToElement() method that would simplify it a lot
	/// </remarks>
	private static JsonElement JsonElementFromObject(object value)
	{
		var jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(value, new JsonSerializerOptions());
		using var doc = JsonDocument.Parse(jsonUtf8Bytes);
		return doc.RootElement.Clone();
	}
}