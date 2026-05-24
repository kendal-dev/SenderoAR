using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SenderoAR.Core.Diagnostics
{
    /// <summary>
    /// Disposable runtime overlay for Sprint 1 Day 2 device validation.
    /// Renders FPS, current AR tracking state, last tracked image name
    /// and Limited-state duration counter via IMGUI.
    /// </summary>
    /// <remarks>
    /// Lifecycle: Sprint 1 only. Removed before Sprint 3 MVVM refactor.
    /// Threading: Unity main thread (Update + OnGUI).
    /// Allocation: zero per-frame string heap pressure beyond the four labels.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class FPSDiagnosticsHUD : MonoBehaviour
    {
        [SerializeField] private ARTrackedImageManager _trackedImageManager;
        [SerializeField] private int _fontSize = 28;
        [SerializeField] private float _sampleInterval = 0.5f;

        private float _accumulatedFrames;
        private float _accumulatedTime;
        private float _currentFps;

        private TrackingState _lastTrackingState = TrackingState.None;
        private string _lastImageName = "—";
        private float _limitedStateStartTime;
        private float _limitedStateDuration;

        private GUIStyle _labelStyle;

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
        }

        private void Update()
        {
            _accumulatedFrames += 1f;
            _accumulatedTime += Time.unscaledDeltaTime;

            if (_accumulatedTime >= _sampleInterval)
            {
                _currentFps = _accumulatedFrames / _accumulatedTime;
                _accumulatedFrames = 0f;
                _accumulatedTime = 0f;
            }

            if (_lastTrackingState == TrackingState.Limited)
            {
                _limitedStateDuration = Time.unscaledTime - _limitedStateStartTime;
            }
        }

        private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
        {
            foreach (var img in args.added)
            {
                UpdateState(img.trackingState, img.referenceImage.name);
            }
            foreach (var img in args.updated)
            {
                UpdateState(img.trackingState, img.referenceImage.name);
            }
            foreach (var img in args.removed)
            {
                UpdateState(TrackingState.None, "—");
            }
        }

        private void UpdateState(TrackingState newState, string imageName)
        {
            if (newState == TrackingState.Limited && _lastTrackingState != TrackingState.Limited)
            {
                _limitedStateStartTime = Time.unscaledTime;
                _limitedStateDuration = 0f;
            }
            else if (newState != TrackingState.Limited)
            {
                _limitedStateDuration = 0f;
            }

            _lastTrackingState = newState;
            _lastImageName = imageName;
        }

        private void OnGUI()
        {
            EnsureStyle();

            var rect = new Rect(16f, 16f, 640f, 220f);
            GUI.Box(rect, GUIContent.none);

            GUI.Label(new Rect(rect.x + 12f, rect.y + 8f, rect.width, 40f),
                $"FPS: {_currentFps:F1}", _labelStyle);

            GUI.Label(new Rect(rect.x + 12f, rect.y + 56f, rect.width, 40f),
                $"Track: {_lastTrackingState}", _labelStyle);

            GUI.Label(new Rect(rect.x + 12f, rect.y + 104f, rect.width, 40f),
                $"Image: {_lastImageName}", _labelStyle);

            GUI.Label(new Rect(rect.x + 12f, rect.y + 152f, rect.width, 40f),
                $"Limited: {_limitedStateDuration:F1}s", _labelStyle);
        }

        private void EnsureStyle()
        {
            if (_labelStyle != null) return;

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = _fontSize,
                normal = { textColor = Color.white }
            };
        }
    }
}