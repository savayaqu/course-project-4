using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace PicsyncAdmin.Converters
{
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string[] _formats = new[]
        {
        "yyyy-MM-ddTHH:mm:ssZ",  // ISO 8601 format
        "yyyy-MM-dd HH:mm:ss",    // Format with a space between date and time
        "yyyy/MM/dd HH:mm:ss",    // Another potential format
        // Add other possible date formats here if needed
    };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string dateString = reader.GetString();
            foreach (var format in _formats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    return date;
                }
            }

            // If no format matches, throw an exception with more specific details
            throw new JsonException($"Unable to parse date: {dateString}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }
    }


}
