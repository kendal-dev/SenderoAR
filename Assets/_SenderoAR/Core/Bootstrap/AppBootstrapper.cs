// Path: Assets/_SenderoAR/Core/Bootstrap/AppBootstrapper.cs

using System;
using System.Collections;
using Unity.XR.CoreUtils;              // ← XROrigin vive acá, no en ARFoundation
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

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
    ///
    /// Decisión arquitectónica:
    ///   No se usa framework de DI (Zenject/VContainer) por simplicidad de MVP
    ///   y para evitar reflection overhead en mobile. Cableado manual via AppContext.
    ///
    /// Nota sobre AR Foundation 5.2:
    ///   Usamos XROrigin (Unity.XR.CoreUtils), introducido en AR Foundation 5.0
    ///   como reemplazo unificado de ARSessionOrigin (deprecada en 5.x).
    ///   La API de tracking de imágenes sigue siendo legacy:
    ///   maxNumberOfMovingImages, trackedImagesChanged, ARTrackedImagesChangedEventArgs.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class AppBootstrapper : MonoBehaviour
    {
        // ────────────────────────────────────────────────────────────────
        // Inspector references (cableado vía Editor, no FindObjectOfType)
        // ────────────────────────────────────────────────────────────────

        [Header("AR Foundation 5.2 References")]
        [Tooltip("Drag AR Session GameObject from hierarchy")]
        [SerializeField] private ARSession arSession;

        [Tooltip("Drag XR Origin GameObject from hierarchy (NOT ARSessionOrigin, deprecated in 5.x)")]
        [SerializeField] private XROrigin xrOrigin;

        [Header("Performance")]
        [Tooltip("Target frame rate. 30 FPS fijo según baseline Snapdragon 7 Gen 1")]
        [SerializeField] private int targetFrameRate = 30;

        // ────────────────────────────────────────────────────────────────
        // Unity lifecycle
        // ────────────────────────────────────────────────────────────────

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

            //AppContext.Initialize();

            // TODO Sprint 1: SceneManager.LoadSceneAsync("_Main", LoadSceneMode.Single);
        }

        // ────────────────────────────────────────────────────────────────
        // Configuration
        // ────────────────────────────────────────────────────────────────

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