using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Foundry
{
    public class QueryBuilder : IEnumerable<Entity>
    {
        private readonly World _world;
        private readonly List<Type> _withTypes = new();
        private readonly List<Type> _withoutTypes = new();

        private Type _withChangedType = null;
        private int _changedSinceTick = -1;

        internal QueryBuilder(World world)
        {
            _world = world;
        }

        /// <summary>
        /// 쿼리 결과에 반드시 포함되어야 할 컴포넌트 타입을 추가합니다.
        /// </summary>
        public QueryBuilder With<T>() where T : struct, IComponent
        {
            _withTypes.Add(typeof(T));
            return this;
        }
        /// <summary>
        /// 쿼리 결과에서 반드시 제외되어야 할 컴포넌트 타입을 추가합니다.
        /// </summary>
        public QueryBuilder Without<T>() where T : struct, IComponent
        {
            _withoutTypes.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// 특정 틱(Tick) 이후로 변경된 특정 컴포넌트를 가진 엔티티를 찾도록 쿼리를 제한합니다.
        /// 이 메서드는 쿼리 당 한 번만 호출할 수 있습니다.
        /// </summary>
        public QueryBuilder WithChangedSince<T>(int lastTick) where T : struct, IComponent
        {
            if (_withChangedType != null)
            {
                throw new InvalidOperationException("WithChangedSince can only be called once per query.");
            }

            var type = typeof(T);

            // WithChanged는 당연히 With를 전제하므로, 자동으로 _withTypes에 추가해줍니다.
            if (!_withTypes.Contains(type))
            {
                _withTypes.Add(type);
            }

            _withChangedType = type;
            _changedSinceTick = lastTick;
            return this;
        }


        /// <summary>
        /// 쿼리 조건에 따라 최적의 순회 메서드를 선택하여 실행하는 분배기 역할을 합니다.
        /// </summary>
        public IEnumerator<Entity> GetEnumerator()
        {
            // With 조건이 있으면 최적화된 빠른 경로를 사용합니다.
            if (_withTypes.Count > 0)
            {
                return GetEnumeratorOptimized();
            }
            // Without 조건만 있으면 전체 순회 경로를 사용합니다.
            else if (_withoutTypes.Count > 0)
            {
                return GetEnumeratorFullScan();
            }

            // 아무 조건도 없으면 빈 결과를 반환합니다.
            return Enumerable.Empty<Entity>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        #region Private Query Execution Methods

        /// <summary>
        /// [빠른 경로] With 조건 중 가장 작은 풀을 기준으로 순회하는 최적화된 쿼리를 실행합니다.
        /// </summary>
        private IEnumerator<Entity> GetEnumeratorOptimized()
        {
            // --- 1. 가장 효율적인 순회 기준(Iteration Source) 찾기 ---
            IEnumerable<EntityId> iterationSource;

            if (_withChangedType != null)
            {
                // WithChangedSince 조건이 있다면, 변경된 엔티티 목록을 미리 필터링하여 순회 기준으로 삼는다.
                var changedPool = _world.GetPool(_withChangedType);
                if (changedPool == null || changedPool.Count == 0) yield break;

                var candidates = new List<EntityId>();
                foreach (var entityId in changedPool.AsEntityIds())
                {
                    if (changedPool.GetTick(entityId) > _changedSinceTick)
                    {
                        candidates.Add(entityId);
                    }
                }
                iterationSource = candidates;
            }
            else
            {
                // WithChangedSince 조건이 없다면, 가장 작은 컴포넌트 풀을 순회 기준으로 삼는다.
                var smallestPool = GetSmallestComponentPool();
                if (smallestPool == null) yield break;
                iterationSource = smallestPool.AsEntityIds();
            }

            // --- 2. 최종 순회 및 나머지 조건 검사 ---
            foreach (var entityId in iterationSource)
            {
                if (MatchesAllRemainingConditions(entityId))
                {
                    yield return new Entity(entityId, _world);
                }
            }
        }

        /// <summary>
        /// [느린 경로] With 조건이 없어 최적화가 불가능할 때, 모든 엔티티를 순회하는 쿼리를 실행합니다.
        /// </summary>

        private IEnumerator<Entity> GetEnumeratorFullScan()
        {
            foreach (var entityId in _world.GetActiveEntities())
            {
                if (MatchesAllRemainingConditions(entityId))
                {
                    yield return new Entity(entityId, _world);
                }
            }
        }

        #endregion
        

        #region Helper Methods

        private IComponentPool GetSmallestComponentPool()
        {
            IComponentPool smallestPool = null;
            int minCount = int.MaxValue;

            foreach (var type in _withTypes)
            {
                var pool = _world.GetPool(type);
                if (pool == null) return null; // 해당 풀이 없으면 결과는 0개
                
                if (pool.Count < minCount)
                {
                    minCount = pool.Count;
                    smallestPool = pool;
                }
            }
            return smallestPool;
        }

        private bool MatchesAllRemainingConditions(EntityId entityId)
        {
            // With 조건 확인
            foreach (var withType in _withTypes)
            {
                // 순회 기준으로 사용된 타입은 다시 검사할 필요가 없지만,
                // 로직 단순화를 위해 모든 With 조건을 검사해도 무방하다.
                if (!_world.HasComponent(entityId, withType))
                {
                    return false;
                }
            }

            // Without 조건 확인
            foreach (var withoutType in _withoutTypes)
            {
                if (_world.HasComponent(entityId, withoutType))
                {
                    return false;
                }
            }

            // (iterationSource로 사용되지 않은 경우를 대비한) WithChangedSince 조건 확인
            if (_withChangedType != null && _world.GetComponentTick(entityId, _withChangedType) <= _changedSinceTick)
            {
                 return false;
            }

            return true;
        }
        
        #endregion
    }
}