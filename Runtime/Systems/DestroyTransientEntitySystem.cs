using Foundry.Queries;

namespace Foundry.Systems
{
    internal class DestroyTransientEntitySystem : ISystem
    {
        // 이 시스템은 별도의 초기화나 정리 로직이 필요 없습니다.
        public void SetUp() { }
        public void TearDown() { }

        public void OnUpdate(World world, CommandBuffer commandBuffer)
        {
            // 1. ITransient 컴포넌트를 가진 모든 엔티티를 쿼리합니다.
            var transientEntities = world.Query<ITransient>();

            // 2. 쿼리 결과로 나온 모든 엔티티에 대해 파괴 명령을 CommandBuffer에 기록합니다.
            foreach (var entity in transientEntities)
            {
                commandBuffer.DestroyEntity(entity.Id);
            }
        }
    }
}