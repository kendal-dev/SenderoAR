// File: Assets/_SenderoAR/Core/Contracts/IChatbotClient.cs
// Sprint 0 · Día 5 · Commit #23

using System;
using System.Threading;
using System.Threading.Tasks;
using SenderoAR.Models;

namespace SenderoAR.Core.Contracts
{
    /// <summary>
    /// Contrato del cliente conversacional con el LLM de patrimonio cultural.
    /// </summary>
    /// <remarks>
    /// <para>BACKEND DETRÁS: Firebase AI Logic → Vertex AI → Gemini 3.1 Flash-Lite,
    /// endpoint southamerica-east1. El modelo concreto se resuelve vía Remote Config
    /// (key "active_gemini_model") en la implementación, NUNCA hardcoded.</para>
    ///
    /// <para>SEGURIDAD: la implementación usa App Check (Replay Protection enabled).
    /// El cliente Unity JAMÁS contiene API keys.</para>
    ///
    /// <para>THREADING CONTRACT: todos los Task se completan en main thread Unity.
    /// Los handlers de OnError corren en main thread.</para>
    ///
    /// <para>SESIÓN: la conversación es stateful en el servidor. Llamar a
    /// ResetSessionAsync limpia el contexto multi-turno.</para>
    /// </remarks>
    public interface IChatbotClient : IDisposable
    {
        /// <summary>True cuando InitializeAsync completó exitosamente.</summary>
        bool IsReady { get; }

        /// <summary>
        /// Disparado cuando ocurre un error categorizado. NO se dispara para errores
        /// de cancelación de usuario (esos vienen como OperationCanceledException del Task).
        /// </summary>
        event Action<ChatbotErrorKind> ErrorOccurred;

        /// <summary>
        /// Inicializa Firebase, valida App Check, descarga Remote Config.
        /// Debe llamarse antes de cualquier QueryAsync.
        /// </summary>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía la pregunta del usuario al LLM y devuelve la respuesta del asistente.
        /// </summary>
        /// <param name="userQuery">Texto crudo del usuario. La implementación lo sanitiza.</param>
        /// <param name="language">Idioma de la respuesta esperada.</param>
        /// <param name="cancellationToken">Cancelación. La UI debe cancelar si el usuario abandona la pantalla.</param>
        /// <param name="timeoutSeconds">Timeout duro. Default 15s alineado con Vertex AI SLA.</param>
        /// <returns>Texto de respuesta del asistente. Cadena vacía si hubo error categorizado (ver ErrorOccurred).</returns>
        Task<string> QueryAsync(
            string userQuery,
            LanguageCode language,
            CancellationToken cancellationToken = default,
            int timeoutSeconds = 15);

        /// <summary>
        /// Limpia el contexto multi-turno en el servidor. Tras esto, la próxima
        /// QueryAsync arranca conversación fresca.
        /// </summary>
        Task ResetSessionAsync(CancellationToken cancellationToken = default);
    }
}