using System.Collections.Generic;
using UnityEngine;

namespace SenderoAR.Models
{
    [CreateAssetMenu(
        fileName = "SenderoMonumentCatalog",
        menuName = "Sendero AR/Monument Catalog",
        order = 101)]
    public sealed class MonumentCatalogSO : ScriptableObject
    {
        [SerializeField] private MonumentDataSO[] _monuments;

        public IReadOnlyList<MonumentDataSO> All => _monuments;
        public int Count => _monuments?.Length ?? 0;

        public bool TryGetById(string monumentId, out MonumentDataSO monument)
        {
            monument = null;
            if (string.IsNullOrEmpty(monumentId) || _monuments == null) return false;

            for (int i = 0; i < _monuments.Length; i++)
            {
                var candidate = _monuments[i];
                if (candidate != null && candidate.MonumentId == monumentId)
                {
                    monument = candidate;
                    return true;
                }
            }
            return false;
        }
    }
}