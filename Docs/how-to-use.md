# How To Use NetEntityAutomation Library

NetEntityAutomation library provides a set of ready-to-use automation scenarios.
It is designed to be used with [NetDaemon](https://netdaemon.xyz/) framework.
Main idea of the library is based on providing typical scenarios per room in the house (e.g. living room, kitchen, bedroom, etc.).
Room is configured via standard C# class that implements interface `IRoomConfig` and contains all the necessary information about the room and its entities.

To start using automations from this library, you need to:

1. Create a new NetDaemon project (process of creating a new project is described in [NetDaemon documentation](https://netdaemon.xyz/docs/user/started/development/)).
2. Install NuGet package [NetEntityAutomation.Core.RoomManager](https://github.com/users/x00Pavel/packages/nuget/package/NetEntityAutomation.Core.RoomManager) in your NetDaemon project.
3. Create configuration classes. Typically, one class represents a room in your house.
4. Add `RoomManager` to Dependency Injection container in your NetDaemon project by calling `AddRoomManager` in `program.cs`. This call has to be made **after** all NetDaemon related services are added to the container.
5. Add `RoomManager` to any NetDaemon app as a dependency to instantiate it. `RoomManager` will use discovered configuration classes to create and manage automations.

At this point you should have a working NetDaemon project with automations from NetEntityAutomation library.

## Configuration

Configuration class is used to define rooms and entities that are present in specified room.
Following example illustrates how to define a room configuration:

```csharp
using NetEntityAutomation.Core.Automations;
using NetEntityAutomation.Core.RoomManager;

namespace nd_app.apps.HomeAutomation.Configs;

public class LivingRoomConfigV1: IRoomConfig
{
    public string Name => "Living room";
    public ILogger Logger { get; set; }
    public IEnumerable<AutomationBase> Entities { get; set; }
    public NightModeConfig? NightMode { get; set; }

    public LivingRoomConfigV1(
        IHaContext context,
        ILogger<LivingRoomConfigV1> logger,
        LightEntities lights,
        BinarySensorEntities sensors,
        CoverEntities covers,
        SunEntities sun,
        PersonEntities personEntities
    )
    {
        Logger = logger;
        var mainLight = ;
        var secondaryLight = ;
        Entities = new List<AutomationBase>
        {
            new MainLightAutomationBase
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
            },
            new LightAutomationBase
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
            },
            new BlindAutomationBase
            {
                Sun = sun.Sun,
                EntitiesList = new List<ICoverEntityCore>
                {
                    covers.LivingRoomBlinds1Cover,
                },
                StartAtTimeFunc = null,
                StopAtTimeFunc = null
            }
        };
    }
}
```

In the example above, `LivingRoomConfig` class is used to define a living room.
Abstracting from the details, it contains a list of entities that are grouped for a specific automation scenario: main light, secondary light and blinds.
Each entity is represented by an instance of `AutomationBase` class.
`AutomationBase` class contains information about the entity, its triggers, and other settings that are specific to the automation scenario.
There is more options available in `AutomationBase` class, but the example above shows the most common ones.
This class should be inherited in order to define a custom automation scenario.

> This configuration might not be ideal and might change in the future.
> It is recommended to check the latest version of the library to see the most up-to-date configuration.

More detailed information about configuration classes and their properties can be found in the API documentation.
All available automations are described in the [automations](/Docs/Automations/main-light-automation.html) docs.
