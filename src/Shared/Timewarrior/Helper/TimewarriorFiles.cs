using Shared.Models;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace Shared.Timewarrior.Helper;

/// <summary>
/// Help with exploring and validating relevant file space.
/// </summary>
internal static class TimewarriorFiles
{
    public static string HomeDir() => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public static DirectoryInfo TimeDir()
    {
        DirectoryInfo dir = new(HomeDir() + @"/.timewarrior");
        if (!dir.Exists) dir.Create();
        return dir;
    }
    public static DirectoryInfo DataDir()
    {
        DirectoryInfo dir = new(HomeDir() + @"/.timewarrior/data");
        if (!dir.Exists) dir.Create();
        return dir;
    }
    public static DirectoryInfo AppDir()
    {
        DirectoryInfo dir = new(HomeDir() + @"/.timewarrior/web");
        if (!dir.Exists) dir.Create();
        return dir;
    }

    /*
    public static DirectoryInfo AltDataDir()
    {
        DirectoryInfo dir = new(@$"{HomeDir()}/.timewarrior/web/data");
        if (!dir.Exists) dir.Create();
        return dir;
    }
    */

    public static bool VerifyData(DirectoryInfo directory)
    {
        var files = directory.EnumerateFiles();
        if (!files.Any()) return false;

        Regex DataNameExpression = new Regex("[0-9]{4}-[0-1][0-9].data");

        var tags = files.FirstOrDefault(f => f.Name == "tags.data");
        var undo = files.FirstOrDefault(f => f.Name == "undo.data");
        List<FileInfo> dataFiles = files.Where(f => DataNameExpression.IsMatch(f.Name)).ToList();

        if (tags == null || undo == null || !dataFiles.Any()) return false;

        if (dataFiles.Where(d => DeserializeMonthData(d) == null).Any()) return false;

        if (DeserializeTagData(tags) == null) return false;
        if (ValidateUndoData(undo) != true) return false;
        return true;
    }
    public static Preview GeneratePreview(DirectoryInfo dir)
    {
        var files = dir.EnumerateFiles();
        Regex DataNameExpression = new Regex("[0-9]{4}-[0-1][0-9].data");
        var tags = files.FirstOrDefault(f => f.Name == "tags.data");
        var undo = files.FirstOrDefault(f => f.Name == "undo.data");

        DateTime? latest = null;

        List<FileInfo>? dataFiles = files
            .Where(f => DataNameExpression.IsMatch(f.Name))
            .Where(f => f.Length > 4)
            .OrderByDescending(f => f.LastWriteTime)
            .ToList();

        if (dataFiles != null && dataFiles.Count != 0)
        {
            List<IntervalMonthData> intervals = DeserializeMonthData(dataFiles[0])
                .OrderByDescending(i => i.Start)
                .ToList();

            if (intervals != null && intervals.Count != 0) latest = intervals[0].Start;
        }

        List<string>? topTags = null;

        if (tags != null)
        {
            List<TagData> tagData = DeserializeTagData(tags);
            topTags = tagData.OrderByDescending(t => t.Count).Select(t => t.Name).Take(5).ToList();
        }

        return new()
        {
            Latest = latest,
            TopTags = topTags
        };
    }
    public static List<IntervalMonthData> DeserializeMonthData(FileInfo file)
    {
        // Regex IntervalDataExpression = new Regex("inc [0-9]{8}T[0-9]{6}Z - [0-9]{8}T[0-9]{6}Z($| #( #| \"[^#.]*\")($| \".*\"$))");
        // if (!IntervalDataExpression.IsMatch(data.OpenText().ReadToEnd())) return false;

        List<string> fileContents = new();

        using (StreamReader sr = file.OpenText())
        {
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (line != null) fileContents.Add(line);
            }
        }

        if (!fileContents.Any()) return new();

        List<IntervalMonthData> data = new();

