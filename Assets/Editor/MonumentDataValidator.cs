using UnityEditor;
using UnityEngine;
using SenderoAR.Models;

namespace SenderoAR.EditorTools
{
    public static class MonumentDataValidator
    {
        private const string MENU_PATH = "Sendero AR/Validate Monument Data";

        [MenuItem(MENU_PATH)]
        public static void ValidateAll()
        {
            var guids = AssetDatabase.FindAssets("t:MonumentDataSO");
            int errors = 0;
            int warnings = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mon = AssetDatabase.LoadAssetAtPath<MonumentDataSO>(path);
                if (mon == null) continue;

                if (string.IsNullOrWhiteSpace(mon.MonumentId))
                {
                    Debug.LogError($"[MonumentValidator] Empty MonumentId at {path}", mon);
                    errors++;
                }

                if (string.IsNullOrWhiteSpace(mon.AnchorImageName))
                {
                    Debug.LogWarning($"[MonumentValidator] Empty AnchorImageName at {path}", mon);
                    warnings++;
                }

                if (mon.PhysicalSize.x <= 0f || mon.PhysicalSize.y <= 0f)
                {
                    Debug.LogError($"[MonumentValidator] Non-positive PhysicalSize at {path}: {mon.PhysicalSize}", mon);
                    errors++;
                }

                if (mon.FoundationYear <= 0)
                {
                    Debug.LogWarning($"[MonumentValidator] Suspicious FoundationYear at {path}: {mon.FoundationYear}", mon);
                    warnings++;
                }

                if (mon.TrackingConfidenceThreshold < 0f || mon.TrackingConfidenceThreshold > 1f)
                {
                    Debug.LogError($"[MonumentValidator] TrackingConfidenceThreshold out of [0..1] at {path}: {mon.TrackingConfidenceThreshold}", mon);
                    errors++;
                }
            }

            Debug.Log($"[MonumentValidator] Audited {guids.Length} assets. Errors: {errors}. Warnings: {warnings}.");
        }
    }
}