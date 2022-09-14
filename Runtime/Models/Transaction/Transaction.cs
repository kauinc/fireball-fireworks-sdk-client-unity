using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fireball.Game.Client.Models
{
    public class TransactionGameStates
    {
        public string Id;
        public string GameState;
        public DateTime? Timestamp;

        [UnityEngine.Scripting.Preserve]
        public TransactionGameStates() { }

        public T ParseGameState<T>() where T : class
        {
            return JsonConvert.DeserializeObject<T>(GameState);
        }
    }

    public class Transaction
    {
        public string ReplayId;
        public DateTime? Start;
        public DateTime? End;
        public string Currency;
        public long Bet;
        public long Win;
        public List<TransactionGameStates> GameStates;
        public bool Pending;

        [UnityEngine.Scripting.Preserve]
        public Transaction() { }

        public string ToJson() =>
            JsonConvert.SerializeObject(this);
    }


    public class TransactionsList
    {
        public List<Transaction> Transactions;

        [UnityEngine.Scripting.Preserve]
        public TransactionsList() { }
    }
}
