using System;

namespace Fireball.Game.Client.Models
{
    public class AuthorizingRequestParams
    {
        public AuthRequest Request;
        public Action<AuthResponse> OnSuccess = null;
        public Action<ErrorResponse> OnError = null;
        public float Timeout = 0;
        public int Attempts = 1;

        public AuthorizingRequestParams(AuthRequest authRequest, Action<AuthResponse> onSuccess = null, Action<ErrorResponse> onError = null, float timeout = 0, int attempts = 1)
        {
            Request = authRequest;
            OnSuccess = onSuccess;
            OnError = onError;
            Timeout = timeout;
            Attempts = attempts;
        }
    }
}
