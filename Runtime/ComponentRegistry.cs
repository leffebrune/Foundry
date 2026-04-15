using System;
using System.Collections.Generic;

namespace Foundry
{
    public class ComponentRegistry
    {
        private readonly Dictionary<Type, IComponentPool> _pools = new();

        private ComponentPool<T> GetOrCreatePool<T>() where T : struct, IComponent
        {
            var componentType = typeof(T);
            if (!_pools.TryGetValue(componentType, out var pool))
            {
                pool = new ComponentPool<T>();
                _pools[componentType] = pool;
            }
            return (ComponentPool<T>)pool;
        }

        private IComponentPool GetOrCreatePool(Type componentType)
        {
            if (!componentType.IsValueType)
                throw new InvalidOperationException($"Component '{componentType.Name}' must be a struct, but it is a class.");

            if (!_pools.TryGetValue(componentType, out var pool))
            {
                var poolTypeDefinition = typeof(ComponentPool<>);
                var concretePoolType = poolTypeDefinition.MakeGenericType(componentType);
                pool = (IComponentPool)Activator.CreateInstance(concretePoolType);
                _pools[componentType] = pool;
            }
            return pool;
        }

        internal IComponentPool GetPool(Type componentType)
        {
            if (_pools.TryGetValue(componentType, out var pool))
            {
                return pool;
            }
            return null;
        }

        internal void AddComponent<T>(Entity entityId, T component, int tick) where T : struct, IComponent
        {
            var pool = GetOrCreatePool<T>();
            if (pool.Has(entityId))
            {
                throw new InvalidOperationException($"Entity {entityId} already has component {typeof(T).Name}. Use SetComponent to update.");
            }
            pool.Set(entityId, component, tick);
        }

        public void AddComponent(Entity entityId, IComponent component, int tick)
        {
            var componentType = component.GetType();
            var pool = GetOrCreatePool(componentType);
            if (pool.Has(entityId))
            {
                throw new InvalidOperationException($"Entity {entityId} already has component {componentType.Name}. Use SetComponent to update.");
            }
            pool.Set(entityId, component, tick);
        }

        internal void SetComponent<T>(Entity entityId, T component, int tick) where T : struct, IComponent
        {
            GetOrCreatePool<T>().Set(entityId, component, tick);
        }

        internal void SetComponent(Entity entityId, IComponent component, int tick)
        {
            var componentType = component.GetType();
            var pool = GetOrCreatePool(componentType);
            pool.Set(entityId, component, tick);
        }

        internal T GetComponent<T>(Entity entityId) where T : struct, IComponent
        {
            return GetOrCreatePool<T>().Get(entityId);
        }

        internal int GetComponentTick(Entity entityId, Type componentType)
        {
            var pool = GetPool(componentType);
            if (pool == null)
            {
                throw new InvalidOperationException($"Component type {componentType.Name} not found for entity {entityId}.");
            }
            return pool.GetTick(entityId);
        }

        internal bool IsComponentChangedSince<T>(Entity entityId, int lastTick) where T : struct, IComponent
        {
            var pool = GetPool(typeof(T));

            if (pool == null || !pool.Has(entityId))
            {
                return false;
            }

            // 컴포넌트의 마지막 업데이트 Tick을 비교하여 변경 여부를 반환합니다.
            return pool.GetTick(entityId) > lastTick;
        }

        internal bool HasComponent<T>(Entity entityId) where T : struct, IComponent
        {
            return GetOrCreatePool<T>().Has(entityId);
        }

        internal bool HasComponent(Entity entityId, Type type)
        {
            var pool = GetOrCreatePool(type);
            return pool != null && pool.Has(entityId);
        }

        internal void RemoveComponent<T>(Entity entityId) where T : struct, IComponent
        {
            GetOrCreatePool<T>().Remove(entityId);
        }

        internal void RemoveComponent(Entity entityId, Type componentType)
        {
            GetOrCreatePool(componentType).Remove(entityId);
        }

        internal void RemoveAllComponents(Entity entityId)
        {
            // 모든 풀을 순회하며 해당 엔티티 ID를 가진 컴포넌트를 제거하도록 요청합니다.
            foreach (var pool in _pools.Values)
            {
                pool.Remove(entityId);
            }
        }
    }
}