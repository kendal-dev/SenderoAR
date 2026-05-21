// File: Assets/_SenderoAR/Core/Contracts/IMonumentRepository.cs
// Sprint 0 · Día 5 · Commit #21

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SenderoAR.Models;

namespace SenderoAR.Core.Contracts
{
    /// <summary>
    /// Contrato de acceso al catálogo de monumentos.
    /// </summary>
    /// <remarks>
    /// <para>RESPONSABILIDAD ÚNICA: leer datos. No escribe, no sincroniza,
    /// no cachea (eso es decisión de la implementación, no del contrato).</para>
    ///
    /// <para>THREADING CONTRACT: todos los métodos retornan Task. La implementación
    /// PUEDE ejecutar I/O en thread pool, pero el Task DEBE completarse de forma
    /// segura para ser awaited en el main thread de Unity. Las propiedades del
    /// MonumentSnapshot devuelto NO requieren main thread para ser leídas.</para>
    ///
    /// <para>FUENTE DE DATOS: opaca al consumidor. Hoy (S2) será ScriptableObjects.
    /// Mañana (S8+) podría ser Firestore. El ViewModel no debe enterarse.</para>
    /// </remarks>
    public interface IMonumentRepository
    {
        /// <summary>
        /// Recupera el snapshot completo de UN monumento por su identificador.
        /// </summary>
        /// <param name="identifier">ID estable. Ej: "mon_01_templo".</param>
        /// <param name="cancellationToken">Token para cancelar la operación si el usuario sale de pantalla.</param>
        /// <returns>El snapshot, o null si el identifier no existe en el catálogo.</returns>
        Task<MonumentSnapshot> GetByIdAsync(string identifier, CancellationToken cancellationToken = default);

        /// <summary>
        /// Recupera TODOS los monumentos del catálogo.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>Lista de snapshots. Vacía (no null) si el catálogo no tiene entradas.</returns>
        /// <remarks>
        /// El orden NO está garantizado por el contrato. Si el consumidor necesita
        /// orden específico (ej: ruta turística), debe ordenar después.
        /// </remarks>
        Task<IReadOnlyList<MonumentSnapshot>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Indica si el catálogo está disponible para queries.
        /// </summary>
        /// <remarks>
        /// Útil para esperar a que ScriptableObjects o Firestore terminen su
        /// warmup antes de mostrar la UI. Análogo a "S3 bucket exists check".
        /// </remarks>
        bool IsReady { get; }
    }
}