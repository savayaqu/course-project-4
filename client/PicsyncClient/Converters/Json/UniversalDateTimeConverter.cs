using System.Text.Json.Serialization;
using System.Text.Json;
using System.Globalization;

namespace PicsyncClient.Converters.Json;

public class UniversalDateTimeConverter : JsonConverter<DateTime>
{
    private static readonly string[] Formats =
    [
        "yyyy-MM-ddTHH:mm:ss.fffffffzzz", // Пример: 2025-01-20T15:52:35.6397813+07:00
        "yyyy-MM-ddTHH:mm:ss.fffffffZ",   // Пример: 2025-01-20T15:52:35.6397813Z
        "yyyy-MM-ddTHH:mm:ss.fffffff",    // Пример: 2025-01-20T15:52:35.6397813
        "yyyy-MM-ddTHH:mm:ss.ffffffzzz",  // Пример: 2025-01-20T15:52:35.639781+07:00
        "yyyy-MM-ddTHH:mm:ss.ffffffZ",    // Пример: 2025-01-20T15:52:35.639781Z
        "yyyy-MM-ddTHH:mm:ss.ffffff",     // Пример: 2025-01-20T15:52:35.639781
        "yyyy-MM-ddTHH:mm:ss.fffffzzz",   // Пример: 2025-01-20T15:52:35.63978+07:00
        "yyyy-MM-ddTHH:mm:ss.fffffZ",     // Пример: 2025-01-20T15:52:35.63978Z
        "yyyy-MM-ddTHH:mm:ss.ffffzzz",    // Пример: 2025-01-20T15:52:35.6397+07:00
        "yyyy-MM-ddTHH:mm:ss.ffffZ",      // Пример: 2025-01-20T15:52:35.6397Z
        "yyyy-MM-ddTHH:mm:ss.ffff",       // Пример: 2025-01-20T15:52:35.6397
        "yyyy-MM-ddTHH:mm:ss.fffzzz",     // Пример: 2025-01-20T15:52:35.639+07:00
        "yyyy-MM-ddTHH:mm:ss.fffZ",       // Пример: 2025-01-20T15:52:35.639Z
        "yyyy-MM-ddTHH:mm:ss.fff",        // Пример: 2025-01-20T15:52:35.639
        "yyyy-MM-ddTHH:mm:sszzz",         // Пример: 2025-01-20T15:52:35+07:00
        "yyyy-MM-ddTHH:mm:ssZ",           // Пример: 2025-01-20T15:52:35Z
        "yyyy-MM-ddTHH:mm:ss",            // Пример: 2025-01-20T15:52:35
        "yyyy-MM-dd HH:mm:ss",            // Пример: 2025-01-20 15:52:35
        "yyyy-MM-dd"                      // Пример: 2025-01-20
    ];

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString(); 
        
        if (string.IsNullOrEmpty(dateString))
            throw new JsonException("The date field is required and cannot be null or empty.");

        if (DateTime.TryParseExact(dateString, Formats, null, DateTimeStyles.None, out var date))
            return date;

        throw new JsonException($"Invalid date format: {dateString}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}

public class UniversalNullableDateTimeConverter : JsonConverter<DateTime?>
{
    private static readonly string[] Formats =
    {
        "yyyy-MM-ddTHH:mm:ss.fffffffzzz", // Пример: 2025-01-20T15:52:35.6397813+07:00
        "yyyy-MM-ddTHH:mm:ss.fffffffZ",   // Пример: 2025-01-20T15:52:35.6397813Z
        "yyyy-MM-ddTHH:mm:ss.ffffffzzz",  // Пример: 2025-01-20T15:52:35.639781+07:00
        "yyyy-MM-ddTHH:mm:ss.ffffffZ",    // Пример: 2025-01-20T15:52:35.639781Z
        "yyyy-MM-ddTHH:mm:ss.fffffzzz",   // Пример: 2025-01-20T15:52:35.63978+07:00
        "yyyy-MM-ddTHH:mm:ss.fffffZ",     // Пример: 2025-01-20T15:52:35.63978Z
        "yyyy-MM-ddTHH:mm:ss.ffffzzz",    // Пример: 2025-01-20T15:52:35.6397+07:00
        "yyyy-MM-ddTHH:mm:ss.ffffZ",      // Пример: 2025-01-20T15:52:35.6397Z
        "yyyy-MM-ddTHH:mm:ss.fffzzz",     // Пример: 2025-01-20T15:52:35.639+07:00
        "yyyy-MM-ddTHH:mm:ss.fffZ",       // Пример: 2025-01-20T15:52:35.639Z
        "yyyy-MM-ddTHH:mm:sszzz",         // Пример: 2025-01-20T15:52:35+07:00
        "yyyy-MM-ddTHH:mm:ssZ",           // Пример: 2025-01-20T15:52:35Z
        "yyyy-MM-ddTHH:mm:ss",            // Пример: 2025-01-20T15:52:35
        "yyyy-MM-dd HH:mm:ss",            // Пример: 2025-01-20 15:52:35
        "yyyy-MM-dd"                      // Пример: 2025-01-20
    };

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();

        if (string.IsNullOrEmpty(dateString))
            return null;

        if (DateTime.TryParseExact(dateString, Formats, null, DateTimeStyles.None, out var date))
            return date;

        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}