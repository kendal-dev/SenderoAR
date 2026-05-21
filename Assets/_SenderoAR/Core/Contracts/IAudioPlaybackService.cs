// File: Assets/_SenderoAR/Core/Contracts/IAudioPlaybackService.cs
// Sprint 0 · Día 5 · Commit #25

using System;
using System.Threading;
using System.Threading.Tasks;
using SenderoAR.Models;

namespace SenderoAR.Core.Contracts
{
    /// <summary>
    /// Contrato de reproducción de narraciones de monumentos.
    /// </summary>
    /// <remarks>
    /// <para>EXCLUSIVIDAD: solo UN audio puede estar reproduciéndose a la vez.
    /// Llamar a PlayAsync mientras otro audio está activo PRIMERO emite
    /// PlaybackEventKind.Stopped del anterior, LUEGO Started del nuevo.</para>
    ///
    /// <para>THREADING: todos los métodos retornan Task que completa en main
    /// thread. El evento PlaybackStateChanged se dispara en main thread.</para>
    ///
    /// <para>FORMATO ESPERADO POR LA IMPL: OGG Vorbis 64-96 kbps VBR.
    /// Si en S7 cambia a otro formato, este contrato no se entera.</para>
    /// </remarks>
    public interface IAudioPlaybackService
    {
        /// <summary>True si hay un audio reproduciéndose (no incluye estado Paused).</summary>
        bool IsPlaying { get; }

        /// <summary>El cue actualmente cargado, o null si no hay nada.</summary>
        AudioCue? CurrentCue { get; }

        /// <summary>
        /// Notificación de cambios de ciclo de vida. Payload incluye el cue afectado.
        /// </summary>
        event Action<AudioCue, PlaybackEventKind> PlaybackStateChanged;

        /// <summary>
        /// Arranca la reproducción del cue. Si ya había uno sonando, lo detiene primero.
        /// </summary>
        Task PlayAsync(AudioCue cue, CancellationToken cancellationToken = default);

        /// <summary>Pausa la reproducción manteniendo posición.</summary>
        Task PauseAsync();

        /// <summary>Reanuda desde la posición pausada.</summary>
        Task ResumeAsync();

        /// <summary>Detiene y descarga el cue actual.</summary>
        Task StopAsync();
    }
}