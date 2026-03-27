using System;
using System.Collections.Generic;

namespace Foundry
{
    internal interface IComponentPool
    {
        int Count { get; }
        IEnumerable<EntityId> AsEntityIds();

        void Set(EntityId entityId, IComponent componentData, int tick);
        int GetTick(EntityId entityId);
        void Remove(EntityId entityId);
        bool Has(EntityId entityId);

        Type GetComponentType();
    }

    internal interface IComponentPool<T> : IComponentPool where T : struct, IComponent
    {
        void Set(EntityId entityId, T component, int tick);
        T Get(EntityId entityId);
    }

    internal class ComponentPool<T> : IComponentPool<T> where T : struct, IComponent
    {
        private readonly struct ComponentEntry
        {
            public readonly T Data;
            public readonly int Tick;

            internal ComponentEntry(T data, int tick)
            {
                Data = data;
                Tick = tick;
            }
        }

        private readonly List<ComponentEntry> _components = new();

        private readonly Dictionary<EntityId, int> _entityIdToIndexMap = new();
        private readonly Dictionary<int, EntityId> _indexToEntityIdMap = new();

        public void Set(EntityId entityId, T component, int tick)
        {
            var entry = new ComponentEntry(component, tick);

           if (_entityIdToIndexMap.TryGetValue(entityId, out var index))
            {
                _components[index] = entry;
                return;
            }

            var newIndex = _components.Count;
            _components.Add(entry);

            _entityIdToIndexMap[entityId] = newIndex;
            _indexToEntityIdMap[newIndex] = entityId;
        }


        public void Set(EntityId entityId, IComponent componentData, int tick)
        {
            if (componentData is not T castedComponent)
            {
                throw new InvalidOperationException($"Component data type mismatch: expected {typeof(T)}, got {componentData.GetType()}");
            }

            Set(entityId, castedComponent, tick);
        }

        public int GetTick(EntityId entityId)
        {
            if (_entityIdToIndexMap.TryGetValue(entityId, out var index))
            {
                return _components[index].Tick; // Entry에서 Tick만 반환
            }
            return -1; // 컴포넌트가 없으면 유효하지 않은 값 반환
        }

        public T Get(EntityId entityId)
        {
            if (_entityIdToIndexMap.TryGetValue(entityId, out var index))
            {
                return _components[index].Data;
            }
            return default;
        }

        public bool Has(EntityId entityId)
        {
            return _entityIdToIndexMap.ContainsKey(entityId);
        }

        public void Remove(EntityId entityId)
        {
            if (!_entityIdToIndexMap.TryGetValue(entityId, out var indexToRemove))
            {
                return; // 지울 대상이 없음
            }

            var lastIndex = _components.Count - 1;

            if (indexToRemove == lastIndex)
            {
                _components.RemoveAt(lastIndex);
                _entityIdToIndexMap.Remove(entityId);
                _indexToEntityIdMap.Remove(lastIndex);
                return;
            }

            // 리스트의 마지막 요소가 어떤 엔티티의 것인지 역방향 맵에서 조회합니다.
            var entityIdOfLastComponent = _indexToEntityIdMap[lastIndex];
            var lastComponent = _components[lastIndex];

            // 마지막 요소를 지울 위치로 복사합니다 (Swap).
            _components[indexToRemove] = lastComponent;

            _entityIdToIndexMap[entityIdOfLastComponent] = indexToRemove;
            _indexToEntityIdMap[indexToRemove] = entityIdOfLastComponent;

            _components.RemoveAt(lastIndex);

            _entityIdToIndexMap.Remove(entityId);
            _indexToEntityIdMap.Remove(lastIndex);
        }

        public int Count => _components.Count;
        public IEnumerable<EntityId> AsEntityIds() => _entityIdToIndexMap.Keys;
        public Type GetComponentType() => typeof(T);
    }
}