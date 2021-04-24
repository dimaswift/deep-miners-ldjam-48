using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DeepMiners.Utils
{
    public static class BlockUtil
    {
        private static void CheckNeighbourBlocks(int3 point, int3 origin, int2 size, NativeHashMap<int3, Entity> map, NativeList<int3> fill, NativeHashMap<int3, int> checkMap)
        {
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

            CheckNeighbourBlocks(new int3(point.x + 1, point.y, point.z), origin, size, map, fill, checkMap);

            CheckNeighbourBlocks(new int3(point.x - 1, point.y, point.z), origin, size, map, fill,checkMap);

            CheckNeighbourBlocks(new int3(point.x, point.y, point.z + 1), origin,  size, map, fill,checkMap);

            CheckNeighbourBlocks(new int3(point.x, point.y, point.z - 1), origin, size, map,fill, checkMap);
        }
        
        public static float3 ToWorld(int3 blockPoint, float3 visualOrigin, float blockSize) =>
            visualOrigin + new float3(blockPoint.x, -blockPoint.y, blockPoint.z) * blockSize;
        
        public static bool GetClosestBlockOnSameLevel(int3 point, NativeHashMap<int3, Entity> map, NativeHashMap<int3, int> checkMap, NativeList<int3> list, int2 size, out int3 result)
        {
            checkMap.Clear();
            result = new int3();
            CheckNeighbourBlocks(point, point, size, map, list, checkMap);
        
            int closest = int.MaxValue;
            for (int i = 0; i < list.Length; i++)
            {
                var p = list[i];
                var dist = math.abs(point.x - p.x) + math.abs(point.z - p.z);
                if (dist < closest)
                {
                    closest = dist;
                    result = p;
                }
            }

            return closest != int.MaxValue;
         
        }
        
        public static bool HasBlock(int3 position, int2 size, int currentDepth, NativeHashMap<int3, Entity> map)
        {
            return ContainsPoint(position, size, currentDepth) && map[position] != Entity.Null;
        }
        
        public static bool  ContainsPoint(int3 point, int2 size, int currentDepth)
        {
            if (point.x < 0 || point.z < 0)
            {
                return false;
            }
            return point.x < size.x && point.z < size.y && point.y < currentDepth;
        }
    }
}