using Unity.Collections;
using Unity.Rendering;
using UnityEngine;

namespace DeepMiners.Prefabs
{

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class RenderMeshPrefab : MonoBehaviour
    {
        [SerializeField] private bool flipTriangles = false;

        public RenderMeshDescription GetDescription()
        {
            var meshRenderer = GetComponent<MeshRenderer>();
            var meshFilter = GetComponent<MeshFilter>();

            return new RenderMeshDescription()
            {
                RenderMesh = new RenderMesh()
                {
                    material = meshRenderer.sharedMaterial,
                    subMesh = meshRenderer.subMeshStartIndex,
                    receiveShadows = meshRenderer.receiveShadows,
                    mesh = meshFilter.sharedMesh,
                    needMotionVectorPass = meshRenderer.motionVectorGenerationMode != MotionVectorGenerationMode.ForceNoMotion,
                    castShadows = meshRenderer.shadowCastingMode,
                    layer = meshRenderer.gameObject.layer
                },
                FlipWinding = flipTriangles,
                RenderingLayerMask = meshRenderer.renderingLayerMask
            }; 
        }
    }
}