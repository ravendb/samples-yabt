using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Primitives;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Raven.Yabt.TicketImporter.Helpers
{
	internal class DtoConvertion
	{
		/// <summary>
		///		Convert a DTO to a flatten out dictionary
		/// </summary>
		public static Dictionary<string, StringValues> ToDictionary<T>(T dto)
		{
			var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
				jsonSettings.Converters.Add(new StringEnumConverter());
			string json = JsonConvert.SerializeObject(dto, jsonSettings);

			JObject jsonObject = JObject.Parse(json);
			IEnumerable<JToken> jTokens = jsonObject.Descendants().Where(p => !p.Any());

			var results = jTokens.Aggregate(
				new Dictionary<string, StringValues>(),
				(pairs, jToken) =>
				{
					if (jToken is JValue jValue)
					{
						string? value;
						if (DateTime.TryParse(Convert.ToString(jValue.Value), out DateTime dateVal))
							value = dateVal.ToString("O");
						else if (jValue.Value is DateTime dateV)
							value = dateV.ToString("O");
						else
							value = jValue.Value?.ToString()?.ToLower();

						if (!string.IsNullOrEmpty(value))
							pairs.Add(jToken.Path.ToLower(), new StringValues(value));
					}
					return pairs;
				});
			return results;
		}

	}
}
