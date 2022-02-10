using KAU.FireballSDK.Demo.Requests;
using KAU.FireballSDK;
using UnityEngine;

namespace KAU.FireballSDK.Demo
{
    public class DemoScene : MonoBehaviour
    {
        private IFireball fireball;

        void Start()
        {
            fireball = Fireball.Instance;
            fireball.Init(session =>
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
                });
        }
    }
}
