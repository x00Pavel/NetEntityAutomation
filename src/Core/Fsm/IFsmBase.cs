using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;
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

public abstract class FsmBase<TState, TTrigger>(ILogger logger): IFsmBase
{
    protected StateMachine<TState, TTrigger> _fsm;
    protected ILogger Logger = logger;
    public CustomTimer Timer = new (logger);
    public TState State => _fsm.State;
    protected TState DefaultState;
    public bool IsEnabled { get; set; } = true;
    public IEntityCore Entity { get; init; }
    protected string StorageDir => $"storage/{GetType().Name}";
    protected string StoragePath { get; init; }

    protected record JsonStorageSchema(TState State);

    protected void InitFsm()
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
        if (!Directory.Exists(StorageDir))
        {
            Logger.LogDebug("Storage directory {Path} does not exist, creating new one", StorageDir);
            Directory.CreateDirectory(StorageDir);
        }
        if (!File.Exists(StoragePath))
        {
            Logger.LogDebug("Storage file {Path} does not exist, creating new one", StoragePath);
            File.Create(StoragePath).Dispose();
            File.WriteAllText(StoragePath, "{\"State\": " + JsonConvert.SerializeObject(DefaultState) + "}");
            return DefaultState;
        }

        var content = File.ReadAllText(StoragePath);
        var jsonContent = JsonConvert.DeserializeObject<JsonStorageSchema>(content);
        if (jsonContent != null)
        {
            Logger.LogDebug("{Path}: {Content}", StoragePath, jsonContent);
            return jsonContent.State;
        }

        Logger.LogError("Could not deserialize storage file content");
        return DefaultState;
    }

    public abstract void FireAllOff();
    
}