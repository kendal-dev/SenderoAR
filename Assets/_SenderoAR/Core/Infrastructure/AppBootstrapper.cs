using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using SenderoAR.Core.Contracts;
using SenderoAR.Core.Repositories;
using SenderoAR.Models;

namespace SenderoAR.Core.Infrastructure
{
    [DefaultExecutionOrder(-1000)]
    public sealed class AppBootstrapper : MonoBehaviour
    {
        [Header("Performance")]
        [Tooltip("Target frame rate. 30 FPS fijo según baseline Snapdragon 778G + Adreno 642L")]
        [SerializeField] private int targetFrameRate = 30;

        [Header("Data")]
        [Tooltip("Catálogo de monumentos. Se asigna en _App_Boot: SenderoMonumentCatalog.asset")]
        [SerializeField] private MonumentCatalogSO monumentCatalog;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            ConfigurePlatform();
            RegisterServices();
        }

        /// <summary>
        /// Composition Root: construye los servicios y los registra en AppContext.
        /// Corre en Awake porque el registro se cierra en AppContext.Initialize(),
        /// que se llama en Start despues de validar el soporte AR.
        /// </summary>
        private void RegisterServices()
        {
            if (monumentCatalog == null)
            {
                Debug.LogError(
                    "[Bootstrap] monumentCatalog no está asignado en _App_Boot. " +
                    "IMonumentRepository no se registra. Asignar SenderoMonumentCatalog.asset.", this);
                return;
            }

            // Si un monumento tiene datos inválidos, el constructor lanza aca, en el
            // arranque, con el nombre del monumento en el mensaje.
            var repository = new CatalogMonumentRepository(monumentCatalog);
            AppContext.Register<IMonumentRepository>(repository);
            Debug.Log($"[Bootstrap] IMonumentRepository registrado ({monumentCatalog.Count} monumentos).");
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