using System;
using System.Collections.Generic;
using NUnit.Framework;
using SenderoAR.Models;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SenderoAR.Tests.EditMode
{
    /// <summary>
    /// Crea MonumentDataSO y MonumentCatalogSO en memoria para los tests y los
    /// destruye al final (Dispose en el TearDown).
    /// </summary>
    internal sealed class TestAssets : IDisposable
    {
        private readonly List<Object> _created = new List<Object>();

        public MonumentDataSO Monument(
            string id,
            string displayName = "Nombre de prueba",
            string anchor = "anchor_prueba",
            int foundationYear = 1745,
            string es = "Descripcion ES")
        {
            var monument = ScriptableObject.CreateInstance<MonumentDataSO>();
            monument.name = id;
            monument.MonumentId = id;
            monument.DisplayName = displayName;
            monument.AnchorImageName = anchor;
            monument.FoundationYear = foundationYear;
            monument.Description = new LocalizedDescription { Es = es };
            _created.Add(monument);
            return monument;
        }

        public MonumentCatalogSO Catalog(params MonumentDataSO[] monuments)
        {
            var catalog = ScriptableObject.CreateInstance<MonumentCatalogSO>();
            _created.Add(catalog);
            SetMonuments(catalog, monuments);
            return catalog;
        }

        /// <summary>
        /// Carga el arreglo privado _monuments con SerializedObject, el mismo camino
        /// que usa el Inspector. Si el campo cambia de nombre, falla con un mensaje claro.
        /// </summary>
        public static void SetMonuments(MonumentCatalogSO catalog, params MonumentDataSO[] monuments)
        {
            var serialized = new SerializedObject(catalog);
            var array = serialized.FindProperty("_monuments");
            Assert.IsNotNull(array, "MonumentCatalogSO ya no serializa '_monuments'.");

            array.arraySize = monuments.Length;
            for (int i = 0; i < monuments.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = monuments[i];

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public void Dispose()
        {
            foreach (var obj in _created)
                if (obj != null) Object.DestroyImmediate(obj);
            _created.Clear();
        }
    }
}
