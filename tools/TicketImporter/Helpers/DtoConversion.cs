using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Primitives;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Raven.Yabt.TicketImporter.Helpers
{
	internal static class DtoConversion
	{
		/// <summary>
		///		Convert a DTO to a flatten out dictionary
		/// </summary>
		public static Dictionary<string, StringValues> ToDictionary<T>(T dto)
		{
			var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
			jsonSettings.Converters.Add(new StringEnumConverter());
			var json = JsonConvert.SerializeObject(dto, jsonSettings);

			var jsonObject = JObject.Parse(json);
			var jTokens = jsonObject.Descendants().Where(p => !p.Any());

			var results = jTokens.Aggregate(
				new Dictionary<string, StringValues>(),
				(pairs, jToken) =>
				{
					if (jToken is JValue jValue)
					{
						var value = jValue.Value?.ToString()?.ToLower();

						if (!string.IsNullOrEmpty(value))
							pairs.Add(jToken.Path.ToLower(), new StringValues(value));
					}
					return pairs;
				});
			return results;
		}
	}
}
