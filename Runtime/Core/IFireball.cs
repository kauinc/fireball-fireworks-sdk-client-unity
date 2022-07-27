using System;
using System.Collections.Generic;
using Fireball.Game.Client.Models;
using Fireball.Game.Client.Modules;

namespace Fireball.Game.Client
{
    public interface IFireball
    {
        FireballSession CurrentSession { get; }
        string LastActionID { get; }

        bool IsInit { get; }
        bool IsAuth { get; }
        bool IsDemo { get; }

        Action<JackpotUpdateMessage> OnJackpotUpdate { get; set; }

        void Init(Action<FireballSession> onSuccess = null, Action<string> onError = null);
        void Init(string customUrl, Action<FireballSession> onSuccess = null, Action<string> onError = null);
        void Init(FireballSettings customSettings, Action<FireballSession> onSuccess = null, Action<string> onError = null);

        void Authorize(AuthRequest authRequest, Action<AuthResponse> onSuccess = null, Action<ErrorResponse> onError = null);
        void Authorize<TRequest, TResponse>(TRequest authRequest, Action<TResponse> onSuccess = null, Action<ErrorResponse> onError = null)
            where TRequest : AuthRequest
            where TResponse : AuthResponse;

        void SendRequest<TRequest, TResponse>(TRequest request, Action<TResponse> onSuccess, Action<ErrorResponse> onError = null, float timeout = 0, int attempts = 1)
            where TRequest : BaseRequest
            where TResponse : BaseResponse;
    }
}