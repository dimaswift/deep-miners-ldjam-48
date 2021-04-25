using System;
using DeepMiners.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace DeepMiners.Utils
{
    public static class BlockUtil
    {

        private static void CheckNeighbourBlocks(Random random, int3 point, int3 origin, int2 size, NativeHashMap<int3, Entity> map, NativeList<int3> fill, NativeHashMap<int3, int> checkMap)
        {
            var r = random.NextInt(0, 3);
            
            if (checkMap.ContainsKey(point))
            {
                return;
            }
            
            if (point.x >= size.x || point.z >= size.y || point.x < 0 || point.z < 0)
            {
                return;
            }

            if (map[point] != Entity.Null)
            {
                checkMap.Add(point, math.abs(origin.x - point.x) + math.abs(origin.z - point.z));
                fill.Add(point);
                return;
            }
            
            checkMap.Add(point, -1);

            if (r == 0)
            {
                CheckNeighbourBlocks(random, new int3(point.x + 1, point.y, point.z), origin, size, map, fill, checkMap);

                CheckNeighbourBlocks(random,new int3(point.x - 1, point.y, point.z), origin, size, map, fill,checkMap);

                CheckNeighbourBlocks(random,new int3(point.x, point.y, point.z + 1), origin,  size, map, fill,checkMap);

                CheckNeighbourBlocks(random,new int3(point.x, point.y, point.z - 1), origin, size, map,fill, checkMap);
            }
            else if (r == 1)
            {
                CheckNeighbourBlocks(random,new int3(point.x, point.y, point.z + 1), origin,  size, map, fill,checkMap);
                
                CheckNeighbourBlocks(random,new int3(point.x, point.y, point.z - 1), origin, size, map,fill, checkMap);
                
                CheckNeighbourBlocks(random,new int3(point.x - 1, point.y, point.z), origin, size, map, fill,checkMap);
                
                CheckNeighbourBlocks(random,new int3(point.x + 1, point.y, point.z), origin, size, map, fill, checkMap);
            }
            else
            {
                CheckNeighbourBlocks(random,new int3(point.x, point.y, point.z + 1), origin,  size, map, fill,checkMap);
                
                CheckNeighbourBlocks(random,new int3(point.x + 1, point.y, point.z), origin, size, map, fill, checkMap);

                CheckNeighbourBlocks(random,new int3(point.x, point.y, point.z - 1), origin, size, map,fill, checkMap);
                
                CheckNeighbourBlocks(random,new int3(point.x - 1, point.y, point.z), origin, size, map, fill,checkMap);
            }
        }
        
        public static float3 ToWorld(int3 blockPoint, float3 visualOrigin, float blockSize) =>
            visualOrigin + new float3(blockPoint.x, -blockPoint.y, blockPoint.z) * blockSize;
        
        public static bool GetClosestBlockOnSameLevel<T>(Random random, int3 point, ComponentDataFromEntity<T> filter, NativeHashMap<int3, Entity> map, NativeHashMap<int3, int> checkMap, NativeList<int3> list, int2 size, out int3 result) where T : struct, IComponentData
        {
            checkMap.Clear();
            result = new int3();
            CheckNeighbourBlocks(random,point, point, size, map, list, checkMap);
        
            int closest = int.MaxValue;
            for (int i = 0; i < list.Length; i++)
            {
                int3 p = list[i];
                if (!filter.HasComponent(map[p]))
                {
                    closest = p.x;
                    result = p;
                    break;
                }
            }

            return closest != int.MaxValue;
         
        }
        
        public static bool HasBlock(int3 position, int2 size, int currentDepth, NativeHashMap<int3, Entity> map)
        {
            return ContainsPoint(position, size, currentDepth) && map[position] != Entity.Null;
        }
        
        public static bool BuildPath(int3 source, int3 destination, NativeHashMap<int3, Entity> map, DynamicBuffer<BlockDestination> path)
        {
            path.Clear();
            
            
            
            return true;
        }
        
        public static bool  ContainsPoint(int3 point, int2 size, int currentDepth)
        {
            if (point.x < 0 || point.z < 0)
            {
                return false;
            }
            return point.x < size.x && point.z < size.y && point.y < currentDepth;
        }

        public static int GetHighestNonEmptyBlockLevel(NativeHashMap<int3, Entity> map, int2 size, int currentDepth, int start)
        {
            for (int level = start; level < currentDepth; level++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    for (int z = 0; z < size.y; z++)
                    {
                        if (map[new int3(x, level, z)] != Entity.Null)
                        {
                            return level;
                        }
                    }
                }
            }

            return start;
        }
        
    }
}