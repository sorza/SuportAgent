using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json.Serialization;

namespace SuportAgent
{
    public sealed class SupportChatHistoryProvider : ChatHistoryProvider
    {
        private readonly ProviderSessionState<ProviderState> _sessionState;

        public SupportChatHistoryProvider()
        {

            // Inicializa o helper de estado por sessão.
            // O stateInitializer gera um ID único para cada sessão nova.
            this._sessionState = new ProviderSessionState<ProviderState>(
                stateInitializer: _ => new ProviderState { ConversationId = Guid.NewGuid().ToString() },
                stateKey: nameof(SupportChatHistoryProvider));
        }

        protected override ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(
            InvokingContext context,
            CancellationToken cancellationToken = default)
        {
            var state = this._sessionState.GetOrInitializeState(context.Session);

            // Carrega o histórico do repositório usando o ID de conversa da sessão.
            var messages = ConversationRepository.Load(state.ConversationId);
            return new(messages.AsEnumerable());
        }

        protected override ValueTask StoreChatHistoryAsync(
            InvokedContext context,
            CancellationToken cancellationToken = default)
        {
            var state = this._sessionState.GetOrInitializeState(context.Session);

            // Carrega o histórico atual e acrescenta as mensagens novas desta execução.
            var existing = ConversationRepository.Load(state.ConversationId);
            var allNewMessages = context.RequestMessages.Concat(context.ResponseMessages ?? []);
            existing.AddRange(allNewMessages);

            // Persiste a lista atualizada de volta ao repositório.
            ConversationRepository.Save(state.ConversationId, existing);

            // Salva o estado da sessão para que o ConversationId seja preservado entre execuções.
            this._sessionState.SaveState(context.Session, state);
            return default;
        }

        // Estado armazenado por sessão no AgentSession.
        // Nunca armazene isso como campo da classe — o provedor é compartilhado entre sessões.
        public sealed class ProviderState
        {
            [JsonPropertyName("conversationId")]
            public string ConversationId { get; set; } = string.Empty;
        }
    }
}
