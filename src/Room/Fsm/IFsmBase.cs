using Microsoft.Extensions.Logging;
using NetEntityAutomation.Extensions.Events;
using Newtonsoft.Json;
using Stateless;

namespace NetEntityAutomation.Room.Core;

public abstract class IFsmBase<TState, TTrigger>{
    private AutomationConfig Config { get; set; }
    protected StateMachine<TState, TTrigger> _fsm;
    protected ILogger Logger;
    public CustomTimer Timer;
    public TState State => _fsm.State;
    public TState DefaultState { get; init; }
    protected string StoragePath;
    public bool IsEnabled { get; set; } = true;
    

    protected record JsonStorageSchema(TState State);
    protected IFsmBase(AutomationConfig config, ILogger logger)
    {
        Config = config;
        Logger = logger;
        
    }

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
}