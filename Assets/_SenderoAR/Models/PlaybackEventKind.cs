// File: Assets/_SenderoAR/Models/PlaybackEventKind.cs
// Sprint 0 · Día 5 · Commit #25

namespace SenderoAR.Models
{
    /// <summary>
    /// Eventos de ciclo de vida de la reproducción de audio.
    /// </summary>
    public enum PlaybackEventKind
    {
        /// <summary>La reproducción arrancó.</summary>
        Started = 0,
        /// <summary>El usuario o el sistema pausó.</summary>
        Paused = 1,
        /// <summary>Se reanudó tras un pause.</summary>
        Resumed = 2,
        /// <summary>El audio llegó al final naturalmente.</summary>
        Completed = 3,
        /// <summary>Se detuvo prematuramente (cambio de monumento, salida de pantalla).</summary>
        Stopped = 4,
        /// <summary>Error al cargar o decodificar el OGG.</summary>
        Failed = 5
    }
}