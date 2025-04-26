using Microsoft.JSInterop;

namespace ClientBlazor.Services
{
    public class SessionService
    {
        private readonly ILocalStorageService _localStorage;
        private const string SESSION_ID_KEY = "user_session_id";

        public SessionService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public string GetOrCreateSessionId()
        {
            // Пытаемся получить ID из localStorage
            var sessionId = _localStorage.GetItem<string>(SESSION_ID_KEY);

            // Если ID нет — создаём новый и сохраняем
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                _localStorage.SetItem(SESSION_ID_KEY, sessionId);
            }

            return sessionId;
        }
    }
}
