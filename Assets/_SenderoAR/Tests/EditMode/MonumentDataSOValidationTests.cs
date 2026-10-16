using System.Reflection;
using NUnit.Framework;
using SenderoAR.Models;
using UnityEngine;

namespace SenderoAR.Tests.EditMode
{
    /// <summary>
    /// Invariantes que MonumentDataSO.OnValidate impone en el editor
    /// (decision A11 de docs/00_Contexto_SenderoAR.md).
    /// </summary>
    /// <remarks>
    /// OnValidate es privado y Unity lo llama solo al editar en el Inspector o
    /// al recargar scripts. Se invoca por reflexion para probar la regla en
    /// aislamiento, sin depender de cuando Unity decide dispararlo.
    /// </remarks>
    public sealed class MonumentDataSOValidationTests
    {
        private MonumentDataSO _monument;

        [SetUp]
        public void SetUp()
        {
            _monument = ScriptableObject.CreateInstance<MonumentDataSO>();
            _monument.name = "mon_99_prueba";
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_monument);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void OnValidate_BlankId_TakesAssetName(string blankId)
        {
            _monument.MonumentId = blankId;

            InvokeOnValidate();

            Assert.AreEqual("mon_99_prueba", _monument.MonumentId);
        }

        [Test]
        public void OnValidate_ExistingId_IsNotOverwritten()
        {
            _monument.MonumentId = "mon_01_templo";

            InvokeOnValidate();

            Assert.AreEqual("mon_01_templo", _monument.MonumentId);
        }

        [TestCase(1.7f, 1f)]
        [TestCase(-0.3f, 0f)]
        [TestCase(0.4f, 0.4f)]
        public void OnValidate_ClampsConfidenceThresholdToUnitRange(float input, float expected)
        {
            _monument.MonumentId = "x";
            _monument.TrackingConfidenceThreshold = input;

            InvokeOnValidate();

            Assert.AreEqual(expected, _monument.TrackingConfidenceThreshold, 1e-6f);
        }

        [Test]
        public void OnValidate_NonPositivePhysicalSize_FallsBackToSixtyCentimeters()
        {
            _monument.MonumentId = "x";
            _monument.PhysicalSize = new Vector2(0f, -1f);

            InvokeOnValidate();

            Assert.AreEqual(0.60f, _monument.PhysicalSize.x, 1e-6f);
            Assert.AreEqual(0.60f, _monument.PhysicalSize.y, 1e-6f);
        }

        [Test]
        public void OnValidate_PositivePhysicalSize_IsKeptEvenIfSmallerThanSixty()
        {
            // El marcador validado en el A52s mide 0.20 m. OnValidate solo corrige
            // valores <= 0; no impone un minimo de 0.60 m.
            _monument.MonumentId = "x";
            _monument.PhysicalSize = new Vector2(0.20f, 0.20f);

            InvokeOnValidate();

            Assert.AreEqual(0.20f, _monument.PhysicalSize.x, 1e-6f);
            Assert.AreEqual(0.20f, _monument.PhysicalSize.y, 1e-6f);
        }

        private void InvokeOnValidate()
        {
            var method = typeof(MonumentDataSO).GetMethod(
                "OnValidate", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "MonumentDataSO ya no declara OnValidate.");
            method.Invoke(_monument, null);
        }
    }
}
