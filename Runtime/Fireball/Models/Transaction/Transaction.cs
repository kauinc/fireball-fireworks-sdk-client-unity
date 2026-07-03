using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fireball.Game.Client.Models
{
    public class Replay
    {
        public string Id;
        public string GameState;
        public DateTime? Timestamp;

        [UnityEngine.Scripting.Preserve]
        public Replay() { }

        public T ParseGameState<T>() where T : class
        {
            return JsonConvert.DeserializeObject<T>(GameState);
        }
    }

    public class ReplayList
    {
        public List<Transaction> Replays;

        [UnityEngine.Scripting.Preserve]
        public ReplayList() { }
    }

    public class Transaction
    {
        public string ReplayId;
        public DateTime? Start;
        public DateTime? End;
        public string Currency;
        public long Bet;
        public long Win;
        public long Jackpot;
        public List<Replay> GameStates;
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
