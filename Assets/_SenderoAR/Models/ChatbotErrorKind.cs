// File: Assets/_SenderoAR/Models/ChatbotErrorKind.cs
// Sprint 0 · Día 5 · Commit #23

namespace SenderoAR.Models
{
    /// <summary>
    /// Categorías de error normalizadas del chatbot. Mapean los códigos HTTP
    /// de Vertex AI a algo que la UI puede traducir.
    /// </summary>
    /// <remarks>
    /// MAPEO desde HTTP (ver 02_AI_Architecture.md sección 6):
    ///   429 RESOURCE_EXHAUSTED → QuotaExceeded
    ///   503 SERVICE_UNAVAILABLE → BackendUnavailable
    ///   400 INVALID_ARGUMENT → InvalidRequest
    ///   OperationCanceledException → Timeout (o Cancelled si fue user-driven)
    ///   No internet → NetworkUnreachable
    ///   Cualquier otro → Unknown
    ///
    /// La UI debe mostrar un mensaje amable distinto por cada caso. NUNCA
    /// mostrar el HTTP code crudo al usuario final.
    /// </remarks>
    public enum ChatbotErrorKind
    {
        Unknown = 0,
        NetworkUnreachable = 1,
        QuotaExceeded = 2,
        BackendUnavailable = 3,
        InvalidRequest = 4,
        Timeout = 5,
        Cancelled = 6,
        AppCheckRejected = 7
    }
}