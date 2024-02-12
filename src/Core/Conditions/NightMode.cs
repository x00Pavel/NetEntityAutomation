namespace NetEntityAutomation.Core.Conditions;

public class NightMode : ICondition
{
    public bool IsEnabled { get; set; }
    public Func<TimeSpan> StopAtTimeFunc { get; }
    public Func<TimeSpan> StartAtTimeFunc { get; }

    public bool IsWorkingHours { get; }

    public bool IsTrue()
    {
        if (!IsEnabled)
        {
            return false;
        }

        return !IsWorkingHours;
    }
}