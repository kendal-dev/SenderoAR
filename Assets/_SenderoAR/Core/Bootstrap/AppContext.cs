// Path: Assets/_SenderoAR/Core/Bootstrap/AppContext.cs

using System;
using UnityEngine;

namespace KendalLab.SenderoAR.Core.Bootstrap
{
    /// <summary>
    /// Service Locator estático del proyecto Sendero AR.
    /// Registro centralizado de servicios consumidos por ViewModels y Views.
    ///
    /// REGLAS DE USO (innegociables):
    ///   1. Solo el AppBootstrapper llama a Initialize().
    ///   2. ViewModels NO llaman a AppContext en su lógica interna.
    ///      Reciben sus dependencias por constructor (Composition Root pattern).
    ///   3. Views (MonoBehaviours) PUEDEN leer AppContext en Awake para
    ///      obtener el ViewModel que necesitan instanciar.
    ///   4. Si AppContext.IsReady == false → cualquier acceso lanza excepción.
    ///
    /// Por qué Service Locator y no DI Container:
    ///   - Cero reflection (Zenject/VContainer son lentos en mobile cold start)
    ///   - Cero allocations en path crítico de AR (60 frames/seg)
    ///   - 1 dev: legibilidad > pureza arquitectónica
    ///
    /// Roadmap del registro:
    ///   Sprint 0: skeleton (este archivo)
    ///   Sprint 3: registro de IMonumentRepository + IARTrackingService
    ///   Sprint 4: registro de ILocalizationService
    ///   Sprint 7: registro de IAudioPlaybackService
    ///   Sprint 9: registro de IChatbotClient (Gemini via Vertex AI)
    /// </summary>
    public static class AppContext
    {
        // ────────────────────────────────────────────────────────────────
        // State
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// True cuando AppBootstrapper terminó la inicialización exitosamente.
        /// Las Views deben chequear esto antes de instanciar ViewModels.
        /// </summary>
        public static bool IsReady { get; private set; }

        // ────────────────────────────────────────────────────────────────
        // Service slots (se irán llenando sprint a sprint)
        // ────────────────────────────────────────────────────────────────

        // Sprint 3: descomentar y wirear
        // public static IMonumentRepository Monuments { get; private set; }
        // public static IARTrackingService Tracking { get; private set; }

        // Sprint 4:
        // public static ILocalizationService Localization { get; private set; }

        // Sprint 7:
        // public static IAudioPlaybackService Audio { get; private set; }

        // Sprint 9:
        // public static IChatbotClient Chatbot { get; private set; }

        // ────────────────────────────────────────────────────────────────
        // Lifecycle
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Inicializa el contexto. Solo invocado desde AppBootstrapper.
        /// En Sprint 0 es un no-op funcional: solo marca IsReady.
        /// </summary>
        public static void Initialize()
        {
            if (IsReady)
            {
                Debug.LogWarning("[AppContext] Initialize() called twice. Ignored.");
                return;
            }

            // Sprint 3+: aquí se cablearán los servicios reales
            // Monuments = new MonumentRepository(...);
            // Tracking = new ARTrackingService(...);

            IsReady = true;
            Debug.Log("[AppContext] Initialized.");
        }

        /// <summary>
        /// Resetea el contexto. Solo para tests unitarios y hot-reload en Editor.
        /// </summary>
        public static void Reset()
        {
            // Sprint 3+: dispose de servicios
            // (Monuments as IDisposable)?.Dispose();

            IsReady = false;
            Debug.Log("[AppContext] Reset.");
        }

        // ────────────────────────────────────────────────────────────────
        // Guard helpers
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Throw si el contexto no está listo. Llamado por getters de servicios.
        /// </summary>
        internal static void EnsureReady(string callerName)
        {
            if (!IsReady)
            {
                throw new InvalidOperationException(
                    $"[AppContext] Access from '{callerName}' rejected: " +
                    "Initialize() has not been called yet. " +
                    "Make sure AppBootstrapper.Start() completed successfully " +
                    "before instantiating ViewModels or Views.");
            }
        }
    }
}