// File: Assets/_SenderoAR/Core/Contracts/ILocalizationService.cs
// Sprint 0 · Día 5 · Commit #24

using System;
using SenderoAR.Models;

namespace SenderoAR.Core.Contracts
{
    /// <summary>
    /// Contrato del servicio central de idioma. Custodia el estado de
    /// LanguageCode actual y notifica cambios a la UI de forma reactiva.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>Idioma actualmente activo en la UI.</summary>
        LanguageCode CurrentLanguage { get; }

        /// <summary>
        /// Cambia el idioma activo de forma idempotente (evita disparar el evento si el valor no cambia).
        /// </summary>
        void SetLanguage(LanguageCode newLanguage);

        /// <summary>
        /// Notifica cuando el idioma cambió. El payload es el nuevo valor.
        /// </summary>
        event Action<LanguageCode> LanguageChanged;
    }
}