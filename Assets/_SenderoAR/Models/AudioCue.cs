// File: Assets/_SenderoAR/Models/AudioCue.cs
// Sprint 0 · Día 5 · Commit #25

namespace SenderoAR.Models
{
    /// <summary>
    /// Pedido abstracto de reproducción. Identifica QUÉ narración tocar,
    /// sin referenciar AudioClip de UnityEngine.
    /// </summary>
    /// <remarks>
    /// La implementación traduce este cue a un AudioClip cargado desde
    /// Resources, Addressables, o Streaming Assets. El consumidor no decide.
    ///
    /// EJEMPLO DE COMBINACIÓN VÁLIDA:
    ///   new AudioCue("mon_01_templo", LanguageCode.EsBO)
    ///   → la impl resuelve a "Audio/mon_01_templo_es-BO.ogg"
    /// </remarks>
    public readonly struct AudioCue
    {
        /// <summary>Identificador de monumento. Debe coincidir con MonumentSnapshot.Identifier.</summary>
        public string MonumentIdentifier { get; }

        /// <summary>Idioma de la narración a reproducir.</summary>
        public LanguageCode Language { get; }

        public AudioCue(string monumentIdentifier, LanguageCode language)
        {
            MonumentIdentifier = monumentIdentifier;
            Language = language;
        }
    }
}