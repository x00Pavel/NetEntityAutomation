# HOWTO Use NetEntityAutomation Library

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
namespace nd_app.apps.HomeAutomation.Configs;

public class LivingRoomConfig: IRoomConfig
{
    public string Name => "Living room";
    public ILogger Logger { get; set; }
    public IEnumerable<AutomationConfig> Entities { get; set; }
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
        var secondaryLight = new AutomationConfig
        {   
            AutomationType = AutomationType.SecondaryLight,
            ServiceAccountId = personEntities.Netdaemon.Attributes?.UserId ?? "",
            Entities = new[]
            {
                lights.LivingroomBulb,
                lights.LedLivingRoomSonoffLed,
            },
            Triggers = new []
            {
                new MotionSensor( 
                    new []
                    {
                        sensors.MotionSensorLivingRoomMotion,
                        sensors.MotionSensorLivingRoom2Motion
                    }, context)
            },
            NightMode = new NightModeConfig
            {
                IsEnabled = true,
            }
        };
        var blinds = new AutomationConfig
        {
            AutomationType = AutomationType.Blinds,
            Entities = new Entity[]
            {
                covers.LumiLumiCurtainAcn002Cover,
                sun.Sun
            }
        };
        Entities = new List<AutomationConfig>
        {
            secondaryLight,
            blinds
        };
    }
}
```

In the example above, `LivingRoomConfig` class is used to define a living room.
Abstracting from the details, it contains a list of entities that are grouped for a specific automation scenario: secondary light and blinds.
Each entity is represented by an instance of `AutomationConfig` class.
`AutomationConfig` class contains information about the entity, its triggers, and other settings that are specific to the automation scenario.
There is more options available in `AutomationConfig` class, but the example above shows the most common ones.

> This configuration might not be ideal and might change in the future.
> It is recommended to check the latest version of the library to see the most up-to-date configuration.

More datailed information about configuration classes and their properties can be found in the API documentation.
