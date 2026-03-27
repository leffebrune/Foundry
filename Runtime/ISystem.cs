namespace Foundry
{    
    // 모든 시스템이 구현할 생명주기 인터페이스
    public interface ISystem
    {
        void OnUpdate(World world, CommandBuffer commandBuffer);
    }
}