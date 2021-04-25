using DeepMiners.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace DeepMiners.Utils
{
    public static class BlockUtil
    {
        private static bool CheckNeighbourBlocks<T>(Random random, 
            int2 point, 
            int2 origin, 
            int2 size,
            NativeHashMap<int2, Entity> map, 
            NativeList<int2> buffer, 
            ComponentDataFromEntity<T> filter, 
            NativeHashMap<int2, int> checkMap) where T: struct, IComponentData
        {
            int nextRandom = random.NextInt(0, 2);
            
            if (checkMap.ContainsKey(point))
            {
                return false;
            }
            
            if (point.x >= size.x || point.y >= size.y || point.x < 0 || point.y < 0)
            {
                return false;
            }

            if (!filter.HasComponent(map[point]) && !point.Equals(origin))
            {
                buffer.Add(point);
                return true;
            }
            
            checkMap.Add(point, -1);

            if (nextRandom == 0)
            {
                CheckNeighbourBlocks(random, new int2(point.x + 1, point.y), origin, size, map, buffer, filter, checkMap);
                CheckNeighbourBlocks(random, new int2(point.x, point.y + 1), origin, size, map, buffer, filter, checkMap);
                CheckNeighbourBlocks(random, new int2(point.x - 1, point.y), origin, size, map, buffer, filter, checkMap);
                CheckNeighbourBlocks(random, new int2(point.x, point.y - 1), origin, size, map, buffer, filter, checkMap);
            }
            else
            {
                CheckNeighbourBlocks(random, new int2(point.x - 1, point.y), origin, size, map, buffer, filter, checkMap);
                CheckNeighbourBlocks(random, new int2(point.x, point.y - 1), origin, size, map, buffer, filter, checkMap);
                CheckNeighbourBlocks(random, new int2(point.x + 1, point.y), origin, size, map, buffer, filter, checkMap);
                CheckNeighbourBlocks(random, new int2(point.x, point.y + 1), origin, size, map, buffer, filter, checkMap);
            }
            
            return false;
        }
        
        public static float3 ToWorld(int3 blockPoint, float3 visualOrigin, float blockSize) =>
            visualOrigin + new float3(blockPoint.x, -blockPoint.y, blockPoint.z) * blockSize;
        
        public static bool GetClosestBlockOnSameLevel<T>(
            Random random, 
            int2 point, 
            ComponentDataFromEntity<Depth> sorter, 
            ComponentDataFromEntity<T> filter, 
            NativeHashMap<int2, Entity> map, 
            NativeHashMap<int2, int> checkMap, 
            NativeList<int2> buffer, int2 size, 
            
            out int2 result) where T : struct, IComponentData
        {
            buffer.Clear();
            checkMap.Clear();
            result = new int2(-1,-1);
            CheckNeighbourBlocks(random, point, point, size, map, buffer, filter, checkMap);
            int2 origin = point;
            float closest = int.MaxValue;
            for (int i = 0; i < buffer.Length; i++)
            {
                int2 p = buffer[i];
                Entity e = map[p];
                Depth dist = sorter[e];
                if (dist.Value < closest && !origin.Equals(p))
                {
                    closest = dist.Value;
                    result = p;
                }
            }
            return result.x >= 0;
        }

        public static bool ContainsPoint(int2 point, int2 size) 
            => point.x >=0 && point.y >= 0 && point.x < size.x && point.y < size.y;
    }
}