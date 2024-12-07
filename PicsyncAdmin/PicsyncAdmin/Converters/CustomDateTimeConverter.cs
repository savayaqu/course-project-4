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
        "yyyy-MM-ddTHH:mm:ssZ",        // ISO 8601 формат с Z (UTC)
        "yyyy-MM-ddTHH:mm:ss.fffZ",     // Формат с миллисекундами и Z
        "yyyy-MM-ddTHH:mm:ss.ffffffZ",  // Формат с микросекундами и Z
        "yyyy-MM-dd HH:mm:ss",          // Формат с пробелом между датой и временем
        "yyyy/MM/dd HH:mm:ss",          // Еще один возможный формат
    };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string dateString = reader.GetString();

            foreach (var format in _formats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
                {
                    return date;
                }
            }

            // Если ни один формат не подошел, выбрасываем исключение с дополнительной информацией
            throw new JsonException($"Unable to parse date: {dateString}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }
    }
}
