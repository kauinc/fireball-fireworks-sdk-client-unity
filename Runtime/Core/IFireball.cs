using System;
using System.Collections.Generic;
using Fireball.Game.Client.Models;
using Fireball.Game.Client.Modules;

namespace Fireball.Game.Client
{
    public interface IFireball
    {
        FireballMultiplayer Multiplayer { get; }
        FireballSession CurrentSession { get; }
        string LastActionID { get; }

        bool IsConnected { get; }
        bool IsInit { get; }
        bool IsAuth { get; }
        bool IsDemo { get; }

        Action<JackpotUpdateMessage> OnJackpotUpdate { get; set; }
        Action<string> OnServerConnectionError { get; set; }

        void Init(Action<FireballSession> onSuccess = null, Action<string> onError = null);
        void Init(string customUrl, Action<FireballSession> onSuccess = null, Action<string> onError = null);
        void Init(FireballSettings customSettings, Action<FireballSession> onSuccess = null, Action<string> onError = null);

        void Authorize(AuthRequest authRequest, Action<AuthResponse> onSuccess = null, Action<ErrorResponse> onError = null, float timeout = 0, int attempts = 1);
        void Authorize<TRequest, TResponse>(TRequest authRequest, Action<TResponse> onSuccess = null, Action<ErrorResponse> onError = null, float timeout = 0, int attempts = 1)
            where TRequest : AuthRequest
            where TResponse : AuthResponse;

        void DemoAuthorize(string currency = FireballConfig.DEFAULT_CURRENCY, long balance = FireballConfig.DEMO_BALANCE, Action<AuthResponse> onSuccess = null, Action<ErrorResponse> onError = null);

        void SendRequest<TRequest, TResponse>(TRequest request, Action<TResponse> onSuccess, Action<ErrorResponse> onError = null, float timeout = 0, int attempts = 1)
            where TRequest : BaseRequest
            where TResponse : BaseResponse;

        void GetBetTiers(string currency, Action<List<TierData>> onSuccess, Action<string> onError = null);
        void GetTransactionsList(Action<TransactionsList> onSuccess, Action<string> onError = null, int startIndex = 0, bool includeGameStates = true);
        void GetReplaysList(string shortReplayId, Action<List<Transaction>> onSuccess, Action<string> onError = null);

        void InvokeInMainThread(Action action, float delay = 0);
    }
}