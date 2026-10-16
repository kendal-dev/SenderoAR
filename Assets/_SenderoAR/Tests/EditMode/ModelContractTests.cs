using System;
using System.Collections.Generic;
using NUnit.Framework;
using SenderoAR.Models;

namespace SenderoAR.Tests.EditMode
{
    /// <summary>
    /// Reglas de los POCOs de Models/ que el resto del proyecto da por hechas.
    /// </summary>
    public sealed class ModelContractTests
    {
        [Test]
        public void LanguageCode_SerializedValuesAreStable()
        {
            // Los .asset guardan el enum como entero (AddressableAudioReference.Language).
            // Reordenar o renumerar los valores cambiaria el idioma de datos ya guardados.
            Assert.AreEqual(0, (int)LanguageCode.EsBO);
            Assert.AreEqual(1, (int)LanguageCode.EnUS);
            Assert.AreEqual(2, (int)LanguageCode.PtBR);
            Assert.AreEqual(3, Enum.GetValues(typeof(LanguageCode)).Length);
        }

        [Test]
        public void MonumentTrackingPhase_SerializedValuesAreStable()
        {
            Assert.AreEqual(0, (int)MonumentTrackingPhase.Hidden);
            Assert.AreEqual(1, (int)MonumentTrackingPhase.Dimmed);
            Assert.AreEqual(2, (int)MonumentTrackingPhase.Active);
        }

        [Test]
        public void MonumentSnapshot_ValidArguments_KeepsValues()
        {
            var descriptions = Descriptions();

            var snapshot = new MonumentSnapshot(
                "mon_01_templo", "Templo de San José de Chiquitos", 1745, "templo_san_jose", descriptions);

            Assert.AreEqual("mon_01_templo", snapshot.Identifier);
            Assert.AreEqual("Templo de San José de Chiquitos", snapshot.DisplayName);
            Assert.AreEqual(1745, snapshot.FoundationYear);
            Assert.AreEqual("templo_san_jose", snapshot.AnchorImageName);
            Assert.AreSame(descriptions, snapshot.Descriptions);
        }

        [TestCase(null, "Nombre", "anchor")]
        [TestCase(" ", "Nombre", "anchor")]
        [TestCase("mon_01_templo", "", "anchor")]
        [TestCase("mon_01_templo", "Nombre", null)]
        public void MonumentSnapshot_BlankRequiredField_ThrowsArgumentException(
            string identifier, string displayName, string anchor)
        {
            Assert.Throws<ArgumentException>(() =>
                new MonumentSnapshot(identifier, displayName, 1745, anchor, Descriptions()));
        }

        [Test]
        public void MonumentSnapshot_NullDescriptions_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new MonumentSnapshot("mon_01_templo", "Nombre", 1745, "anchor", null));
        }

        private static IReadOnlyDictionary<LanguageCode, string> Descriptions() =>
            new Dictionary<LanguageCode, string>
            {
                { LanguageCode.EsBO, "es" },
                { LanguageCode.EnUS, "en" },
                { LanguageCode.PtBR, "pt" },
            };
    }
}
