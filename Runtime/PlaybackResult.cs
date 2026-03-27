using System.Collections.Generic;

namespace Foundry
{
    /// <summary>
    /// World.Update() 실행 후의 결과물을 담는 구조체입니다.
    /// 해당 틱에서 발생한 이벤트 목록 등을 포함합니다.
    /// </summary>
    public readonly struct PlaybackResult
    {
        /// <summary>
        /// 해당 틱에서 발생한 모든 이벤트의 읽기 전용 목록입니다.
        /// </summary>
        public IReadOnlyList<IEventData> Events { get; }

        internal PlaybackResult(IReadOnlyList<IEventData> events)
        {
            Events = events;
        }

        public static readonly PlaybackResult Empty = new(new List<IEventData>());
    }
}