using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SenderoAR.Core.Repositories;
using SenderoAR.Models;
using UnityEditor;

namespace SenderoAR.Tests.EditMode
{
    /// <summary>
    /// Contrato de IMonumentRepository cumplido por CatalogMonumentRepository.
    /// </summary>
    /// <remarks>
    /// La implementacion devuelve Task ya completados, asi que los tests leen el
    /// resultado con GetAwaiter().GetResult(): el Unity Test Framework 1.1 no
    /// soporta metodos de test async.
    /// </remarks>
    public sealed class CatalogMonumentRepositoryTests
    {
        private TestAssets _assets;

        [SetUp]
        public void SetUp() => _assets = new TestAssets();

        [TearDown]
        public void TearDown() => _assets.Dispose();

        [Test]
        public void Constructor_NullCatalog_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new CatalogMonumentRepository(null));
        }

        [Test]
        public void IsReady_AfterConstruction_IsTrue()
        {
            var repository = new CatalogMonumentRepository(_assets.Catalog());

            Assert.IsTrue(repository.IsReady);
        }

        [Test]
        public void GetByIdAsync_ExistingId_MapsEveryField()
        {
            var templo = _assets.Monument("mon_01_templo", "Templo", "templo_san_jose", 1745, "Texto ES");
            templo.Description.En = "Text EN";
            var repository = new CatalogMonumentRepository(_assets.Catalog(templo));

            var snapshot = repository.GetByIdAsync("mon_01_templo").GetAwaiter().GetResult();

            Assert.IsNotNull(snapshot);
            Assert.AreEqual("mon_01_templo", snapshot.Identifier);
            Assert.AreEqual("Templo", snapshot.DisplayName);
            Assert.AreEqual(1745, snapshot.FoundationYear);
            Assert.AreEqual("templo_san_jose", snapshot.AnchorImageName);
            Assert.AreEqual("Texto ES", snapshot.Descriptions[LanguageCode.EsBO]);
            Assert.AreEqual("Text EN", snapshot.Descriptions[LanguageCode.EnUS]);
        }

        [Test]
        public void GetByIdAsync_MissingTranslation_IsEmptyStringNotMissingKey()
        {
            // MonumentSnapshot promete lookup para los 3 idiomas; hoy EN y PT estan vacios.
            var repository = new CatalogMonumentRepository(_assets.Catalog(_assets.Monument("mon_01_templo")));

            var snapshot = repository.GetByIdAsync("mon_01_templo").GetAwaiter().GetResult();

            Assert.AreEqual(3, snapshot.Descriptions.Count);
            Assert.AreEqual(string.Empty, snapshot.Descriptions[LanguageCode.EnUS]);
            Assert.AreEqual(string.Empty, snapshot.Descriptions[LanguageCode.PtBR]);
        }

        [Test]
        public void GetByIdAsync_NullDescription_AllLanguagesEmpty()
        {
            var monument = _assets.Monument("mon_01_templo");
            monument.Description = null;
            var repository = new CatalogMonumentRepository(_assets.Catalog(monument));

            var snapshot = repository.GetByIdAsync("mon_01_templo").GetAwaiter().GetResult();

            Assert.AreEqual(string.Empty, snapshot.Descriptions[LanguageCode.EsBO]);
            Assert.AreEqual(string.Empty, snapshot.Descriptions[LanguageCode.EnUS]);
            Assert.AreEqual(string.Empty, snapshot.Descriptions[LanguageCode.PtBR]);
        }

        [TestCase("mon_99_desconocido")]
        [TestCase("")]
        [TestCase(null)]
        public void GetByIdAsync_UnknownOrEmptyId_ReturnsNull(string id)
        {
            // Contrato de IMonumentRepository: null si el identificador no existe.
            var repository = new CatalogMonumentRepository(_assets.Catalog(_assets.Monument("mon_01_templo")));

            Assert.IsNull(repository.GetByIdAsync(id).GetAwaiter().GetResult());
        }

        [Test]
        public void GetAllAsync_KeepsCatalogOrder()
        {
            var catalog = _assets.Catalog(
                _assets.Monument("mon_03_torre", "Torre"),
                _assets.Monument("mon_01_templo", "Templo"));
            var repository = new CatalogMonumentRepository(catalog);

            var all = repository.GetAllAsync().GetAwaiter().GetResult();

            Assert.AreEqual(2, all.Count);
            Assert.AreEqual("mon_03_torre", all[0].Identifier);
            Assert.AreEqual("mon_01_templo", all[1].Identifier);
        }

        [Test]
        public void GetAllAsync_EmptyCatalog_ReturnsEmptyListNotNull()
        {
            var repository = new CatalogMonumentRepository(_assets.Catalog());

            var all = repository.GetAllAsync().GetAwaiter().GetResult();

            Assert.IsNotNull(all);
            Assert.AreEqual(0, all.Count);
        }

        [Test]
        public void GetAllAsync_ResultCannotBeMutatedByCallers()
        {
            var repository = new CatalogMonumentRepository(_assets.Catalog(_assets.Monument("mon_01_templo")));

            var all = repository.GetAllAsync().GetAwaiter().GetResult();

            Assert.IsNotInstanceOf<MonumentSnapshot[]>(all, "Un arreglo se podria castear y modificar.");
            Assert.IsNotInstanceOf<System.Collections.Generic.List<MonumentSnapshot>>(all);
        }

        [Test]
        public void Constructor_SkipsNullCatalogEntries()
        {
            var repository = new CatalogMonumentRepository(
                _assets.Catalog(null, _assets.Monument("mon_03_torre", "Torre")));

            var all = repository.GetAllAsync().GetAwaiter().GetResult();

            Assert.AreEqual(1, all.Count);
            Assert.AreEqual("mon_03_torre", all[0].Identifier);
        }

        [Test]
        public void Constructor_DuplicateIds_ThrowsNamingTheId()
        {
            var catalog = _assets.Catalog(
                _assets.Monument("mon_01_templo", "Uno"),
                _assets.Monument("mon_01_templo", "Dos"));

            var ex = Assert.Throws<InvalidOperationException>(() => new CatalogMonumentRepository(catalog));
            StringAssert.Contains("mon_01_templo", ex.Message);
        }

        [Test]
        public void Constructor_MonumentWithoutDisplayName_ThrowsNamingTheMonument()
        {
            var catalog = _assets.Catalog(_assets.Monument("mon_02_capilla", displayName: ""));

            var ex = Assert.Throws<InvalidOperationException>(() => new CatalogMonumentRepository(catalog));
            StringAssert.Contains("mon_02_capilla", ex.Message);
            Assert.IsInstanceOf<ArgumentException>(ex.InnerException);
        }

        [Test]
        public void CancelledToken_ReturnsCancelledTasks()
        {
            var repository = new CatalogMonumentRepository(_assets.Catalog(_assets.Monument("mon_01_templo")));
            var cancelled = new CancellationToken(canceled: true);

            Assert.IsTrue(repository.GetByIdAsync("mon_01_templo", cancelled).IsCanceled);
            Assert.IsTrue(repository.GetAllAsync(cancelled).IsCanceled);
        }

        [Test]
        public void RealCatalog_BuildsFiveSnapshotsInCanonicalOrder()
        {
            // Integracion con los datos reales: si un .asset tiene un campo que
            // MonumentSnapshot rechaza, este test lo dice antes de que llegue al teléfono.
            var catalog = AssetDatabase.LoadAssetAtPath<MonumentCatalogSO>(
                "Assets/_SenderoAR/Models/SenderoMonumentCatalog.asset");
            Assert.IsNotNull(catalog);

            var all = new CatalogMonumentRepository(catalog).GetAllAsync().GetAwaiter().GetResult();

            CollectionAssert.AreEqual(
                new[] { "mon_01_templo", "mon_02_capilla", "mon_03_torre", "mon_04_boveda", "mon_05_pila" },
                System.Linq.Enumerable.Select(all, s => s.Identifier));
        }
    }
}
