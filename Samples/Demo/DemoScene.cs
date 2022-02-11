using KAU.FireballSDK.Demo.Requests;
using KAU.FireballSDK;
using UnityEngine;

namespace KAU.FireballSDK.Demo
{
    public class DemoScene : MonoBehaviour
    {
        private IFireball fireball;

        public void Start()
        {
            string url = GetCleosDiaryURLParams();

            fireball = Fireball.Instance;
            fireball.Init(url,
                session =>
                {
                    fireball.SendRequest<AuthRequest, AuthResponse>(new AuthRequest(session.Token, session.Mode, session),
                        response =>
                        {
                            if (response != null)
                            {
                                fireball.CurrentSession.GameSession = response.gameSession;
                                fireball.CurrentSession.PlayerId = response.playerId;
                            }
                        },
                        error =>
                        {
                            Debug.LogError(error.errorMessage.Message);
                        });
                },
                errorString =>
                {
                    Debug.LogError(errorString);
                },
                Modules.MessengerType.SignalR);
        }

        private string GetWilliamsQuestURLParams()
        {
            GameMode mode = GameMode.money;
            Environments environment = Environments.development;
            string operatorId = "72c43a89-585e-4cf9-8fab-dcc1740eec40";
            string gameId = "b5b6562c-f4a6-4838-86d7-a7bede90ed6c";
            string playerId = "3696f882-7a05-40c7-be2c-49bcf6168fcc";
            string token = "be258078-0d2d-4b49-9b5d-97a087a4c8fd";
            string wsToken = "3112000f-5283-41fa-a692-b8a17784c62c";

            return $"?gameId={gameId}" +
                $"&operatorId={operatorId}" +
                $"&playerId={playerId}" +
                $"&environment={environment.ToString()}" +
                $"&token={token}" +
                $"&mode={mode.ToString().ToLower()}" +
                "&router=https%3A%2F%2Fcloud.fireballserver.com%2Frouter" +
                "&messages=wss%3A%2F%2Fcloud.fireballserver.com%2Fmessages" + // default WebSocket app-messages
                $"&wsToken={wsToken}" +
                "&platform=web" +
                "&age=36" +
                "&gender=male" +
                "&currency=DKK" +
                "&language=en" +
                "&country=DK" +
                "&siteId=1" +
                "&home=&router=https%3A%2F%2Fcloud.fireballserver.com%2Frouter" +
                "";
        }

        private string GetCleosDiaryURLParams()
        {
            GameMode mode = GameMode.money;
            Environments environment = Environments.development;
            string operatorId = "72c43a89-585e-4cf9-8fab-dcc1740eec40";
            string gameId = "c61b4556-5816-44bc-845b-39083f19956b";
            string playerId = "566dd8dd-836c-40c1-afeb-dc930ddfac4f";
            string token = "aa2108de-9cfc-484d-a58f-a77e6b4989d6";
            string wsToken = "c55a072f-b941-4ce5-8866-824f147e307e";

            return $"?gameId={gameId}" +
                $"&operatorId={operatorId}" +
                $"&playerId={playerId}" +
                $"&environment={environment.ToString()}" +
                $"&token={token}" +
                $"&mode={mode.ToString().ToLower()}" +
                "&router=https%3A%2F%2Fcloud.fireballserver.com%2Frouter" +
                "&messages=https%3A%2F%2Fcloud.fireballserver.com%2Fmessages-net%2Fmessages" + // .NET-messages with SignalR
                $"&wsToken={wsToken}" +
                "&platform=web" +
                "&age=36" +
                "&gender=male" +
                "&currency=DKK" +
                "&language=en" +
                "&country=DK" +
                "&siteId=1" +
                "&home=&router=https%3A%2F%2Fcloud.fireballserver.com%2Frouter" +
                "";
        }
    }
}
