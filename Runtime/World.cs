using System;
using System.Collections.Generic;
using Foundry.Systems;

namespace Foundry
{
    public class World : IComponentAccessor
    {
        public const int InvalidUid = -1;
        public int Tick { get; private set; }
        public CommandBuffer IncomingCommands { get; } = new();

        internal EventQueue EventQueue => _eventQueue;

        private readonly HashSet<EntityId> _activeEntities = new();
        private readonly SystemRegistry _systemRegistry = new();
        private readonly ComponentRegistry _componentRegistry = new();
        private readonly CommandBuffer _commandBuffer = new();
        private readonly EventQueue _eventQueue = new();
        private EntityId _currentId = new(0);

        private readonly ExecutionPhase[] _phaseOrder =
        {
            ExecutionPhase.Input,
            ExecutionPhase.Validation,
            ExecutionPhase.Execution,
            ExecutionPhase.Reaction,
            ExecutionPhase.ViewCalculation,
            ExecutionPhase.Cleanup
        };

        public static World CreateDefault()
        {
            var world = new World();

            world.AddSystem(ExecutionPhase.Cleanup, new DestroyTransientEntitySystem(), int.MaxValue);

            return world;
        }

        internal World()
        {
        }

        public EntityId CreateEntity()
        {
            _currentId = _currentId.Next();
            _activeEntities.Add(_currentId);
            UnityEngine.Debug.Log($"Entity created with ID: {_currentId}");
            return _currentId;
        }

        public bool IsAlive(EntityId entityId)
        {
            return _activeEntities.Contains(entityId);
        }

        public void DestroyEntity(EntityId entityId)
        {
            if (!_activeEntities.Remove(entityId))
            {
                throw new InvalidOperationException($"Entity {entityId} does not exist.");
            }
            _componentRegistry.RemoveAllComponents(entityId);

            UnityEngine.Debug.Log($"Entity destroyed with ID: {entityId}");
        }

        public void AddComponent<T>(EntityId entityId, T component) where T : struct, IComponent
        {
            if (!_activeEntities.Contains(entityId))
            {
                throw new InvalidOperationException($"Entity {entityId} does not exist.");
            }
            _componentRegistry.AddComponent(entityId, component, Tick);
        }

        public void AddComponent(EntityId entityId, IComponent component)
        {
            if (!_activeEntities.Contains(entityId))
            {
                throw new InvalidOperationException($"Entity {entityId} does not exist.");
            }
            _componentRegistry.AddComponent(entityId, component, Tick);
        }

        public void SetComponent<T>(EntityId entityId, T component) where T : struct, IComponent
        {
            if (!_activeEntities.Contains(entityId))
            {
                throw new InvalidOperationException($"Entity {entityId} does not exist.");
            }
            _componentRegistry.SetComponent(entityId, component, Tick);
        }

        public void SetComponent(EntityId entityId, IComponent component)
        {
            if (!_activeEntities.Contains(entityId))
            {
                throw new InvalidOperationException($"Entity {entityId} does not exist.");
            }
            _componentRegistry.SetComponent(entityId, component, Tick);
        }

        public T GetComponent<T>(EntityId entityId) where T : struct, IComponent
        {
            if (!_activeEntities.Contains(entityId))
            {
                throw new InvalidOperationException($"Entity {entityId} does not exist.");
            }
            return _componentRegistry.GetComponent<T>(entityId);
        }

        public int GetComponentTick(EntityId entityId, Type componentType)
        {
            if (!_activeEntities.Contains(entityId))
            {
                throw new InvalidOperationException($"Entity {entityId} does not exist.");
            }
            return _componentRegistry.GetComponentTick(entityId, componentType);
        }

        public bool IsComponentChangedSince<T>(EntityId entityId, int lastTick) where T : struct, IComponent
        {
            if (!_activeEntities.Contains(entityId))
            {
                throw new InvalidOperationException($"Entity {entityId} does not exist.");
            }
            return _componentRegistry.IsComponentChangedSince<T>(entityId, lastTick);
        }

        public bool HasComponent<T>(EntityId entityId) where T : struct, IComponent
        {
            if (!_activeEntities.Contains(entityId))
            {
                throw new InvalidOperationException($"Entity {entityId} does not exist.");
            }
            return _componentRegistry.HasComponent<T>(entityId);
        }

        public bool HasComponent(EntityId entityId, Type componentType)
        {
            if (!_activeEntities.Contains(entityId))
            {
                throw new InvalidOperationException($"Entity {entityId} does not exist.");
            }
            return _componentRegistry.HasComponent(entityId, componentType);
        }

        public bool TryGetComponent<T>(EntityId entityId, out T component) where T : struct, IComponent
        {
            if (HasComponent<T>(entityId))
            {
                component = _componentRegistry.GetComponent<T>(entityId);
                return true;
            }
            component = default;    
            return false;
        }

        public void RemoveComponent<T>(EntityId entityId) where T : struct, IComponent
        {
            if (!_activeEntities.Contains(entityId))
            {
                throw new InvalidOperationException($"Entity {entityId} does not exist.");
            }
            _componentRegistry.RemoveComponent<T>(entityId);
        }

        public void RemoveComponent(EntityId entityId, Type componentType)
        {
            if (!_activeEntities.Contains(entityId))
            {
                throw new InvalidOperationException($"Entity {entityId} does not exist.");
            }
            _componentRegistry.RemoveComponent(entityId, componentType);
        }

        public void AddSystem(ExecutionPhase phase, ISystem system, int priority = 0)
        {
            _systemRegistry.Register(phase, system, priority);
        }

        public PlaybackResult Update()
        {
            Tick++;
            _eventQueue.Clear();

            // 외부에서 들어온 커맨드를 메인 커맨드 버퍼로 복사하여 처리 준비.
            // 이 커맨드들은 첫 번째 단계(Input)의 시스템들과 함께 처리됩니다.
            _commandBuffer.CopyFrom(IncomingCommands);
            IncomingCommands.Clear();

            // 정의된 순서대로 각 단계를 실행합니다.
            foreach (var phase in _phaseOrder)
            {
                // 1. 현재 단계에 해당하는 모든 시스템을 가져와 실행합니다.
                //    시스템들은 메인 커맨드 버퍼에 새로운 명령을 추가합니다.
                var systemsToRun = _systemRegistry.GetSystemsForPhase(phase);
                foreach (var system in systemsToRun)
                {
                    system.OnUpdate(this, _commandBuffer);
                }

                // 2. 현재 단계까지 쌓인 모든 커맨드를 즉시 재생(Playback)합니다.
                //    이것이 다음 단계의 시스템들이 최신 데이터를 볼 수 있게 하는 핵심입니다.
                _commandBuffer.Playback(this);
            }

            return new PlaybackResult(_eventQueue.AsReadOnly());
        }

        // 게임 종료 시 모든 시스템을 정리
        public void ShutdownAllSystems()
        {
            // 종료 시에는 등록의 역순으로 정리하는 것이 안전할 수 있습니다.
            // for (int i = _systemUpdateOrder.Count - 1; i >= 0; i--)
            // {
            //     _systemUpdateOrder[i].Shutdown();
            // }
        }

        public IEnumerable<EntityId> GetActiveEntities()
        {
            return _activeEntities;
        }

        internal IComponentPool GetPool(Type componentType)
        {
            return _componentRegistry.GetPool(componentType);
        }
    }
}