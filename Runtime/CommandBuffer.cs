using System;
using System.Collections.Generic;
using System.Linq;

namespace Foundry
{
    internal enum CommandType
    {
        CreateEntity,
        DestroyEntity,
        AddComponent,
        RemoveComponent,
        SetComponent
    }

    public readonly struct CommandHandle
    {
        private readonly CommandBuffer _commandBuffer;
        private readonly int _commandIndex;

        internal CommandHandle(CommandBuffer commandBuffer, int commandIndex)
        {
            _commandBuffer = commandBuffer;
            _commandIndex = commandIndex;
        }

        public void ContinueWith(Action<World, EntityId> callback)
        {
            _commandBuffer.AddCallbackToCommand(_commandIndex, callback);
        }
    }

    internal class EntityCommand
    {
        public readonly CommandType Type;
        public readonly EntityId TargetId;
        public readonly IComponent ComponentData;
        public readonly Type ComponentType;
        public readonly IReadOnlyList<IComponent> ComponentsForCreate;

        public Action<World, EntityId> PostExecutionCallback { get; set; }


        public EntityCommand(CommandType type, EntityId targetId, IComponent componentData, Type componentType, IReadOnlyList<IComponent> componentsForCreate)
        {
            Type = type;
            TargetId = targetId;
            ComponentData = componentData;
            ComponentType = componentType;
            ComponentsForCreate = componentsForCreate;
        }
    }

    public class CommandBuffer
    {
        private readonly List<EntityCommand> _commands = new();
        private readonly List<IEventData> _events = new();

        internal void CopyFrom(CommandBuffer other)
        {
            _commands.AddRange(other._commands);
            _events.AddRange(other._events);
        }

        public void RaiseEvent(IEventData eventData)
        {
            _events.Add(eventData);
        }

        internal void AddCallbackToCommand(int commandIndex, Action<World, EntityId> callback)
        {
            if (commandIndex >= 0 && commandIndex < _commands.Count)
            {
                // 이미 콜백이 있다면 체이닝되도록 처리
                _commands[commandIndex].PostExecutionCallback += callback;
            }
        }

        public CommandHandle CreateEntity()
        {
            _commands.Add(new EntityCommand(CommandType.CreateEntity, EntityId.Invalid, null, null, null));
            return new CommandHandle(this, _commands.Count - 1);
        }

        public CommandHandle CreateEntity(IEnumerable<IComponent> components)
        {
            _commands.Add(new EntityCommand(CommandType.CreateEntity, EntityId.Invalid, null, null, components?.ToList() ?? new List<IComponent>()));
            return new CommandHandle(this, _commands.Count - 1);
        }

        public CommandHandle CreateEntity(params IComponent[] components)
        {
            _commands.Add(new EntityCommand(CommandType.CreateEntity, EntityId.Invalid, null, null, components));
            return new CommandHandle(this, _commands.Count - 1);
        }

        public CommandHandle DestroyEntity(EntityId entityId)
        {
            _commands.Add(new EntityCommand(CommandType.DestroyEntity, entityId, null, null, null));
            return new CommandHandle(this, _commands.Count - 1);
        }

        public CommandHandle AddComponent<T>(EntityId entityId, T component) where T : struct, IComponent
        {
            _commands.Add(new EntityCommand(CommandType.AddComponent, entityId, component, typeof(T), null));
            return new CommandHandle(this, _commands.Count - 1);
        }

        public CommandHandle RemoveComponent<T>(EntityId entityId) where T : struct, IComponent
        {
            _commands.Add(new EntityCommand(CommandType.RemoveComponent, entityId, null, typeof(T), null));
            return new CommandHandle(this, _commands.Count - 1);
        }

        public CommandHandle SetComponent<T>(EntityId entityId, T component) where T : struct, IComponent
        {
            _commands.Add(new EntityCommand(CommandType.SetComponent, entityId, component, typeof(T), null));
            return new CommandHandle(this, _commands.Count - 1);
        }

        public void Playback(World world)
        {
            foreach (var command in _commands)
            {
                switch (command.Type)
                {
                    case CommandType.CreateEntity:
                        var newEntityId = world.CreateEntity();
                        if (command.ComponentsForCreate != null)
                        {
                            foreach (var component in command.ComponentsForCreate)
                            {
                                world.AddComponent(newEntityId, component);
                            }
                        }
                        command.PostExecutionCallback?.Invoke(world, newEntityId);
                        break;

                    case CommandType.DestroyEntity:
                        world.DestroyEntity(command.TargetId);
                        command.PostExecutionCallback?.Invoke(world, command.TargetId);
                        break;

                    case CommandType.AddComponent:
                        world.AddComponent(command.TargetId, command.ComponentData);
                        command.PostExecutionCallback?.Invoke(world, command.TargetId);
                        break;

                    case CommandType.RemoveComponent:
                        world.RemoveComponent(command.TargetId, command.ComponentType);
                        command.PostExecutionCallback?.Invoke(world, command.TargetId);
                        break;

                    case CommandType.SetComponent:
                        world.SetComponent(command.TargetId, command.ComponentData);
                        command.PostExecutionCallback?.Invoke(world, command.TargetId);
                        break;
                }
            }

            _commands.Clear();

            world.EventQueue.PushEvents(_events);
            _events.Clear();
        }

        public void Clear()
        {
            _commands.Clear();
            _events.Clear();
        }
    }
}