using System.Collections.Generic;
using Fireball.Game.Client.Models;

namespace SlotSample
{
    public class SpinResult : BaseResponse
    {
        private const string NAME = "spin-result";

        public string GameType;
        public List<int> Symbols;
        public long WinAmount;
        public long Balance;
        public bool IsWon;

        public SpinResult()
        {
            Name = NAME;
        }
    }
}
