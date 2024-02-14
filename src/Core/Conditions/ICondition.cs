namespace NetEntityAutomation.Core.Conditions;

public interface ICondition
{
    public bool IsTrue();
}

public class DefaultCondition : ICondition
{
    public bool IsTrue() => true;
}

public class Condition(Func<bool> condition) : ICondition
{
    public bool IsTrue() => condition();
}