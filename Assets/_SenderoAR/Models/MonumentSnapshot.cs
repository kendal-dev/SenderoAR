// File: Assets/_SenderoAR/Models/MonumentSnapshot.cs
// Sprint 0 · Día 5 · Commit #21

using System;
using System.Collections.Generic;

namespace SenderoAR.Models
{
    /// <summary>
    /// Vista inmutable de los datos de un monumento, lista para ser consumida
    /// por un ViewModel. Es un POCO puro: cero referencias a UnityEngine,
    /// cero lógica de presentación, cero I/O.
    /// </summary>
    /// <remarks>
    /// POR QUÉ "Snapshot" en el nombre: indica explícitamente que es un
    /// retrato del estado en un instante. Si el repositorio detrás cambia
    /// (recarga JSON, sincroniza Firestore), este objeto NO se actualiza.
    /// El consumidor debe re-pedir al repositorio.
    ///
    /// POR QUÉ INMUTABLE (campos readonly + constructor): previene que un
    /// ViewModel descuidado mute los datos del catálogo y corrompa el estado
    /// global. Es la misma filosofía que "S3 objects are immutable" en AWS.
    ///
    /// POR QUÉ DICCIONARIO PARA DESCRIPCIONES: indexar por LanguageCode
    /// permite extender idiomas sin cambiar la clase. Trade-off consciente:
    /// agrega ~40 bytes por instance, pero solo tenemos 5 monumentos.
    /// </remarks>
    public sealed class MonumentSnapshot
    {
        /// <summary>Identificador estable del monumento. Ej: "mon_01_templo".</summary>
        public string Identifier { get; }

        /// <summary>Nombre mostrable. Ej: "Templo de San José de Chiquitos".</summary>
        public string DisplayName { get; }

        /// <summary>Año de fundación (rango inferior si es un período). Ej: 1745.</summary>
        public int FoundationYear { get; }

        /// <summary>
        /// Nombre EXACTO de la imagen de tracking dentro de la XRReferenceImageLibrary.
        /// Debe coincidir con el archivo subido (ej: "mon_01_templo_1230").
        /// </summary>
        public string AnchorImageName { get; }

        /// <summary>
        /// Descripciones por idioma. Lookup garantizado para los 3 LanguageCode.
        /// </summary>
        public IReadOnlyDictionary<LanguageCode, string> Descriptions { get; }

        public MonumentSnapshot(
            string identifier,
            string displayName,
            int foundationYear,
            string anchorImageName,
            IReadOnlyDictionary<LanguageCode, string> descriptions)
        {
            // Fail-fast: si alguien construye este objeto con basura,
            // queremos crashear acá, NO 50 frames después en el ViewModel.
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("identifier no puede estar vacío.", nameof(identifier));
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("displayName no puede estar vacío.", nameof(displayName));
            if (string.IsNullOrWhiteSpace(anchorImageName))
                throw new ArgumentException("anchorImageName no puede estar vacío.", nameof(anchorImageName));

            Identifier = identifier;
            DisplayName = displayName;
            FoundationYear = foundationYear;
            AnchorImageName = anchorImageName;
            Descriptions = descriptions ?? throw new ArgumentNullException(nameof(descriptions));
        }
    }
}