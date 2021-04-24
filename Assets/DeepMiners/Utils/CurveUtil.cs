
using DeepMiners.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DeepMiners.Utils
{
    public static class CurveUtil
    {
        public static BlobAssetReference<KeyframeBlobArray> GetCurveBlobReference(Keyframe[] keyframes)
        {
            using BlobBuilder builder = new BlobBuilder(Allocator.Temp);
            ref KeyframeBlobArray data = ref builder.ConstructRoot<KeyframeBlobArray>();
            BlobBuilderArray<Keyframe> arr = builder.Allocate(ref data.Keyframes, keyframes.Length);
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = keyframes[i];
            }
            return builder.CreateBlobAssetReference<KeyframeBlobArray>(Allocator.Persistent);
        }
        
        public static float Evaluate(ref BlobArray<Keyframe> curve, float t)
        {
            float value = 0;

            for (int i = 0; i < curve.Length; i++)
            {
                int next = math.clamp(i + 1, 0, curve.Length - 1);
                Keyframe start = curve[i];
                Keyframe end = curve[next];
            
                int minCheck = math.select(0, 1, t > start.time);
                int maxCheck = math.select(0, 1, t <= end.time);
                int check = minCheck * maxCheck;
            
                float distanceTime = end.time - start.time;
            
                float m0 = start.outTangent * distanceTime;
                float m1 = end.inTangent * distanceTime;
            
                float t2 = t * t;
                float t3 = t2 * t;
            
                float a = 2 * t3 - 3 * t2 + 1;
                float b = t3 - 2 * t2 + t;
                float c = t3 - t2;
                float d = -2 * t3 + 3 * t2;
            
                value += (a * start.value + b * m0 + c * m1 + d * end.value) * check;
            }
            
            return value;
        }
    }
}