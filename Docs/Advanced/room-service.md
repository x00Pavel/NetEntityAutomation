# Room Controls

The NetEntityAutomation creates several services for controlling automation in individual rooms.

> Currently, only toggle of all automations in the room is supported.

The name of such service is in the format `netdaemon.toggle_<room_name>_service`
where `<room_name>` is the name of the room in lower case with spaces replace by underscores.
To call such service in Home Assistant, use the following service call:

```yaml
service: netdaemon.toggle_living_room_service
data: {}
```

> As only toggle action is supported, there is no need to specify any data.
> This approach is used to simplify the service call.
> In the future, more actions with data can be added to the service.