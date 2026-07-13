
using Microsoft.Extensions.AI;
using System.Collections.Concurrent;

namespace SuportAgent
{
    // Simula uma camada de persistência com um dicionário thread-safe.
    // Em produção, substitua por uma implementação com banco de dados ou Redis.

    public static class ConversationRepository
    {
        private static readonly ConcurrentDictionary<string, List<ChatMessage>> _store = new();

        public static List<ChatMessage> Load(string conversationId)
        {
            // Retorna a lista existente ou uma nova lista vazia para o ID informado.
            return _store.GetOrAdd(conversationId, _ => []);
        }

        public static void Save(string conversationId, List<ChatMessage> messages)
        {
            _store[conversationId] = messages;
        }
    }
}
