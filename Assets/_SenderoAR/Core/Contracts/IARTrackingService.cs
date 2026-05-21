// File: Assets/_SenderoAR/Core/Contracts/IARTrackingService.cs
// Sprint 0 · Día 5 · Commit #22

using System;
using System.Threading;
using System.Threading.Tasks;
using SenderoAR.Models;

namespace SenderoAR.Core.Contracts
{
    /// <summary>
    /// Contrato del servicio de Image Tracking. Abstrae la API legacy de
    /// AR Foundation 5.2 detrás de un stream de eventos de dominio.
    /// </summary>
    /// <remarks>
    /// <para>THREADING CONTRACT: el evento <see cref="MonumentTrackingChanged"/>
    /// SIEMPRE se invoca en el Unity main thread (los callbacks de
    /// AR Foundation viven ahí). Los handlers pueden tocar UI directamente.</para>
    ///
    /// <para>CICLO DE VIDA: el consumidor debe llamar a InitializeAsync una
    /// sola vez, luego suscribirse al evento. Al cerrar la escena AR debe
    /// llamar StopAsync para liberar recursos del subsystem.</para>
    ///
    /// <para>OWNERSHIP DE LA SESSION: este servicio NO crea la ARSession ni
    /// el XROrigin (eso lo hace AppBootstrapper en _App_Boot). Solo opera
    /// sobre el ARTrackedImageManager ya presente en la escena _Main.</para>
    /// </remarks>
    public interface IARTrackingService
    {
        /// <summary>
        /// Evento de cambio de fase de tracking. Se dispara para added/updated/removed
        /// de la API 5.2 (ARTrackedImagesChangedEventArgs) normalizado a un solo callback.
        /// </summary>
        /// <remarks>
        /// El handler corre en main thread. NO bloquear con I/O ni Thread.Sleep.
        /// </remarks>
        event Action<TrackedMonumentEvent> MonumentTrackingChanged;

        /// <summary>
        /// Inicializa el servicio. Verifica disponibilidad del subsystem AR y
        /// se suscribe a ARTrackedImageManager.trackedImagesChanged.
        /// </summary>
        /// <param name="cancellationToken">Cancela el setup si el usuario sale de la escena.</param>
        /// <returns>True si quedó operativo. False si el dispositivo no soporta AR.</returns>
        Task<bool> InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Detiene el servicio y libera la suscripción al ARTrackedImageManager.
        /// </summary>
        /// <remarks>
        /// IDEMPOTENTE: invocarlo dos veces no debe lanzar excepción.
        /// </remarks>
        Task StopAsync();

        /// <summary>
        /// Estado actual del servicio.
        /// </summary>
        bool IsTracking { get; }
    }
}