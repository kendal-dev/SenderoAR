using System;
using NUnit.Framework;
using SenderoAR.Core.Contracts;
using SenderoAR.Core.Repositories;
// System.AppContext existe: fuera de SenderoAR.Core.Infrastructure, con "using System;",
// el nombre corto es ambiguo (CS0104). docs/09_Lecciones_Unity.md §1.
using AppContext = SenderoAR.Core.Infrastructure.AppContext;

namespace SenderoAR.Tests.EditMode
{
    /// <summary>
    /// Ciclo de vida del Service Locator: Register → Initialize → Resolve.
    /// </summary>
    /// <remarks>
    /// AppContext es estatico: cada test arranca y termina con Reset() para no
    /// heredar servicios de otro test.
    /// </remarks>
    public sealed class AppContextTests
    {
        private TestAssets _assets;

        [SetUp]
        public void SetUp()
        {
            AppContext.Reset();
            _assets = new TestAssets();
        }

        [TearDown]
        public void TearDown()
        {
            AppContext.Reset();
            _assets.Dispose();
        }

        [Test]
        public void RegisterThenInitialize_ResolveReturnsSameInstance()
        {
            var repository = NewRepository();
            AppContext.Register<IMonumentRepository>(repository);
            AppContext.Initialize();

            Assert.AreSame(repository, AppContext.Resolve<IMonumentRepository>());
        }

        [Test]
        public void Resolve_BeforeInitialize_Throws()
        {
            AppContext.Register<IMonumentRepository>(NewRepository());

            Assert.Throws<InvalidOperationException>(() => AppContext.Resolve<IMonumentRepository>());
        }

        [Test]
        public void Resolve_Unregistered_ThrowsNamingTheType()
        {
            AppContext.Initialize();

            var ex = Assert.Throws<InvalidOperationException>(() => AppContext.Resolve<IMonumentRepository>());
            StringAssert.Contains(nameof(IMonumentRepository), ex.Message);
        }

        [Test]
        public void Resolve_ByConcreteType_DoesNotFindServiceRegisteredByContract()
        {
            // La clave es el tipo de Register<T>: se registra y se pide por el contrato.
            AppContext.Register<IMonumentRepository>(NewRepository());
            AppContext.Initialize();

            Assert.IsFalse(AppContext.TryResolve<CatalogMonumentRepository>(out _));
        }

        [Test]
        public void TryResolve_BeforeInitialize_ReturnsFalse()
        {
            AppContext.Register<IMonumentRepository>(NewRepository());

            Assert.IsFalse(AppContext.TryResolve<IMonumentRepository>(out var service));
            Assert.IsNull(service);
        }

        [Test]
        public void TryResolve_Unregistered_ReturnsFalseWithoutThrowing()
        {
            AppContext.Initialize();

            Assert.IsFalse(AppContext.TryResolve<IMonumentRepository>(out var service));
            Assert.IsNull(service);
        }

        [Test]
        public void TryResolve_Registered_ReturnsTrueAndInstance()
        {
            var repository = NewRepository();
            AppContext.Register<IMonumentRepository>(repository);
            AppContext.Initialize();

            Assert.IsTrue(AppContext.TryResolve<IMonumentRepository>(out var service));
            Assert.AreSame(repository, service);
        }

        [Test]
        public void Register_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => AppContext.Register<IMonumentRepository>(null));
        }

        [Test]
        public void Register_SameContractTwice_Throws()
        {
            AppContext.Register<IMonumentRepository>(NewRepository());

            Assert.Throws<InvalidOperationException>(() =>
                AppContext.Register<IMonumentRepository>(NewRepository()));
        }

        [Test]
        public void Register_AfterInitialize_Throws()
        {
            // El registro se cierra al inicializar: la composicion vive en un solo lugar.
            AppContext.Initialize();

            Assert.Throws<InvalidOperationException>(() =>
                AppContext.Register<IMonumentRepository>(NewRepository()));
        }

        [Test]
        public void Reset_ClearsServicesAndReadiness()
        {
            AppContext.Register<IMonumentRepository>(NewRepository());
            AppContext.Initialize();

            AppContext.Reset();

            Assert.IsFalse(AppContext.IsReady);
            AppContext.Initialize();
            Assert.IsFalse(AppContext.TryResolve<IMonumentRepository>(out _));
        }

        private CatalogMonumentRepository NewRepository() =>
            new CatalogMonumentRepository(_assets.Catalog(_assets.Monument("mon_01_templo")));
    }
}
