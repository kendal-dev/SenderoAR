// File: Assets/_SenderoAR/Models/MonumentTrackingPhase.cs
// Sprint 0 · Día 5 · Commit #22

namespace SenderoAR.Models
{
    /// <summary>
    /// Fase de tracking de un monumento desde la óptica del dominio.
    /// </summary>
    /// <remarks>
    /// POR QUÉ NO USAR TrackingState DE UnityEngine.XR.ARSubsystems: ese enum
    /// pertenece a un assembly de Unity. Si lo expusiéramos aquí, este archivo
    /// no podría vivir en App.Domain.asmdef (que es noEngineReferences:true).
    ///
    /// MAPEO 1:1 con la API legacy de AR Foundation 5.2:
    ///   TrackingState.None     → Hidden
    ///   TrackingState.Limited  → Dimmed (dead reckoning, posiblemente fuera de cámara)
    ///   TrackingState.Tracking → Active
    ///
    /// Removed (cuando ARTrackedImage entra en eventArgs.removed) → un evento
    /// separado, no una fase: representa el fin del ciclo de vida del tracking.
    /// </remarks>
    public enum MonumentTrackingPhase
    {
        /// <summary>El monumento no está visible ni inferible. Equivalente a TrackingState.None.</summary>
        Hidden = 0,

        /// <summary>Pose inferida por VIO sin ver la imagen. Mantener 5s, luego despawn. Equivalente a TrackingState.Limited.</summary>
        Dimmed = 1,

        /// <summary>Tracking activo y confiable. Equivalente a TrackingState.Tracking.</summary>
        Active = 2
    }
}