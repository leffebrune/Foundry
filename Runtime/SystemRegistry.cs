using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Foundry
{
    public enum ExecutionPhase
    {
        Input,          // 입력 및 커맨드 생성
        Validation,     // 커맨드 처리 및 유효성 검사
        Execution,      // 핵심 시뮬레이션 (행동)
        Reaction,       // 결과 및 반응 처리
        ViewCalculation,// 뷰 데이터 계산
        Cleanup         // 임시 데이터 정리
    }

    internal class SystemRegistry
    {
        private readonly struct Entry
        {
            public ISystem System { get; }
            public int Priority { get; }
            public Entry(ISystem system, int priority)
            {
                System = system;
                Priority = priority;
            }
        }
        private readonly Dictionary<Type, ISystem> _systemsLookup = new();

        private readonly Dictionary<ExecutionPhase, List<Entry>> _systemsByPhase = new();
        private readonly Dictionary<ExecutionPhase, List<ISystem>> _sortedSystemsCache = new();
        private bool _isCacheDirty = true;

        /// <summary>
        /// 새로운 시스템을 레지스트리에 등록합니다.
        /// </summary>
        public void Register(ExecutionPhase phase, ISystem system, int priority)
        {
            var systemType = system.GetType();

            if (_systemsLookup.ContainsKey(systemType))
            {
                Debug.LogWarning($"[Warning] System type '{systemType.Name}' is already registered.");
                return;
            }

            // Phase에 해당하는 리스트가 없으면 새로 생성
            if (!_systemsByPhase.ContainsKey(phase))
            {
                _systemsByPhase[phase] = new List<Entry>();
            }

            _systemsLookup.Add(systemType, system);
            _systemsByPhase[phase].Add(new Entry(system, priority));

            _isCacheDirty = true;
        }

        /// <summary>
        /// 시스템을 레지스트리에서 등록 해제합니다.
        /// </summary>
        public void Unregister(ISystem system)
        {
            var systemType = system.GetType();

            if (_systemsLookup.Remove(systemType))
            {
                // 모든 Phase를 순회하며 해당 시스템을 제거
                foreach (var phaseSystems in _systemsByPhase.Values)
                {
                    int removedCount = phaseSystems.RemoveAll(entry => entry.System == system);
                    if (removedCount > 0)
                    {
                        _isCacheDirty = true;
                        break; // 시스템은 한 곳에만 등록되므로 찾으면 중단
                    }
                }
            }
        }

        /// <summary>
        /// 제네릭 타입으로 등록된 시스템을 조회합니다.
        /// </summary>
        public T GetSystem<T>() where T : ISystem
        {
            if (_systemsLookup.TryGetValue(typeof(T), out var system))
            {
                return (T)system;
            }

            return default; // default는 참조 타입에 대해 null을 반환합니다.
        }

        /// <summary>
        /// 특정 실행 단계(Phase)에 등록된 모든 시스템을 우선순위에 따라 정렬하여 반환합니다.
        /// </summary>
        public IEnumerable<ISystem> GetSystemsForPhase(ExecutionPhase phase)
        {
            // 캐시가 유효하지 않으면 전체 캐시를 다시 빌드
            if (_isCacheDirty)
            {
                RebuildCache();
            }

            if (_sortedSystemsCache.TryGetValue(phase, out var systems))
            {
                return systems;
            }

            return Enumerable.Empty<ISystem>();
        }

        /// <summary>
        /// 모든 Phase의 시스템 목록을 우선순위에 따라 정렬하여 캐시를 재생성합니다.
        /// </summary>
        private void RebuildCache()
        {
            _sortedSystemsCache.Clear();
            foreach (var phasePair in _systemsByPhase)
            {
                var phase = phasePair.Key;
                var systems = phasePair.Value;

                // Stable Sort를 보장하는 OrderBy 사용
                _sortedSystemsCache[phase] = systems.OrderBy(entry => entry.Priority)
                                                    .Select(entry => entry.System)
                                                    .ToList();
            }
            _isCacheDirty = false;
        }
    }
}