        foreach (var line in fileContents)
        {
            var split = line.Split("#");
            IntervalMonthData datum = new IntervalMonthData();

            datum.Start = TimewarriorHelper.ObjectifyDate(split[0].Trim().Substring(4, 16));

            if (split[0].Contains("-")) datum.End = TimewarriorHelper.ObjectifyDate(split[0].Trim().Substring(23, 16));

            if (split.Length > 1)
            {
                var tags = split[1].Trim().Split(" ");

                var fullTags = new List<string>();
                var tagCache = "";
                bool building = false;

                foreach (var tagSegment in tags)
                {

                    if (tagSegment.StartsWith("\""))
                    {
                        building = true;
                        tagCache = tagSegment.Substring(1);
                    }
                    else if (tagSegment.EndsWith("\""))
                    {
                        building = false;
                        fullTags.Add(tagCache + " " + tagSegment.Replace("\"", ""));
                    }
                    else
                    {
                        if (building)
                        {
                            tagCache = tagCache + " " + tagSegment;
                        }
                        else
                        {
                            fullTags.Add(tagSegment);
                        }
                    }
                }
            }

            if (split.Length > 2) datum.Annotation = split[2].Trim().Replace("\"", "");

            data.Add(datum);
        }
        return data;
    }
    private static List<TagData> DeserializeTagData(FileInfo file)
    {
        var tagDataRaw = JsonSerializer.Deserialize<Dictionary<string, Tuple<string, int>>>(file.OpenText().ReadToEnd());

        if (tagDataRaw == null) return new();

        return tagDataRaw.Select(t => new TagData() { Name = t.Key, Count = t.Value.Item2 }).ToList();
    }
    private static bool ValidateUndoData(FileInfo file)
    {
        using (StreamReader sr = file.OpenText())
        {
            var firstLine = sr.ReadLine();
            if (firstLine != "txn:") return false;

            string? line;
            string prevLine = firstLine;

            while ((line = sr.ReadLine()) != null)
            {
                if (prevLine.StartsWith("txn:"))
                {
                    if (line != "  type: interval") return false;
                }

                if (prevLine == "  type: interval")
                {
                    if (line != "  before: ")
                    {
                        if (!line.StartsWith("  before:")) return false;

                        bool valid = ValidateUndoInterval(line.Substring(9));
                        if (!valid) return false;
                    }
                }

                if (prevLine.StartsWith("  before:"))
                {
                    if (!(line == "  after: "))
                    {
                        if (!line.StartsWith("  after:")) return false;

                        bool valid = ValidateUndoInterval(line.Substring(9));
                        if (!valid) return false;
                    }
                }

                if (prevLine.StartsWith("  after:"))
                {
                    if (line != "txn:")
                    {
                        if (line != "  type: interval") return false;
                    }
                }
                prevLine = line;
            }
        }

        return true;
    }
    private static bool ValidateUndoInterval(string intervalString)
    {
        if (intervalString == "") return true;

        var deserializeOpts = new JsonSerializerOptions
        {
            Converters = { new DateTimeJsonConverter() },
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        IntervalUndoData? data = JsonSerializer.Deserialize<IntervalUndoData>(intervalString, deserializeOpts);

        if (data == null) return false;

        return true;
    }
}

// some deserializing typing

// monthly data file does not provide contain id
public class IntervalMonthData
{
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public List<string> Tags { get; set; }
    public string Annotation { get; set; }

};

// optional id for undo.data file, doesn't use id on otherwise id=1 "active" interval
public class IntervalUndoData : IntervalMonthData
{
    public int? Id { get; set; }
    public override string ToString()
    {
        string tags = Tags == null ? "No tags." : string.Join(", ", Tags);
        string annotation = Annotation ?? "No annotation.";
        return $"Id: {Id}\nStart: {Start}\nEnd: {End}\nTags: {tags}\nAnnotation: {annotation}\n";
    }
};

public class TagData
{
    public string Name { get; set; }
    public int Count { get; set; }
}



