using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SenderoAR.EditorTools
{
    public static class FindMissingScripts
    {
        [MenuItem("Sendero/Find Missing Scripts In Open Scenes")]
        public static void FindInOpenScenes()
        {
            int totalMissing = 0;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                UnityEngine.Debug.Log($"[FindMissing] Scanning scene: {scene.name}");
                foreach (var root in scene.GetRootGameObjects())
                {
                    totalMissing += ScanRecursive(root);
                }
            }
            UnityEngine.Debug.Log($"[FindMissing] DONE. Missing components found: {totalMissing}");
        }

        [MenuItem("Sendero/Find Missing Scripts In All Project Assets")]
        public static void FindInProjectAssets()
        {
            int total = 0;
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in prefabGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null) continue;
                int found = ScanRecursive(go, prefixPath: $"[Prefab:{path}] ");
                total += found;
            }
            UnityEngine.Debug.Log($"[FindMissing] DONE Project Assets. Missing in prefabs: {total}");
        }

        private static int ScanRecursive(GameObject go, string prefixPath = "")
        {
            int count = 0;
            var components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    UnityEngine.Debug.LogWarning(
                        $"{prefixPath}Missing component at index {i} on GameObject: {GetHierarchyPath(go)}",
                        go);
                    count++;
                }
            }
            foreach (Transform child in go.transform)
            {
                count += ScanRecursive(child.gameObject, prefixPath);
            }
            return count;
        }

        private static string GetHierarchyPath(GameObject go)
        {
            if (go.transform.parent == null) return go.name;
            return GetHierarchyPath(go.transform.parent.gameObject) + "/" + go.name;
        }
    }
}