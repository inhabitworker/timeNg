using Shared.Models;
using Shared.Timewarrior.Helper;
using System.Diagnostics;

namespace Shared.Timewarrior;

/// <summary>
/// Timewarrior support, interacting through shell.
/// </summary>
public class TimewarriorService : ITimewarriorService
{
    protected ILogger<ITimewarriorService> _logger;

    public TimewarriorService(ILogger<ITimewarriorService> logger)
    {
        _logger = logger;
    }

    public virtual async Task<string> Execute(string cmd)
    {
        // string wsl = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "wsl" : "";
        // string overrides = @" :yes";

        cmd = cmd.Replace("\'", "\\\'");

        Process proc = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "bash",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                // WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = $" -c \"{cmd}\""
            },
            EnableRaisingEvents = true
        };

        var arg = proc.StartInfo.Arguments;
        var logString = arg.Length > 40 ? arg.Substring(0, 40) + $"... [Total length: {arg.Length}]" : arg;
        _logger.LogInformation("Running: {0}", logString);

        proc.Start();

        string output = proc.StandardOutput.ReadToEnd();
        string error = proc.StandardError.ReadToEnd();

        proc.WaitForExit();

        int exitCode = proc.ExitCode;

        if (exitCode != 0)
        {
            _logger.LogError(error);
            throw new Exception($"Error during execution: error");
        }
        else
        {
            if (output.StartsWith("Create new"))
            {
                // call again?
                proc.StandardInput.WriteLine("yes");
                _logger.LogInformation($"Timew created a new database.");
                return await Execute(cmd);
            }

            _logger.LogInformation(output);

            return output;
        }
    }

    /// <summary>
    /// Execute a batch of commands.
    /// </summary>
    /// <param name="cmds"></param>
    public async Task Execute(IEnumerable<string> cmds)
    {
        // max argument chars 32699
        // https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.arguments?view=net-7.0
        int maxLength = 30000;

        List<string> cmdList = new();
        List<string> superCmdList = new();

        foreach (var cmd in cmds)
        {
            cmdList.Add(cmd);
            if (string.Join(" && ", cmdList).Length > maxLength || cmd == cmds.Last())
            {
                superCmdList.Add(string.Join(" && ", cmdList));
                cmdList.Clear();
            }
        }

        foreach (var superCmd in superCmdList)
        {
            await Execute(superCmd);
        }
    }


    // Export
    public async Task<IEnumerable<IInterval>> Export()
    {
        string result = await Execute("timew export");
        return TimewarriorHelper.DeserializeExport(result);
    }
    public async Task<IInterval?> ExportSingle(int id)
    {
        string result = await Execute($"timew get dom.tracked.{id}.json");
        return TimewarriorHelper.DeserializeExport(result).ToList()[0];
    }

    // Commands
    public async Task Track(DateTime start, DateTime? end, List<string> tags)
    {
        string command = $"timew track {TimewarriorHelper.StringifyDate(start)} ";
        if (end != null) command += $"to {TimewarriorHelper.StringifyDate((DateTime)end)} ";
        if (tags.Count > 0) command += string.Join(" ", tags.Select(t => $"'{t}'"));
        await Execute(command);
    }

    public async Task Delete(int id) => await Execute($"timew delete @{id}");
    public async Task Delete(IEnumerable<int> ids) => await Execute(ids.Select(id => $"timew delete @{id}"));
    public async Task Annotate(int id, string annotation) => await Execute($"timew annotate @{id} '{annotation}'");
    public async Task Tag(int id, string tag) => await Execute($"timew tag @{id} '{tag}'");
    public async Task Tag(IEnumerable<int> ids, string tag) => await Execute(ids.Select(id => $"timew tag @{id} '{tag}'"));
    public async Task Untag(int id, string tag) => await Execute($"timew untag @{id} '{tag}'");
    public async Task Untag(IEnumerable<int> ids, string tag) => await Execute(ids.Select(id => $"timew untag @{id} '{tag}'"));
    public async Task Undo() => await Execute("timew undo");
    public async Task Seed(IEnumerable<IInterval> data)
    {
        foreach (FileInfo file in TimewarriorFiles.DataDir().EnumerateFiles()) file.Delete();
        foreach (DirectoryInfo directory in TimewarriorFiles.DataDir().EnumerateDirectories()) directory.Delete();

        var ordered = data
            .OrderByDescending(i => i.Start)
            .Select(i => i.Start)
            .ToList();

        List<string> trackList = new();
        List<string> annotateList = new();

        foreach (var interval in data)
        {
            string command = $"timew track {TimewarriorHelper.StringifyDate(interval.Start)} ";
            if (interval.End != null) command += $"to {TimewarriorHelper.StringifyDate((DateTime)interval.End)} ";
            if (interval.Tags.Count > 0) command += string.Join(" ", interval.Tags.Select(t => "'t'"));
            trackList.Add(command);

            if (interval.Annotation != "")
            {
                var id = ordered.FindIndex(d => DateTime.Compare(d, interval.Start) < 1) + 1;
                annotateList.Add($"timew annotate @{id} '{interval.Annotation}'");
            }
        }

        var fullList = trackList.Concat(annotateList);
        await Execute(fullList.ToList());
    }
}


