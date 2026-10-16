using System.Collections.Generic;
using NUnit.Framework;
using SenderoAR.Models;
using UnityEditor;

namespace SenderoAR.Tests.EditMode
{
    /// <summary>
    /// Integridad de los datos REALES: SenderoMonumentCatalog.asset y los 5
    /// MonumentDataSO que referencia.
    /// </summary>
    /// <remarks>
    /// Complementa a Tools/validate_project.py (que corre sin Unity) con las
    /// reglas que solo se pueden leer con el AssetDatabase. No verifica que
    /// AnchorImageName exista en la libreria de tracking: eso lo hace el
    /// validador de CI, y hoy da los 5 avisos esperados.
    /// </remarks>
    public sealed class MonumentCatalogAssetTests
    {
        private const string CatalogPath = "Assets/_SenderoAR/Models/SenderoMonumentCatalog.asset";

        private static readonly string[] CanonicalOrder =
        {
            "mon_01_templo",
            "mon_02_capilla",
            "mon_03_torre",
            "mon_04_boveda",
            "mon_05_pila",
        };

        private MonumentCatalogSO _catalog;

        [OneTimeSetUp]
        public void LoadCatalog()
        {
            _catalog = AssetDatabase.LoadAssetAtPath<MonumentCatalogSO>(CatalogPath);
            Assert.IsNotNull(_catalog, $"No se encontro el catalogo en {CatalogPath}.");
        }

        [Test]
        public void Catalog_HasFiveMonumentsInCanonicalOrder()
        {
            Assert.AreEqual(CanonicalOrder.Length, _catalog.Count);
            for (int i = 0; i < CanonicalOrder.Length; i++)
            {
                Assert.IsNotNull(_catalog.All[i], $"Slot {i} vacio en el catalogo.");
                Assert.AreEqual(CanonicalOrder[i], _catalog.All[i].MonumentId);
            }
        }

        [Test]
        public void EveryMonument_IdMatchesAssetName_AndIsUnique()
        {
            var seen = new HashSet<string>();
            foreach (var monument in _catalog.All)
            {
                Assert.AreEqual(monument.name, monument.MonumentId,
                    "MonumentId tiene que coincidir con el nombre del .asset.");
                Assert.IsTrue(seen.Add(monument.MonumentId),
                    $"MonumentId duplicado: {monument.MonumentId}");
            }
        }

        [Test]
        public void EveryMonument_IsResolvableThroughTryGetById()
        {
            foreach (var id in CanonicalOrder)
            {
                Assert.IsTrue(_catalog.TryGetById(id, out var monument), id);
                Assert.AreEqual(id, monument.MonumentId);
            }
        }

        [Test]
        public void EveryMonument_HasTrackingDataWithinBounds()
        {
            foreach (var monument in _catalog.All)
            {
                string id = monument.MonumentId;
                Assert.IsFalse(string.IsNullOrWhiteSpace(monument.AnchorImageName), $"{id}: AnchorImageName vacio.");
                Assert.Greater(monument.PhysicalSize.x, 0f, $"{id}: PhysicalSize.x");
                Assert.Greater(monument.PhysicalSize.y, 0f, $"{id}: PhysicalSize.y");
                Assert.That(monument.TrackingConfidenceThreshold, Is.InRange(0f, 1f), $"{id}: umbral");
            }
        }

        [Test]
        public void EveryMonument_HasSpanishDescription()
        {
            // EN y PT estan vacias a proposito hasta la localizacion: no se exigen.
            foreach (var monument in _catalog.All)
            {
                Assert.IsNotNull(monument.Description, $"{monument.MonumentId}: Description null.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(monument.Description.Es),
                    $"{monument.MonumentId}: falta la descripcion ES.");
            }
        }
    }
}
