using Unity.Entities;

namespace DeepMiners.Data
{
    public struct Block : IComponentData
    {
        public BlockType Type;
    }
    
    public struct IsBeingDrilling : IComponentData
    {
        
    }
}
