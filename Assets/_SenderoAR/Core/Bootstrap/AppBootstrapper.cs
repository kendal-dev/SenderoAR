using System;
using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using SenderoAR.Core.Infrastructure; // ← Inyectamos el acceso a SceneNames

namespace KendalLab.SenderoAR.Core.Bootstrap
{
    /// <summary>
    /// Composition Root del proyecto Sendero AR.
    /// Único MonoBehaviour permitido como punto de entrada del sistema.
    ///
    /// Responsabilidades:
    ///   1. Bloquear orientación + target frame rate (30 FPS fijo)
    ///   2. Validar disponibilidad de AR en el device
    ///   3. Instanciar servicios y registrarlos en AppContext (Sprint 3+)
    ///   4. Transicionar a la escena de gameplay (_Main, Sprint 1)
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class AppBootstrapper : MonoBehaviour
    {
        [Header("AR Foundation 5.2 References")]
        [Tooltip("Drag AR Session GameObject from hierarchy")]
        [SerializeField] private ARSession arSession;

        [Tooltip("Drag XR Origin GameObject from hierarchy (NOT ARSessionOrigin, deprecated in 5.x)")]
        [SerializeField] private XROrigin xrOrigin;

        [Header("Performance")]
        [Tooltip("Target frame rate. 30 FPS fijo según baseline Snapdragon 7 Gen 1")]
        [SerializeField] private int targetFrameRate = 30;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            ConfigurePlatform();
            ValidateInspectorReferences();
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
                // TODO Sprint 5: mostrar UI de error al usuario
                yield break;
            }

            Debug.Log($"[Bootstrap] AR disponible. Estado: {ARSession.state}");
            Debug.Log($"[Bootstrap] XR Origin camera: {xrOrigin.Camera.name}");

            AppContext.Initialize();

            // Reemplazo del TODO del Sprint 1: Lógica asíncrona de transición
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

        private void ValidateInspectorReferences()
        {
            if (arSession == null)
                throw new InvalidOperationException(
                    "[Bootstrap] ARSession reference is null. " +
                    "Drag the AR Session GameObject into the Inspector field.");

            if (xrOrigin == null)
                throw new InvalidOperationException(
                    "[Bootstrap] XROrigin reference is null. " +
                    "Drag the XR Origin GameObject into the Inspector field.");

            if (xrOrigin.Camera == null)
                throw new InvalidOperationException(
                    "[Bootstrap] XROrigin.Camera is null. " +
                    "Check that Main Camera is properly nested under Camera Offset.");
        }
    }
}