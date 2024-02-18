namespace NetEntityAutomation.Extensions.ExtensionMethods;

public static class UtilsMethods
{
    public static bool NowInTimeRange(TimeSpan startTime, TimeSpan endTime)
    {
        var now = DateTime.Now.TimeOfDay;
        if (startTime > endTime)
            return now >= startTime || now <= endTime;
        return now >= startTime && now <= endTime;
    }
    
    public static bool NowBeforeTime(TimeSpan endTime)
    {
        return DateTime.Now.TimeOfDay < endTime;
    }
    
    public static bool NowAfterTime(TimeSpan startTime)
    {
        return DateTime.Now.TimeOfDay > startTime;
    }
}