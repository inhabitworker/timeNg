using Shared.Data;
using Shared.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Shared.Timewarrior.Helper;

internal static class TimewarriorHelper
{
    /// <summary>
    /// Takes the string date exported by timewarrior binary and parses it into a dotNet DateTime object.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static DateTime ObjectifyDate(string input)
    {
        string date = $"{input.Substring(0, 4)}-{input.Substring(4, 2)}-{input.Substring(6, 2)}";
        string time = $"{input.Substring(9, 2)}:{input.Substring(11, 2)}:{input.Substring(13, 2)}";

        return DateTime.Parse($"{date}T{time}Z").ToUniversalTime();
    }

    /// <summary>
    /// Stringify dotNet datetime object into a format accepted by the timewarrior binary.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string StringifyDate(DateTime input)
    {
        var str = input.ToString("o")
            .Replace("-", "")
            .Replace(":", "")
            .Substring(0, 15);

        return $"{str}Z";
    }

    /// <summary>
    /// Deserialize Timewarrior json data exports.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static IEnumerable<IInterval> DeserializeExport(string input)
    {
        string arrData = input.StartsWith("[") ? input : $"[{input}]";

        var deserializeOpts = new JsonSerializerOptions
        {
            Converters = { new DateTimeJsonConverter() },
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        List<Interval> data = JsonSerializer.Deserialize<List<Interval>>(arrData, deserializeOpts)!;
        data.Reverse();
        return data;
    }

    /// <summary>
    /// Get the timewarrior binary Id (which is 1-index by recency of start date) for an interval, using the unique start time.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="date"></param>
    /// <returns>Integer Id.</returns>
    public static int TimewId(IQueryService query, DateTime date)
    {
        var intervals = query.Intervals();

        return intervals.FindIndex(i => DateTime.Compare(i.Start, date) < 1) + 1;
    }

    /// <summary>
    /// Get the timewarrior binary Id (which is 1-index by recency of start date) for an interval, using its unique local Id.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="id"></param>
    /// <returns>An integer Id.</returns>
    public static int TimewId(TimeDbContext context, long id)
    {
        var intervals = context.Intervals
            .AsNoTracking()
            .OrderByDescending(i => i.Start)
            .ToList();

        return intervals.FindIndex(0, intervals.Count - 1, i => i.Id == id) + 1;
    }

}

/// <summary>
/// Json converter for Timew date/time format
/// </summary>
public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
            TimewarriorHelper.ObjectifyDate(reader.GetString()!);

    public override void Write(
            Utf8JsonWriter writer,
            DateTime dateTimeValue,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(TimewarriorHelper.StringifyDate(dateTimeValue));
}

