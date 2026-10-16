using NUnit.Framework;
using SenderoAR.Models;
using UnityEngine;

namespace SenderoAR.Tests.EditMode
{
    /// <summary>
    /// Contrato de lookup de MonumentCatalogSO: TryGetById nunca lanza ni
    /// devuelve basura (decision A8 de docs/00_Contexto_SenderoAR.md).
    /// </summary>
    /// <remarks>
    /// El arreglo _monuments es privado y serializado: se carga con
    /// TestAssets.SetMonuments (SerializedObject, el mismo camino que usa el Inspector) en vez de
    /// reflexion sobre el campo. Si el campo cambia de nombre, el test falla
    /// en FindProperty con un mensaje claro.
    /// </remarks>
    public sealed class MonumentCatalogSOTests
    {
        private MonumentCatalogSO _catalog;
        private MonumentDataSO _templo;
        private MonumentDataSO _torre;

        [SetUp]
        public void SetUp()
        {
            _templo = CreateMonument("mon_01_templo");
            _torre = CreateMonument("mon_03_torre");
            _catalog = ScriptableObject.CreateInstance<MonumentCatalogSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_catalog);
            Object.DestroyImmediate(_templo);
            Object.DestroyImmediate(_torre);
        }

        [Test]
        public void TryGetById_ExistingId_ReturnsTrueAndMonument()
        {
            SetMonuments(_templo, _torre);

            bool found = _catalog.TryGetById("mon_03_torre", out var monument);

            Assert.IsTrue(found);
            Assert.AreSame(_torre, monument);
        }

        [Test]
        public void TryGetById_UnknownId_ReturnsFalseAndNull()
        {
            SetMonuments(_templo, _torre);

            bool found = _catalog.TryGetById("mon_05_scv", out var monument);

            Assert.IsFalse(found);
            Assert.IsNull(monument);
        }

        [TestCase(null)]
        [TestCase("")]
        public void TryGetById_NullOrEmptyId_ReturnsFalseWithoutThrowing(string id)
        {
            SetMonuments(_templo);

            bool found = _catalog.TryGetById(id, out var monument);

            Assert.IsFalse(found);
            Assert.IsNull(monument);
        }

        [Test]
        public void TryGetById_EmptyCatalog_ReturnsFalse()
        {
            bool found = _catalog.TryGetById("mon_01_templo", out var monument);

            Assert.IsFalse(found);
            Assert.IsNull(monument);
            Assert.AreEqual(0, _catalog.Count);
        }

        [Test]
        public void TryGetById_SkipsNullEntries()
        {
            // Un slot vacio en el Inspector deja un null en el arreglo.
            SetMonuments(null, _torre);

            bool found = _catalog.TryGetById("mon_03_torre", out var monument);

            Assert.IsTrue(found);
            Assert.AreSame(_torre, monument);
        }

        [Test]
        public void TryGetById_IsCaseSensitive()
        {
            // Los IDs son el nombre del .asset; "MON_01_TEMPLO" no es el mismo
            // monumento. Documenta el comportamiento actual.
            SetMonuments(_templo);

            Assert.IsFalse(_catalog.TryGetById("MON_01_TEMPLO", out _));
        }

        private static MonumentDataSO CreateMonument(string id)
        {
            var monument = ScriptableObject.CreateInstance<MonumentDataSO>();
            monument.name = id;
            monument.MonumentId = id;
            return monument;
        }

        private void SetMonuments(params MonumentDataSO[] monuments)
        {
            TestAssets.SetMonuments(_catalog, monuments);
        }
    }
}
