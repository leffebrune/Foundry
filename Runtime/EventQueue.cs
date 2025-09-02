using System.Collections.Generic;

namespace Foundry
{
    /// <summary>
    /// 한 틱 동안 발생한 모든 이벤트를 수집하는 내부용 클래스입니다.
    /// </summary>
    internal class EventQueue
    {
        private readonly List<IEventData> _events = new List<IEventData>();
        
        // PlaybackResult를 통해 외부에 노출될 읽기 전용 리스트
        public IReadOnlyList<IEventData> AsReadOnly() => _events;

        /// <summary>
        /// CommandBuffer로부터 재생된 이벤트들을 메인 큐에 추가합니다.
        /// </summary>
        internal void PushEvents(List<IEventData> events)
        {
            if (events.Count > 0)
            {
                _events.AddRange(events);
            }
        }

        internal void Clear()
        {
            _events.Clear();
        }
    }
}