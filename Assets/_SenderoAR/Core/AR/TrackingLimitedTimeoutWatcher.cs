using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SenderoAR.Core.AR
{
    /// <summary>
    /// Sprint 1 provisional watcher. Emits a warning log when a tracked
    /// image stays in TrackingState.Limited beyond the configured timeout.
    /// </summary>
    /// <remarks>
    /// Implements the VIO Drift Timeout pattern from 03_AR_Tracking_Specs.md §10.
    /// Despawn responsibility is intentionally deferred to the Sprint 3
    /// IARTrackingService implementation; this watcher is observation-only
    /// and never mutates scene state. Threading: Unity main thread.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class TrackingLimitedTimeoutWatcher : MonoBehaviour
    {
        private const string LOG_TAG = "[Sendero/TLW]";

        [SerializeField] private ARTrackedImageManager _trackedImageManager;
        [SerializeField] private float _limitedTimeoutSeconds = 5f;

        private Coroutine _watchdog;
        private TrackingState _lastState = TrackingState.None;

        private void Awake()
        {
            if (_trackedImageManager == null)
            {
                _trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
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
            StopWatchdog();
        }

        private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
        {
            foreach (var img in args.added)
            {
                EvaluateState(img.trackingState);
            }
            foreach (var img in args.updated)
            {
                EvaluateState(img.trackingState);
            }
            foreach (var img in args.removed)
            {
                EvaluateState(TrackingState.None);
            }
        }

        private void EvaluateState(TrackingState newState)
        {
            if (newState == TrackingState.Limited && _lastState != TrackingState.Limited)
            {
                StopWatchdog();
                _watchdog = StartCoroutine(LimitedTimeoutRoutine());
            }
            else if (newState != TrackingState.Limited)
            {
                StopWatchdog();
            }

            _lastState = newState;
        }

        private IEnumerator LimitedTimeoutRoutine()
        {
            yield return new WaitForSeconds(_limitedTimeoutSeconds);

            if (_lastState == TrackingState.Limited)
            {
                UnityEngine.Debug.LogWarning(
                    $"{LOG_TAG} TrackingState.Limited exceeded {_limitedTimeoutSeconds:F1}s. " +
                    "Sprint 3 IARTrackingService will force despawn on this trigger.");
            }

            _watchdog = null;
        }

        private void StopWatchdog()
        {
            if (_watchdog != null)
            {
                StopCoroutine(_watchdog);
                _watchdog = null;
            }
        }
    }
}