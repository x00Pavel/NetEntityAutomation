using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Extensions.LightExtensionMethods;

public partial record LightEntity : Entity<LightEntity, EntityState<LightAttributes>, LightAttributes>, ILightEntityCore
{
    public LightEntity(IHaContext haContext, string entityId) : base(haContext, entityId)
    {
    }

    public LightEntity(IEntityCore entity) : base(entity)
    {
    }
}