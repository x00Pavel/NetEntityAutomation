How to Build and Use Custom Automation
=======================================

NetEntityAutomation library provides a set of interfaces that can be used for creating custom automation.
Such automations can be packed to the NuGet package.
This is the way how users can share their automation with others.

## Automation Interfaces

There are several abstract classes that need to be implemented to create a custom automation:

- `AutomationBase` - the base class for all automations.
- `AutomationBase<TEntity>` - the base class for automations that work with specific entity type.
   This class is used for the most common cases. 
- `AutomationBase<TEntity, TFsm>` - the base class for automations that work with specific entity type and uses Finite State Machine (FSM).
   FSM in most cases is used for storing the current state of the automation to restore it after application restart (e.g. after system restart or crash).

An example of usage of these classes can be found in the `NetEntityAutomation.Core.Automations` project (built-in automations) or in [example](https://github.com/x00Pavel/NetEntityAutomation-Custo-Automatation) repo with the entire NetDaemon application and custom automation.
