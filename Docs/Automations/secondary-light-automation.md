# Secondary Light Automation

```csharp
public class LightAutomationBase : AutomationBase<ILightEntityCore, LightFsmBase>
```

## Description

This automation aims to control the secondary light in the living room.
Main trigger for the automation is a motion sensor.
The secondary light is turned on when the motion sensor(s) trigger(s) an event `state_changed` in Home Assistant.
The light is turned off after a specified time period if no motion at the moment of timeout.

To track the automation flow and correct loading of the entities, the automation uses Finite State Machine for each of the light entities.
The FSM has the following states:

- `OnByMotion` - the light is turned on by the motion sensor
- `OnBySwitch` - the light is turned on manually
- `Off` - the light is turned off by the automation
- `OffBySwitch` - the light is turned off manually

and triggers

- `MotionOnTrigger`
- `MotionOffTrigger`
- `SwitchOnTrigger`
- `SwitchOffTrigger`
- `TimerElapsed`
- `AllOff`

State diagram: **[TODO]**

Single automation object can include several light entities that will be controlled.
When the user manually turn on the any light from the list, the automation will use extended timeout period before turning off the light.
Events from the motion sensor(s) aren't considered in this case.

When the light manually turned off, the FSM fot the related light entity will be set to `OffBySwitch` state.
In this state, the light will not be turned on by the motion sensor(s) until the user manually turn on the light again.
All FSM's, related to this automation, are moved to the `Off` state when ALL light are turned off manually or by automation (by `AllOff` FSM event).

## Configuration
Example of configuration for the automation:

```csharp
var secondaryLight = new LightAutomationBase
        {   
            EntitiesList = new List<ILightEntityCore>
            {
                lights.LivingroomBulb,
                lights.LedLivingRoomSonoffLed,
            },
            Triggers = new []
            {
                new MotionSensor( 
                    new[]
                    {
                        sensors.LivingRoomMotionSensor1Motion,
                        sensors.LivingRoomMotionSensorMotion
                    }, context)
            },
            ServiceAccountId = personEntities.Netdaemon.Attributes?.UserId ?? "",
            WaitTime = TimeSpan.FromMinutes(20),
            SwitchTimer = TimeSpan.FromHours(1),
            StopAtTimeFunc = () => DateTime.Parse(sun.Sun.Attributes?.NextDawn ?? "06:00:00").TimeOfDay,
            StartAtTimeFunc = () =>
                DateTime.Parse(sun.Sun.Attributes?.NextDusk ?? "20:00:00").TimeOfDay -
                TimeSpan.FromMinutes(30),
            NightMode = new NightModeConfig
            {
                IsEnabled = true,
                Devices = new List<ILightEntityCore>
                {
                    lights.LivingroomBulb,
                },
                StartAtTimeFunc = () => DateTime.Parse("23:00:00").TimeOfDay
            }
        };
```

As you can se, `StopAtTimeFunc` and `StartAtTimeFunc` properties are callable, so you can use any logic to calculate the time.
In this example, the start and stop time are calculated based on the next dawn and next dusk time with default value to 6:00 and 20:00.

### Night Mode

An additional feature of the automation is the night mode.
The night mode is activated at a specified time and is deactivated at a specified time.
When the night mode is activated, the specified set of lights is turned on with a lower brightness level (configurable value).
When the time is out of the night mode period, the light will be turned on with the previous brightness level on the next motion event.

> [!WARNING]
> When the light is turned on during night mode by the automation, the last parameters of the light are saved **inside the automation**.
> That means that the light will be turned on manually by after the night mode period, the light will be set to parameters **as it was turned on during the night mode**
> because this event is not handled by the automation, but rather by the Home Assistant itself.

> [!NOTE]
> Currently the automation can't configure light action based on switch event.
> However, this feature can be implemented in the future.


