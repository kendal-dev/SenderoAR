// File: Assets/_SenderoAR/Models/ChatTurn.cs
// Sprint 0 · Día 5 · Commit #23

namespace SenderoAR.Models
{
    /// <summary>
    /// Identifica quién emitió un turno en la conversación.
    /// </summary>
    public enum ChatRole
    {
        /// <summary>El turista preguntando.</summary>
        User = 0,
        /// <summary>El "Guía Historiador" (Gemini).</summary>
        Assistant = 1
    }

    /// <summary>
    /// Un turno individual de la conversación. Inmutable.
    /// </summary>
    /// <remarks>
    /// POR QUÉ NO INCLUIR TIMESTAMP: el dominio (chat histórico-cultural)
    /// no requiere ordering temporal explícito. La lista de turnos es
    /// ordenada implícitamente por inserción. Agregar timestamp sería
    /// over-engineering para el MVP.
    /// </remarks>
    public sealed class ChatTurn
    {
        public ChatRole Role { get; }
        public string Content { get; }

        public ChatTurn(ChatRole role, string content)
        {
            Role = role;
            Content = content ?? string.Empty;
        }
    }
}