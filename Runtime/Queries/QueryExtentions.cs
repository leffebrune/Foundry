namespace Foundry.Queries // 별도의 네임스페이스로 분리
{
    public static class QueryExtensions
    {
        public static QueryBuilder Query(this World world)
        {
            return new QueryBuilder(world);
        }

        public static QueryBuilder Query<T1>(this World world)
            where T1 : struct, IComponent
        {
            return world.Query().With<T1>();
        }

        public static QueryBuilder Query<T1, T2>(this World world)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            return world.Query<T1>().With<T2>();
        }

        public static QueryBuilder Query<T1, T2, T3>(this World world)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            return world.Query<T1, T2>().With<T3>();
        }

        public static QueryBuilder Query<T1, T2, T3, T4>(this World world)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            return world.Query<T1, T2, T3>().With<T4>();
        }

        public static QueryBuilder Query<T1, T2, T3, T4, T5>(this World world)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            where T5 : struct, IComponent
        {
            return world.Query<T1, T2, T3, T4>().With<T5>();
        }


        /// <summary>
        /// 월드에 단 하나만 존재해야 하는 싱글턴 컴포넌트를 안전하게 가져옵니다.
        /// </summary>
        /// <param name="component">찾은 컴포넌트입니다.</param>
        /// <returns>컴포넌트를 가진 엔티티가 정확히 하나일 경우 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool TryQuerySingletonEntity<T>(this World world, out Entity entity) where T : struct, IComponent
        {
            var query = world.Query<T>();
            using var enumerator = query.GetEnumerator();

            // 1. 첫 번째 엔티티를 찾습니다.
            if (!enumerator.MoveNext())
            {
                entity = default;
                return false; // 컴포넌트를 가진 엔티티가 없음
            }

            var firstEntity = enumerator.Current;

            // 2. 두 번째 엔티티가 있는지 확인합니다.
            if (enumerator.MoveNext())
            {
                entity = default;
                return false; // 컴포넌트를 가진 엔티티가 둘 이상임
            }

            // 정확히 하나만 존재하므로 성공
            entity = firstEntity;
            return true;
        }


        /// <exception cref="System.InvalidOperationException">컴포넌트를 가진 엔티티가 없거나 둘 이상일 경우 발생합니다.</exception>
        public static Entity QuerySingletonEntity<T>(this World world) where T : struct, IComponent
        {
            if (TryQuerySingletonEntity<T>(world, out var entity))
            {
                return entity;
            }

            // TryQuerySingleton이 실패한 경우, 더 상세한 예외 메시지를 제공할 수 있습니다.
            throw new System.InvalidOperationException($"Could not find a unique singleton component of type '{typeof(T).Name}'.");
        }        
        
        /// <summary>
        /// 월드에 단 하나만 존재해야 하는 싱글턴 컴포넌트를 가져옵니다.
        /// </summary>
        /// <returns>찾은 컴포넌트입니다.</returns>
        /// <exception cref="System.InvalidOperationException">컴포넌트를 가진 엔티티가 없거나 둘 이상일 경우 발생합니다.</exception>
        public static T QuerySingleton<T>(this World world) where T : struct, IComponent
        {
            if (TryQuerySingletonEntity<T>(world, out var entity))
            {
                return entity.GetComponent<T>();
            }

            // TryQuerySingleton이 실패한 경우, 더 상세한 예외 메시지를 제공할 수 있습니다.
            throw new System.InvalidOperationException($"Could not find a unique singleton component of type '{typeof(T).Name}'.");
        }           
    }
}