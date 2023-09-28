using System.Reactive;
using System.Reactive.Linq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Extensions.SunEntity;

public record SunEntity: Entity<SunAttributes>, ISunEntityCore
{
    public SunEntity(IHaContext haContext, string entityId) : base(haContext, entityId)
    {
    }

    public SunEntity(IEntityCore entity) : base(entity)
    {
    }
    
    public IObservable<Unit> SunAboveHorizon =>
        StateChanges()
            .Where(e => e.New?.State == "above_horizon")
            .Select(_ => Unit.Default);
    
    public IObservable<Unit> SunBelowHorizon =>
        StateChanges()
            .Where(e => e.New?.State == "below_horizon")
            .Select(_ => Unit.Default);
}