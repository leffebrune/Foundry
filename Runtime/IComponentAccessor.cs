using System;

namespace Foundry
{
    public interface IComponentAccessor
    {
        T GetComponent<T>(EntityId entityId) where T : struct, IComponent;
        bool HasComponent(EntityId entityId, Type componentType);
    }
}