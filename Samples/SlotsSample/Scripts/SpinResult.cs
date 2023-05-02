using System.Collections.Generic;
using Fireball.Game.Client.Models;

namespace SlotsSample
{
    public class SpinResult : BaseResponse
    {
        private const string NAME = "spin-result";

        public string GameType;
        public Dictionary<int, int> Symbols;
        public long WinAmount;
        public long Balance;
        public bool IsWon;

        public SpinResult()
        {
            Name = NAME;
        }
    }
}
