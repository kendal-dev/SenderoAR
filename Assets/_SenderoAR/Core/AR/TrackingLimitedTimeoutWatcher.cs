using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SenderoAR.Core.AR
{
    /// <summary>
    /// Sprint 1 provisional watcher. Emite un warning cuando una imagen trackeada
    /// permanece en TrackingState.Limited mas alla del timeout configurado.
    /// </summary>
    /// <remarks>
    /// Implementa el patron VIO Drift Timeout de 03_AR_Tracking_Specs.md seccion 10.
    /// El despawn queda deliberadamente diferido a la implementacion de
    /// IARTrackingService (Sprint 3): este watcher solo observa, nunca muta la
    /// escena. Threading: Unity main thread.
    ///
    /// ESTADO POR IMAGEN, NO GLOBAL: la version de Sprint 1 guardaba un unico
    /// _lastState para todas las imagenes. Con una sola imagen (el PoC) daba el
    /// resultado correcto; con las 5 del catalogo, la segunda imagen pisaba el
    /// estado de la primera y el watchdog se cancelaba o se disparaba con el
    /// monumento equivocado. El estado se lleva ahora por TrackableId.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class TrackingLimitedTimeoutWatcher : MonoBehaviour
    {
        private const string LOG_TAG = "[Sendero/TLW]";

        [SerializeField] private ARTrackedImageManager _trackedImageManager;
        [SerializeField] private float _limitedTimeoutSeconds = 5f;

        private readonly Dictionary<TrackableId, TrackingState> _lastStates = new();
        private readonly Dictionary<TrackableId, Coroutine> _watchdogs = new();

        private void Awake()
        {
            if (_trackedImageManager == null)
            {
                _trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
            }

            if (_trackedImageManager == null)
            {
                Debug.LogError(
                    $"{LOG_TAG} No hay ARTrackedImageManager en la escena. " +
                    "El watchdog de Limited queda inerte.", this);
            }
        }

        private void OnEnable()
        {
            if (_trackedImageManager != null)
            {
                _trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
            }
        }

        private void OnDisable()
        {
            if (_trackedImageManager != null)
            {
                _trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
            }
            StopAllWatchdogs();
        }

        private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
        {
            foreach (var img in args.added)
            {
                EvaluateState(img.trackableId, img.referenceImage.name, img.trackingState);
            }
            foreach (var img in args.updated)
            {
                EvaluateState(img.trackableId, img.referenceImage.name, img.trackingState);
            }
            foreach (var img in args.removed)
            {
                StopWatchdog(img.trackableId);
                _lastStates.Remove(img.trackableId);
            }
        }

        private void EvaluateState(TrackableId id, string imageName, TrackingState newState)
        {
            _lastStates.TryGetValue(id, out var previous);

            if (newState == TrackingState.Limited && previous != TrackingState.Limited)
            {
                StopWatchdog(id);
                _lastStates[id] = newState;
                _watchdogs[id] = StartCoroutine(LimitedTimeoutRoutine(id, imageName));
                return;
            }

            if (newState != TrackingState.Limited)
            {
                StopWatchdog(id);
            }

            _lastStates[id] = newState;
        }

        private IEnumerator LimitedTimeoutRoutine(TrackableId id, string imageName)
        {
            yield return new WaitForSeconds(_limitedTimeoutSeconds);

            if (_lastStates.TryGetValue(id, out var state) && state == TrackingState.Limited)
            {
                Debug.LogWarning(
                    $"{LOG_TAG} '{imageName}' lleva mas de {_limitedTimeoutSeconds:F1}s en " +
                    "TrackingState.Limited. Sprint 3 IARTrackingService forzara el despawn " +
                    "con este mismo disparador.");
            }

            _watchdogs.Remove(id);
        }

        private void StopWatchdog(TrackableId id)
        {
            if (_watchdogs.TryGetValue(id, out var routine))
            {
                if (routine != null)
                    StopCoroutine(routine);
                _watchdogs.Remove(id);
            }
        }

        private void StopAllWatchdogs()
        {
            foreach (var routine in _watchdogs.Values)
            {
                if (routine != null)
                    StopCoroutine(routine);
            }
            _watchdogs.Clear();
            _lastStates.Clear();
        }
    }
}
