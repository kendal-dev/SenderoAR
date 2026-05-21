// File: Assets/_SenderoAR/Models/LanguageCode.cs
// Sprint 0 · Día 5 · Commit #21

namespace SenderoAR.Models
{
    /// <summary>
    /// Identifica el idioma de visualización y narración del usuario.
    /// </summary>
    /// <remarks>
    /// POR QUÉ ENUM (no string): un enum es type-safe en compile-time.
    /// Si en algún punto del código alguien escribe "es-BO" mal escrito como
    /// "es-bo" o "es_BO", un string lo aceptaría silenciosamente y rompería
    /// en runtime. El enum forzaría el compilador a fallar inmediatamente.
    ///
    /// POR QUÉ TRES VALORES (no más): decisión inmutable del proyecto.
    /// Cualquier idioma adicional requeriría re-grabación TTS completa
    /// (Azure es-BO + ElevenLabs v3) → fuera de scope MVP.
    /// </remarks>
    public enum LanguageCode
    {
        /// <summary>Español de Bolivia. Voz Azure es-BO-MarceloNeural. Idioma base.</summary>
        EsBO = 0,

        /// <summary>Inglés de Estados Unidos. Voz ElevenLabs Bill L. Oxley.</summary>
        EnUS = 1,

        /// <summary>Portugués de Brasil. Voz ElevenLabs Dara de Alcantara.</summary>
        PtBR = 2
    }
}