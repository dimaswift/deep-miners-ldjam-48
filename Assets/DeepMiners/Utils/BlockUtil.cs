using System;
using DeepMiners.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = Unity.Mathematics.Random;

namespace DeepMiners.Utils
{
    public static class BlockUtil
    {

        private static bool CheckNeighbourBlocks<T>(Random random, int2 point, int2 origin, int2 size, NativeHashMap<int2, Entity> map, NativeList<int2> list, ComponentDataFromEntity<T> filter, NativeHashMap<int2, int> checkMap) where T: struct, IComponentData
        {
            var r = random.NextInt(0, 6);
            
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
                list.Add(point);
                return true;
            }
            
            checkMap.Add(point, -1);

            CheckNeighbourBlocks(random, new int2(point.x + 1, point.y), origin, size, map, list, filter, checkMap);
            CheckNeighbourBlocks(random, new int2(point.x, point.y + 1), origin, size, map, list, filter, checkMap);
            CheckNeighbourBlocks(random, new int2(point.x - 1, point.y), origin, size, map, list, filter, checkMap);
            CheckNeighbourBlocks(random, new int2(point.x, point.y - 1), origin, size, map, list, filter, checkMap);
            
            
            return false;
        }
        
        public static float3 ToWorld(int3 blockPoint, float3 visualOrigin, float blockSize) =>
            visualOrigin + new float3(blockPoint.x, -blockPoint.y, blockPoint.z) * blockSize;
        
        public static bool GetClosestBlockOnSameLevel<T>(
            Random random, 
            int2 point, 
            ComponentDataFromEntity<Dent> sorter, 
            ComponentDataFromEntity<T> filter, 
            NativeHashMap<int2, Entity> map, 
            NativeHashMap<int2, int> checkMap, 
            NativeList<int2> list, int2 size, 
            
            out int2 result) where T : struct, IComponentData
        {
            list.Clear();
            checkMap.Clear();
            result = new int2(-1,-1);
            CheckNeighbourBlocks(random, point, point, size, map, list, filter, checkMap);
            int2 origin = point;
            float closest = int.MaxValue;

            for (int i = 0; i < list.Length; i++)
            {
                int2 p = list[i];
                var e = map[p];

                var dist = sorter[e];
                if (dist.Value < closest && !origin.Equals(p))
                {
                    closest = dist.Value;
                    result = p;
                }
            }

            return result.x >= 0;

        }
        
        public static bool HasBlock(int2 position, int2 size, NativeHashMap<int2, Entity> map)
        {
            return ContainsPoint(position, size) && map[position] != Entity.Null;
        }
        
        
        public static bool ContainsPoint(int2 point, int2 size)
        {
            if (point.x < 0 || point.y < 0)
            {
                return false;
            }
            return point.x < size.x && point.y < size.y;
        }
    }
}