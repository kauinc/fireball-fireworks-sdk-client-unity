using Fireball.Game.Client;
using Fireball.Game.Client.Models;

namespace SlotsSample
{
    public class SpinRequest : BaseRequest
    {
        private const string NAME = "spin";

        public long Amount;

        public SpinRequest(long betAmount, FireballSession session, string customActionID = null) : base(NAME, session, customActionID)
        {
            Amount = betAmount;
        }
    }
}