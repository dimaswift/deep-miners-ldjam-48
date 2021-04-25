using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeepMiners.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace DeepMiners.UI
{
    [RequireComponent(typeof(Button))]
    public class ClickEffect : MonoBehaviour
    {
        [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0,0,1,1);
        [SerializeField] private float volume = 0.5f;
        [SerializeField] private SoundReference clickSound;

        
        private AudioClip sound;
        private Transform cam;
        private void OnValidate()
        {
            #if UNITY_EDITOR

            if (clickSound == null || clickSound.editorAsset == null)
            {
                clickSound = new SoundReference(UnityEditor.AssetDatabase.AssetPathToGUID("Assets/Sounds/button.wav"));
            }
            
            #endif
        }

        private async void Awake()
        {
            sound = await clickSound.LoadOrGetAsync<AudioClip>();

            while (Camera.main == null)
            {
                await Task.Yield();
            }
            
            cam = Camera.main.transform;
            GetComponent<Button>().onClick.AddListener(() =>
            {
                StartCoroutine(ClickRoutine());
            });
        }

        private IEnumerator ClickRoutine()
        {
            AudioSource.PlayClipAtPoint(sound, cam.position, volume);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / 0.2f;
                transform.localScale = Vector3.one * (0.9f + 0.1f * curve.Evaluate(t));
                yield return null;
            }

            transform.localScale = Vector3.one;
        }
    }
}
