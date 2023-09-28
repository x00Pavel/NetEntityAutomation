using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Extensions.CoverEntity;
using NetEntityAutomation.Extensions.SunEntity;
namespace NetEntityAutomation.Automations.BlindsAutomation;

public class BlindAutomation: BaseAutomation
{
    public ICoverEntityCore Blind { get; set; }
    public SunEntity Sun { get; }
    private new IBlindAutomationConfig Config => (IBlindAutomationConfig) base.Config;
        public BlindAutomation(IHaContext ha, IBlindAutomationConfig config, ILogger logger) : base(logger, config, ha )
    {
        Blind = Config.Blind;
        Sun = new SunEntity(ha, "sun.sun");
        Sun.SunAboveHorizon.Subscribe(_ => Blind.OpenCover());
        Sun.SunBelowHorizon.Subscribe(_ => Blind.CloseCover());
    }

    protected override void InitFsmTransitions()
    {
    }
}