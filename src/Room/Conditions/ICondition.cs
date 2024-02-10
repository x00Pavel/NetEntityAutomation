namespace NetEntityAutomation.Room.Conditions;

public interface ICondition
{
    public bool IsTrue();
}

public class DefaultCondition: ICondition
{
    public bool IsTrue() => true;
}
