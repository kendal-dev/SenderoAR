using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SenderoAR.Core.Diagnostics
{
    /// <summary>
    /// Overlay descartable para la validacion en dispositivo del Sprint 1.
    /// Muestra FPS, estado de tracking, ultima imagen trackeada y duracion del
    /// estado Limited via IMGUI.
    /// </summary>
    /// <remarks>
    /// Ciclo de vida: solo Sprint 1. Se elimina antes del refactor MVVM del Sprint 3.
    /// Threading: Unity main thread (Update + OnGUI).
    ///
    /// ALLOCATION: OnGUI se ejecuta varias veces por frame (Layout y Repaint). La
    /// version anterior interpolaba las cuatro etiquetas dentro de OnGUI, o sea
    /// ~8 strings nuevos por frame: el instrumento que mide los FPS estaba
    /// generando la basura que hace caer los FPS. Las etiquetas se construyen
    /// ahora solo cuando su valor cambia.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class FPSDiagnosticsHUD : MonoBehaviour
    {
        [SerializeField] private ARTrackedImageManager _trackedImageManager;
        [SerializeField] private int _fontSize = 28;
        [SerializeField] private float _sampleInterval = 0.5f;

        private float _accumulatedFrames;
        private float _accumulatedTime;

        private readonly Dictionary<TrackableId, TrackingState> _states = new();
        private TrackableId _displayedId;
        private TrackingState _lastTrackingState = TrackingState.None;
        private string _lastImageName = "-";
        private float _limitedStateStartTime;
        private float _limitedStateDuration;
        private float _lastRenderedDuration = -1f;

        private string _fpsLabel = "FPS: --";
        private string _trackLabel = "Track: None";
        private string _imageLabel = "Image: -";
        private string _limitedLabel = "Limited: 0.0s";

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
                var fps = _accumulatedFrames / _accumulatedTime;
                _fpsLabel = string.Concat("FPS: ", fps.ToString("F1"));
                _accumulatedFrames = 0f;
                _accumulatedTime = 0f;
            }

            if (_lastTrackingState != TrackingState.Limited)
                return;

            _limitedStateDuration = Time.unscaledTime - _limitedStateStartTime;

            // Reconstruir la etiqueta solo cuando cambia la decima de segundo.
            var rounded = Mathf.Floor(_limitedStateDuration * 10f) / 10f;
            if (!Mathf.Approximately(rounded, _lastRenderedDuration))
            {
                _lastRenderedDuration = rounded;
                _limitedLabel = string.Concat("Limited: ", rounded.ToString("F1"), "s");
            }
        }

        private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
        {
            foreach (var img in args.added)
            {
                _states[img.trackableId] = img.trackingState;
                UpdateState(img.trackableId, img.trackingState, img.referenceImage.name);
            }
            foreach (var img in args.updated)
            {
                _states[img.trackableId] = img.trackingState;
                UpdateState(img.trackableId, img.trackingState, img.referenceImage.name);
            }
            foreach (var img in args.removed)
            {
                _states.Remove(img.trackableId);

                // Solo limpiar el HUD si la imagen que se fue es la que se estaba
                // mostrando. Antes, perder una imagen borraba el estado de otra
                // que seguia trackeando.
                if (img.trackableId == _displayedId)
                {
                    UpdateState(TrackableId.invalidId, TrackingState.None, "-");
                }
            }
        }

        private void UpdateState(TrackableId id, TrackingState newState, string imageName)
        {
            if (newState == TrackingState.Limited && _lastTrackingState != TrackingState.Limited)
            {
                _limitedStateStartTime = Time.unscaledTime;
                _limitedStateDuration = 0f;
            }
            else if (newState != TrackingState.Limited)
            {
                _limitedStateDuration = 0f;
                _lastRenderedDuration = -1f;
                _limitedLabel = "Limited: 0.0s";
            }

            if (newState != _lastTrackingState)
            {
                _lastTrackingState = newState;
                _trackLabel = string.Concat("Track: ", newState.ToString());
            }

            if (!string.Equals(imageName, _lastImageName))
            {
                _lastImageName = imageName;
                _imageLabel = string.Concat("Image: ", imageName);
            }

            _displayedId = id;
        }

        private void OnGUI()
        {
            EnsureStyle();

            var rect = new Rect(16f, 16f, 640f, 220f);
            GUI.Box(rect, GUIContent.none);

            GUI.Label(new Rect(rect.x + 12f, rect.y + 8f, rect.width, 40f), _fpsLabel, _labelStyle);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 56f, rect.width, 40f), _trackLabel, _labelStyle);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 104f, rect.width, 40f), _imageLabel, _labelStyle);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 152f, rect.width, 40f), _limitedLabel, _labelStyle);
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
