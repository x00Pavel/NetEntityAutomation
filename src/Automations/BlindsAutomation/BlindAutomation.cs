using NetDaemon.HassModel.Entities;

namespace BlindsAutomation;

public class BlindAutomation
{
    public ICoverEntityCore Blind { get; set; }
    public string? SwitchId { get; set; }

    public IEnumerable<IBinarySensorEntityCore?> MotionSensor { get; set; }

    
    public BlindAutomation(ICoverEntityCore blind, IEnumerable<IBinarySensorEntityCore?> motionSensor, string? switchId = null)
    {
        Blind = blind;
        MotionSensor = motionSensor;
        SwitchId = switchId;
    }
    
    public BlindAutomation(ICoverEntityCore blind, IBinarySensorEntityCore? motionSensor = null, string? switchId = null): 
        this(blind, new [] {motionSensor}, switchId) { }

    

    
    
}