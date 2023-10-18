namespace Shared.Helpers;

public static class Time
{
    public static int ToUnixTimestamp(this DateTime date) => (int)(date.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds;
    public static int ToJsTimestamp(this DateTime date) => ToUnixTimestamp(date) * 1000;
    public static DateTime UnixTimestampToDateTime(this int epoch) => DateTime.UnixEpoch.AddSeconds(epoch);

    public static string DurationString(this TimeSpan span)
    {
        int hours = (int)span.TotalHours;
        string hourString = hours.ToString();

        if (hours < 10)
        {
            hourString = $"0{hours}";
        }

        return $"{hourString}:" + span.ToString(@"mm\:ss");
    }


    public static TimeSpan Duration(this IntervalDTO interval) => (interval.End ?? DateTime.Now) - interval.Start;

    public static TimeSpan Duration(IEnumerable<IntervalDTO> intervals)
    {
        TimeSpan result = TimeSpan.Zero;

        foreach (IntervalDTO interval in intervals)
        {
            result += ((interval.End ?? DateTime.Now) - interval.Start);
        }

        return result;
    }

    public static TimeSpan Duration(IEnumerable<IntervalDTO> intervals, DateTime day)
    {
        var date = day.Date;
        TimeSpan result = TimeSpan.Zero;

        foreach (IntervalDTO interval in intervals)
        {
            var start = interval.Start;
            var end = interval.End ?? DateTime.Now;

            if (start.Date == date && end.Date == date)
            {
                // intra day
                result += (end - start).Duration();
            }
            else if(start.Date != date && end.Date == date) 
            {
                // starts previous day, ends on day
                result += (end - date).Duration();
            }
            else if (start.Date == date && end.Date != date)
            {
                // starts on day, ends on proceeding day
                result += (date.AddDays(1) - start).Duration();
            }
            else if (start < date && end > date)
            {
                result = TimeSpan.FromDays(1);
            }
        }

        return result;  
    }

}
