namespace Shared.Timewarrior;

public class TimewarriorServiceMock : TimewarriorService
{
    public TimewarriorServiceMock(ILogger<ITimewarriorService> logger) : base(logger)
    {
    }

    public override async Task<string> Execute(string cmd)
    {
        string overrides = @" :yes";
        string esc = cmd.Replace("\"", "\\\"");
        string command = $"-c \" timew {esc} {overrides}\"";

        var logString = command.Length > 40 ? command.Substring(0, 40) + $"... [Total length: {command.Length}]" : command;
        _logger.LogInformation("Running: {0}", logString);

        await Task.Delay(200);

        return "Mock output.";
    }
}
