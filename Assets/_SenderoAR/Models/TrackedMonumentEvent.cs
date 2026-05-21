// File: Assets/_SenderoAR/Models/TrackedMonumentEvent.cs
// Sprint 0 · Día 5 · Commit #22

namespace SenderoAR.Models
{
    /// <summary>
    /// Snapshot inmutable de un evento de tracking. Se emite cada vez que
    /// un monumento aparece, se actualiza, o se pierde.
    /// </summary>
    /// <remarks>
    /// POR QUÉ readonly struct: este evento puede emitirse decenas de veces
    /// por segundo (cada update de tracking). Un struct evita GC pressure que
    /// rompería los 30 FPS en Snapdragon 7 Gen 1. Trade-off: copia por valor,
    /// pero el payload es chico (~40 bytes).
    ///
    /// POR QUÉ NO INCLUYE Pose DE UnityEngine: misma razón que el enum.
    /// El contrato expone solo "qué monumento + en qué fase". El consumidor
    /// que necesite la pose 3D (ej: spawn de modelo) recibe un Pose en una
    /// API separada implementada por el lado Views/, donde sí puede importar
    /// UnityEngine.
    /// </remarks>
    public readonly struct TrackedMonumentEvent
    {
        /// <summary>Identificador del monumento. Coincide con MonumentSnapshot.Identifier.</summary>
        public string MonumentIdentifier { get; }

        /// <summary>Fase actual del tracking.</summary>
        public MonumentTrackingPhase Phase { get; }

        /// <summary>Confianza estimada del tracking (0.0 - 1.0). En API 5.2 es siempre 1.0 cuando Phase=Active.</summary>
        public float Confidence { get; }

        public TrackedMonumentEvent(string monumentIdentifier, MonumentTrackingPhase phase, float confidence)
        {
            MonumentIdentifier = monumentIdentifier;
            Phase = phase;
            Confidence = confidence;
        }
    }
}