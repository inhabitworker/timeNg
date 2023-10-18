using System.Timers;

namespace Shared.Models;

public class Notification : IDisposable
{
    private System.Timers.Timer Decay;

    public string Message { get; set; }
    public bool IsError { get; set; }
    public int Duration { get; set; }
    public int Remaining { get; set; }

    public Notification(string message, bool isError, int seconds)
    {
        if (seconds < 1) throw new Exception("Notification duration must be integer over 0.");

        Message = message;
        IsError = isError;
        Duration = seconds;
        Remaining = seconds;
        
        Decay = new System.Timers.Timer(1000);
        Decay.Elapsed += Tick;
        Decay.Enabled = true;
        Decay.AutoReset = true;
        Decay.Start();
    }

    private void Tick(object? sender, ElapsedEventArgs args)
    {
        Remaining -= 1;

        if (Remaining == 0)
        {
            OnExpire?.Invoke();
        }
        else
        {
            OnTick?.Invoke();
        }
    }

    public void Close()
    {
        OnExpire?.Invoke();
    }

    public event Action OnExpire;
    public event Action OnTick;

    public void Dispose()
    {
        Decay.Stop();
        Decay.Enabled = false;
        Decay.Elapsed -= Tick;
        Decay.Dispose();
    }

}

