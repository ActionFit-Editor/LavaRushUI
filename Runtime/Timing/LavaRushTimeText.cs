using System;

namespace ActionFit.LavaRush.UI
{
    public static class LavaRushTimeText
    {
        public static string FormatHourMinSec(TimeSpan remaining)
        {
            return $"{(int)remaining.TotalHours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }

        internal static string FormatDefault(TimeSpan remaining)
        {
            return (int)remaining.TotalDays > 0
                ? $"{(int)remaining.TotalDays}D {remaining.Hours:00}:{remaining.Minutes:00}"
                : FormatHourMinSec(remaining);
        }
    }
}
