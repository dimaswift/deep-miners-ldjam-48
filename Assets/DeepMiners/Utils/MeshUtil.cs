using DeepMiners.Prefabs;
using Unity.Entities;
using Unity.Rendering;

namespace DeepMiners.Utils
{
    public static class MeshUtil
    {
        public static void AddMeshRendererToEntity(Entity entity, RenderMeshPrefab prefab)
        {
            RenderMeshUtility.AddComponents(entity, World.DefaultGameObjectInjectionWorld.EntityManager, prefab.GetDescription());
        }
    }
}