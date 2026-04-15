namespace Foundry
{
    public static class WorldExtensions
    {
        public static bool TryGetComponents<T1, T2>(this World world, Entity entity, out T1 component1, out T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            component1 = default;
            component2 = default;

            if (entity == Entity.Invalid || entity == Entity.Null)
                return false;

            if (!world.IsAlive(entity))
                return false;

            var hasComponent1 = world.HasComponent<T1>(entity);
            if (!hasComponent1)
                return false;

            var hasComponent2 = world.HasComponent<T2>(entity);
            if (!hasComponent2)
                return false;

            component1 = world.GetComponent<T1>(entity);
            component2 = world.GetComponent<T2>(entity);
            return true;
        }

        public static bool TryGetComponents<T1, T2, T3>(this World world, Entity entity, out T1 component1, out T2 component2, out T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            component1 = default;
            component2 = default;
            component3 = default;

            if (entity == Entity.Invalid || entity == Entity.Null)
                return false;

            if (!world.IsAlive(entity))
                return false;

            var hasComponent1 = world.HasComponent<T1>(entity);
            if (!hasComponent1)
                return false;

            var hasComponent2 = world.HasComponent<T2>(entity);
            if (!hasComponent2)
                return false;

            var hasComponent3 = world.HasComponent<T3>(entity);
            if (!hasComponent3)
                return false;

            component1 = world.GetComponent<T1>(entity);
            component2 = world.GetComponent<T2>(entity);
            component3 = world.GetComponent<T3>(entity);
            return true;
        }

        public static bool TryGetComponents<T1, T2, T3, T4>(this World world, Entity entity, out T1 component1, out T2 component2, out T3 component3, out T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            component1 = default;
            component2 = default;
            component3 = default;
            component4 = default;

            if (entity == Entity.Invalid || entity == Entity.Null)
                return false;

            if (!world.IsAlive(entity))
                return false;

            var hasComponent1 = world.HasComponent<T1>(entity);
            if (!hasComponent1)
                return false;

            var hasComponent2 = world.HasComponent<T2>(entity);
            if (!hasComponent2)
                return false;

            var hasComponent3 = world.HasComponent<T3>(entity);
            if (!hasComponent3)
                return false;

            var hasComponent4 = world.HasComponent<T4>(entity);
            if (!hasComponent4)
                return false;

            component1 = world.GetComponent<T1>(entity);
            component2 = world.GetComponent<T2>(entity);
            component3 = world.GetComponent<T3>(entity);
            component4 = world.GetComponent<T4>(entity);
            return true;
        }

        public static bool TryGetComponents<T1, T2, T3, T4, T5>(this World world, Entity entity, out T1 component1, out T2 component2, out T3 component3, out T4 component4, out T5 component5)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
        {
            component1 = default;
            component2 = default;
            component3 = default;
            component4 = default;
            component5 = default;

            if (entity == Entity.Invalid || entity == Entity.Null)
                return false;

            if (!world.IsAlive(entity))
                return false;

            var hasComponent1 = world.HasComponent<T1>(entity);
            if (!hasComponent1)
                return false;

            var hasComponent2 = world.HasComponent<T2>(entity);
            if (!hasComponent2)
                return false;

            var hasComponent3 = world.HasComponent<T3>(entity);
            if (!hasComponent3)
                return false;

            var hasComponent4 = world.HasComponent<T4>(entity);
            if (!hasComponent4)
                return false;

            var hasComponent5 = world.HasComponent<T5>(entity);
            if (!hasComponent5)
                return false;

            component1 = world.GetComponent<T1>(entity);
            component2 = world.GetComponent<T2>(entity);
            component3 = world.GetComponent<T3>(entity);
            component4 = world.GetComponent<T4>(entity);
            component5 = world.GetComponent<T5>(entity);
            return true;
        }
    }
}