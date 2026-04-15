using System;

namespace Foundry
{
    public interface IComponentAccessor
    {
        T GetComponent<T>(Entity entityId) where T : struct, IComponent;
        bool HasComponent(Entity entityId, Type componentType);
    }
}