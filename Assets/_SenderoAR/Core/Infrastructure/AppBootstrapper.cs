using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using SenderoAR.Core.Infrastructure;

namespace SenderoAR.Core.Infrastructure
{
    [DefaultExecutionOrder(-1000)]
    public sealed class AppBootstrapper : MonoBehaviour
    {
        [Header("Performance")]
        [Tooltip("Target frame rate. 30 FPS fijo según baseline Snapdragon 778G + Adreno 642L")]
        [SerializeField] private int targetFrameRate = 30;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            ConfigurePlatform();
        }

        private IEnumerator Start()
        {
            Debug.Log("[Bootstrap] Iniciando validación de soporte AR...");

            yield return ARSession.CheckAvailability();

            if (ARSession.state == ARSessionState.NeedsInstall)
            {
                Debug.Log("[Bootstrap] AR requiere instalación de ARCore. Solicitando...");
                yield return ARSession.Install();
            }

            if (ARSession.state == ARSessionState.Unsupported)
            {
                Debug.LogError("[Bootstrap] Device sin soporte AR. Abortando.");
                yield break;
            }

            Debug.Log($"[Bootstrap] AR disponible. Estado: {ARSession.state}");

            AppContext.Initialize();

            Debug.Log("[Bootstrap] Transicionando a escena _Main...");
            var op = SceneManager.LoadSceneAsync(SceneNames.Main, LoadSceneMode.Single);
            if (op != null)
            {
                op.allowSceneActivation = true;
                while (!op.isDone)
                    yield return null;
            }
        }

        private void ConfigurePlatform()
        {
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;

            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;

            Debug.Log($"[Bootstrap] Platform configured: {targetFrameRate} FPS, Portrait locked");
        }
    }
}