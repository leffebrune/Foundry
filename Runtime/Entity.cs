namespace Foundry
{
    public readonly struct Entity
    {
        public readonly EntityId Id;
        private readonly IComponentAccessor _accessor;

        internal Entity(EntityId id, IComponentAccessor accessor)
        {
            Id = id;
            _accessor = accessor;
        }

        public T GetComponent<T>() where T : struct, IComponent
        {
            return _accessor.GetComponent<T>(Id);
        }

        public bool HasComponent<T>() where T : struct, IComponent
        {
            return _accessor.HasComponent(Id, typeof(T));
        }
    }
}