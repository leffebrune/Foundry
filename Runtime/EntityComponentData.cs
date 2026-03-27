using UnityEngine;

namespace Foundry
{
    public abstract class EntityComponentData : ScriptableObject
    {
        public abstract IComponent AddComponentTo(World world, in EntityId uid);
    }
    
    public abstract class EntityComponentData<T> : EntityComponentData where T : struct, IComponent
    {
        protected abstract T CreateComponent(EntityId uid);

        public override IComponent AddComponentTo(World world, in EntityId uid)
        {
            var component = CreateComponent(uid);
            world.AddComponent(uid, component);
            return component;
        }
    }    
}