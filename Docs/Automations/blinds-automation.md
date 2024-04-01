# Blinds Automation

```csharp
```

## Description

This automation aims to control the blinds.
The idea of this automation is to close and open the blinds at specific time of the day.

> [!NOTE]
> The development was made using [AQARA Roller Shade Driver E1](https://www.aqara.com/en/product/roller-shade-driver-e1/) as a main device.
> So, the functionality in this automation is based on the features of this device presented in Home Assistant.

## Configuration

```csharp
var blinds = new BlindAutomationBase
{
    Sun = sun.Sun,
    EntitiesList = new List<ICoverEntityCore>
    {
        covers.LivingRoomBlinds1Cover,
    },
    StartAtTimeFunc = () => Date.Parse("06:00:00").TimeOfDay,
    StopAtTimeFunc = null
};
```

In blinds automation `StartAtTimeFunc` and `StopAtTimeFunc` have the following meaning:

- `StartAtTimeFunc` -> when the blinds should be **opened**
- `StopAtTimeFunc` -> when the blinds should be **closed**

In this example `StartAtTimeFunc` is set to `06:00:00` and `StopAtTimeFunc` is set to `null`.
This means that the blinds will be opened at 6:00 AM.
`null` value means that the blinds should be triggered by sun state changed event in Home Assistant.
This event is triggered when the sun rises or sets.

