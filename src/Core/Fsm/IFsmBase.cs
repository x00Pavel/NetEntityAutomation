using Microsoft.Extensions.Logging;
using NetEntityAutomation.Extensions.Events;
using NetEntityAutomation.Core.Configs;
using Newtonsoft.Json;
using Stateless;

namespace NetEntityAutomation.Core.Fsm;

public interface IFsmBase
{
    public void FireAllOff();
}

public static class FsmBaseExtensionMethods
{
    public static void FireAllOff(this IEnumerable<IFsmBase> state)
    {
        foreach (var fsm in state)
        {
            fsm.FireAllOff();
        }
    }
}

public abstract class FsmBase<TState, TTrigger>(AutomationConfig config, ILogger logger): IFsmBase
{
    private AutomationConfig Config { get; set; } = config;
    protected StateMachine<TState, TTrigger> _fsm;
    protected ILogger Logger = logger;
    public CustomTimer Timer;
    public TState State => _fsm.State;
    protected TState DefaultState;
    protected string StoragePath;
    public bool IsEnabled { get; set; } = true;
    protected record JsonStorageSchema(TState State);

    protected void CreateFsm()
    {
        _fsm = new StateMachine<TState, TTrigger>(GetStateFromStorage, StoreState);
    }
    
    private void StoreState(TState state)
    {
        Logger.LogDebug("Storing state in storage ({Path}) {State}", StoragePath, state);
        File.WriteAllText(StoragePath, "{\"State\": " + JsonConvert.SerializeObject(state) + "}");
    }

    private TState GetStateFromStorage()
    {
        Logger.LogInformation("Getting state from storage ({Path})", StoragePath);
        if (!File.Exists(StoragePath))
        {
            Logger.LogDebug("Storage file does not exist, creating new one");
            File.Create(StoragePath).Dispose();
            File.WriteAllText(StoragePath, "{\"State\": " + JsonConvert.SerializeObject(DefaultState) + "}");
            return DefaultState;
        }

        var content = File.ReadAllText(StoragePath);
        var jsonContent = JsonConvert.DeserializeObject<JsonStorageSchema>(content);
        if (jsonContent != null)
        {
            Logger.LogDebug("Storage file content: {Content}", jsonContent);
            return jsonContent.State;
        }

        Logger.LogError("Could not deserialize storage file content");
        return DefaultState;
    }

    public abstract void FireAllOff();
}