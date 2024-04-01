# Main Light Automation

```csharp
public class MainLightAutomationBase: AutomationBase<ILightEntityCore, MainLightFsmBase>
```

## Description

This automation aims to control the main light in the living room.
Controlling of the light means turning off the light after a specified time period if no motion at the moment of timeout.
This automation uses Finite State Machine to store the state of the light entity.

## Configuration

```csharp
var mainLight = new MainLightAutomationBase
        {
            EntitiesList = new List<ILightEntityCore>
            {
                lights.LivingroomBulbLight
            },
            Triggers = new []
            {
                new MotionSensor(new []
                {
                    sensors.LivingRoomMotionSensorMotion,
                    sensors.LivingRoomMotionSensor1Motion
                }, context)
            },
            WaitTime = TimeSpan.FromHours(1)
        };
```