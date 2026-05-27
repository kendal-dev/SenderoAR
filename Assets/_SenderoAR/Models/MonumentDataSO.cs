using System;
using UnityEngine;

namespace SenderoAR.Models
{
    [CreateAssetMenu(
        fileName = "mon_XX_monument",
        menuName = "Sendero AR/Monument Data",
        order = 100)]
    public sealed class MonumentDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string MonumentId;
        public int FoundationYear;

        [Header("Geolocation")]
        public Vector2 GPSCoordinates;

        [Header("AR Tracking")]
        public string AnchorImageName;
        public Vector2 PhysicalSize;
        [Range(0f, 1f)] public float TrackingConfidenceThreshold = 0.4f;

        [Header("3D Content (Sprint 6)")]
        public GameObject Monument3DPrefab;
        public Vector3 PivotOffset;

        [Header("Localization & Audio")]
        public LocalizedDescription Description;
        public AddressableAudioReference[] NarrationClips;

        [Header("Defense Flags")]
        public bool IsPhysicalInDefense;
    }
}