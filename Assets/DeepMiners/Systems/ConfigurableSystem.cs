using System.Collections.Generic;
using System.Threading.Tasks;
using DeepMiners.Config;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Systems
{
    public abstract class ConfigurableSystem<TConfig> : SystemBase where TConfig : BaseConfig
    {
        protected readonly Dictionary<int, TConfig> Configs = new Dictionary<int, TConfig>();
        protected readonly Dictionary<int, RenderMeshDescription> MeshDescriptions = new Dictionary<int, RenderMeshDescription>();

        protected Entity CreateBaseEntity(float3 position)
        {
            Entity entity = EntityManager.CreateEntity();
            EntityManager.AddComponentData(entity, new Translation() { Value = position });
            EntityManager.AddComponentData(entity, new Rotation());
            EntityManager.AddComponentData(entity, new LocalToWorld());
            return entity;
        }
        
        protected virtual async Task LoadConfigs(TConfig[] configs)
        {
            foreach (TConfig config in configs)
            {
                if (Configs.ContainsKey(config.TypeIndex))
                {
                    continue;
                }
                Configs[config.TypeIndex] = config;
                MeshDescriptions[config.TypeIndex] = config.GetDescription();
            }
        }
    }
}