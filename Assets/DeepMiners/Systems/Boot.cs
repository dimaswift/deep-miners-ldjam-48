using UnityEngine;

namespace Systems
{
    public static class Boot
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
#if UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP
        DefaultWorldInitialization.Initialize("Default World", false);
#endif
        }
    }
}