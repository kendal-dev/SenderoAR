using NUnit.Framework;
using SenderoAR.Core.Infrastructure;
using SenderoAR.Models;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace SenderoAR.Tests.EditMode
{
    /// <summary>
    /// Lo que _App_Boot.unity tiene que traer cableado para que el Composition
    /// Root pueda registrar los servicios.
    /// </summary>
    /// <remarks>
    /// La escena se abre en modo aditivo y se cierra al final. En modo edicion
    /// AppBootstrapper.Awake no corre: el test solo lee la serializacion.
    /// </remarks>
    public sealed class BootSceneTests
    {
        private const string BootScenePath = "Assets/_SenderoAR/Scenes/_App_Boot.unity";
        private const string CatalogPath = "Assets/_SenderoAR/Models/SenderoMonumentCatalog.asset";

        private Scene _scene;

        [SetUp]
        public void OpenBootScene()
        {
            _scene = EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Additive);
        }

        [TearDown]
        public void CloseBootScene()
        {
            if (_scene.IsValid())
                EditorSceneManager.CloseScene(_scene, removeScene: true);
        }

        [Test]
        public void BootScene_HasExactlyOneAppBootstrapper()
        {
            Assert.AreEqual(1, FindBootstrappers().Length);
        }

        [Test]
        public void AppBootstrapper_HasTheMonumentCatalogAssigned()
        {
            var bootstrapper = FindBootstrappers()[0];
            var property = new SerializedObject(bootstrapper).FindProperty("monumentCatalog");
            Assert.IsNotNull(property, "AppBootstrapper ya no serializa 'monumentCatalog'.");

            var expected = AssetDatabase.LoadAssetAtPath<MonumentCatalogSO>(CatalogPath);
            Assert.AreSame(expected, property.objectReferenceValue,
                "Sin el catalogo asignado, IMonumentRepository no se registra al arrancar.");
        }

        private AppBootstrapper[] FindBootstrappers()
        {
            var found = new System.Collections.Generic.List<AppBootstrapper>();
            foreach (var root in _scene.GetRootGameObjects())
                found.AddRange(root.GetComponentsInChildren<AppBootstrapper>(includeInactive: true));
            return found.ToArray();
        }
    }
}
