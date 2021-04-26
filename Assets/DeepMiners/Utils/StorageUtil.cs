using System;
using UnityEngine;

namespace DeepMiners.Utils
{
    public static class StorageUtil
    {
        public static void Serialize<T>(T obj, string key = null)
        {
            string path = typeof(T).FullName + key;
            PlayerPrefs.SetString(path, JsonUtility.ToJson(obj));
        }
        
        public static bool Exists<T>(string key = null)
        {
            string path = typeof(T).FullName + key;
            return PlayerPrefs.HasKey(path);
        }
        
        public static T Deserialize<T>(string key = null)
        {
            string path = typeof(T).FullName + key;
            try
            {
                if (PlayerPrefs.HasKey(path))
                {
                    return JsonUtility.FromJson<T>(PlayerPrefs.GetString(path));
                }

                return default;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return default;
            }
        }
        
        public static T DeserializeScriptable<T>(T instance, string key = null) where T : ScriptableObject
        {
            string path = typeof(T).FullName + key;
            try
            {
                if (PlayerPrefs.HasKey(path))
                {
                    if (instance == null)
                    {
                        instance = ScriptableObject.CreateInstance<T>();
                    }
                    JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(path), instance);
                    return instance;
                }

                return default;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return default;
            }
        }
    }
}