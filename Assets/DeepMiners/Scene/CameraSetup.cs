using UnityEngine;

namespace DeepMiners.Scene
{
    public class CameraSetup : MonoBehaviour
    {
        public float refreshRate = 5;
        public float depthOffset;
        public float moveSpeed = 5;
        public float minZoom;
        public float maxZoom;
        public float zoomSpeed = 5;
        public float blockSizeFactor = 1;
        public float offsetFactor = 1;
    }
}