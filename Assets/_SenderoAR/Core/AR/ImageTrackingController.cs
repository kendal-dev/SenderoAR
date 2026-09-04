using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SenderoAR.Core.AR
{
    /// <summary>
    /// Sprint 1 Provisional Controller. Pre-MVVM. Será reemplazado en Sprint 3
    /// por un binding View ↔ MonumentTrackingViewModel sobre IARTrackingService.
    /// </summary>
    [RequireComponent(typeof(ARTrackedImageManager))]
    [DisallowMultipleComponent]
    public sealed class ImageTrackingController : MonoBehaviour
    {
        [Header("Debug Cube Visualization")]
        [SerializeField] private float _debugCubeSize = 0.10f;
        [SerializeField] private Color _activeColor = new Color(0.0f, 0.85f, 1.0f, 1.0f); // Cyan
        [SerializeField] private Color _limitedColor = new Color(1.0f, 0.85f, 0.0f, 1.0f); // Amber
        [SerializeField] private Material _cubeMaterial; // Inyección de dependencia (URP seguro)

        [Header("Performance Budget (Samsung A52s)")]
        [SerializeField] private int _targetFrameRate = 30;

        private ARTrackedImageManager _manager;
        private readonly Dictionary<TrackableId, GameObject> _spawnedCubes = new();
        private readonly Dictionary<TrackableId, Renderer> _spawnedRenderers = new();

        private void Awake()
        {
            Application.targetFrameRate = _targetFrameRate;
            QualitySettings.vSyncCount = 0;

            _manager = GetComponent<ARTrackedImageManager>();
            _manager.requestedMaxNumberOfMovingImages = 1;
        }

        private void OnEnable()
        {
            _manager.trackedImagesChanged += OnTrackedImagesChanged;
        }

        private void OnDisable()
        {
            _manager.trackedImagesChanged -= OnTrackedImagesChanged;
        }

        private void OnDestroy()
        {
            foreach (var entry in _spawnedCubes)
            {
                if (entry.Value != null)
                    Destroy(entry.Value);
            }
            _spawnedCubes.Clear();
            _spawnedRenderers.Clear();
        }

        private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (var added in eventArgs.added)
                HandleAdded(added);

            foreach (var updated in eventArgs.updated)
                HandleUpdated(updated);

            foreach (var removed in eventArgs.removed)
                HandleRemoved(removed);
        }

        private void HandleAdded(ARTrackedImage tracked)
        {
            if (_spawnedCubes.ContainsKey(tracked.trackableId))
                return;

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"DebugCube_{tracked.referenceImage.name}";

            if (cube.TryGetComponent<Collider>(out var collider))
                Destroy(collider);

            cube.transform.SetParent(tracked.transform, worldPositionStays: false);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = Vector3.one * _debugCubeSize;

            var renderer = cube.GetComponent<Renderer>();

            // Asignar el material inyectado de forma segura
            if (_cubeMaterial != null)
            {
                renderer.material = new Material(_cubeMaterial);
            }

            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            _spawnedCubes[tracked.trackableId] = cube;
            _spawnedRenderers[tracked.trackableId] = renderer;

            ApplyTrackingState(tracked);
        }

        private void HandleUpdated(ARTrackedImage tracked)
        {
            if (!_spawnedCubes.ContainsKey(tracked.trackableId))
            {
                HandleAdded(tracked);
                return;
            }

            ApplyTrackingState(tracked);
        }

        private void HandleRemoved(ARTrackedImage tracked)
        {
            if (_spawnedCubes.TryGetValue(tracked.trackableId, out var cube) && cube != null)
                Destroy(cube);

            _spawnedCubes.Remove(tracked.trackableId);
            _spawnedRenderers.Remove(tracked.trackableId);
        }

        private void ApplyTrackingState(ARTrackedImage tracked)
        {
            if (!_spawnedCubes.TryGetValue(tracked.trackableId, out var cube) || cube == null)
                return;

            switch (tracked.trackingState)
            {
                case TrackingState.Tracking:
                    cube.SetActive(true);
                    if (_spawnedRenderers.TryGetValue(tracked.trackableId, out var rendererActive))
                        rendererActive.material.SetColor("_BaseColor", _activeColor); // Corrección URP
                    break;

                case TrackingState.Limited:
                    cube.SetActive(true);
                    if (_spawnedRenderers.TryGetValue(tracked.trackableId, out var rendererLimited))
                        rendererLimited.material.SetColor("_BaseColor", _limitedColor);
                    break;

                case TrackingState.None:
                default:
                    cube.SetActive(false);
                    break;
            }
        }
    }
}