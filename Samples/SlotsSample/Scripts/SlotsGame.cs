using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Fireball.Game.Client;
using Fireball.Game.Client.Models;
using Fireball.Game.Client.Modules;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SlotsSample
{
    public enum States
    {
        None = 0,
        Initialazing = 1,
        Initialized = 2,
        Authorizing = 3,
        Authorized = 4,
        Spinning = 5,
        Stoping = 6,
    }

    public class SlotsGame : MonoBehaviour
    {
        public States State => _state;
        public SlotsUI ui;

        [Header("Configs")]
        public FireballSettings CustomSettings;
        public LogLevels LogLevel = LogLevels.Information;

        [Header("Game Elements")]
        [SerializeField] List<Sprite> _symbols;
        [SerializeField] List<SpriteRenderer> _slots;

        private IFireball _fireball;
        private long _betAmount = 100;
        private long _balance = 100000;
        private string _currency = "USD";
        [SerializeField] private States _state = States.None;
        private List<int> _resultSymbols = null;

        public void Start()
        {
            _fireball = Fireball.Game.Client.Fireball.Instance;

            //fireball.GetReplaysList("Q7kEgjE1KV",
            //    (list) =>
            //    {
            //        foreach (var item in list)
            //        {
            //            Debug.Log("REPLAY: " + item.ToJson());
            //        }
            //    },
            //    (error) =>
            //    {
            //        Debug.LogError("REPLAY: " + error);
            //    });
        }

        public void Init()
        {
            _state = States.Initialazing;
            FireballConfig.LogLevel = LogLevel;
            ui.Initializing();
            _fireball.Init(CustomSettings, (session) =>
            {
                _state = States.Initialized;
                ui.Initialized(true);
            },
            (error) =>
            {
                _state = States.None;
                ui.Initialized(false);
                ui.ShowError(error);
            });
        }

        public void Auth()
        {
            _state = States.Authorizing;
            ui.Authorizing();
            _fireball.Authorize(new AuthRequest(_fireball.CurrentSession), (response) =>
            {
                _state = States.Authorized;
                OnAuth(response.Currency, response.Balance, _betAmount);
            },
            (error) =>
            {
                _state = States.Initialized;
                ui.Authorized(false);
                ui.ShowError(error.Reason);
            });
        }

        public void Spin()
        {
            _state = States.Spinning;
            var spinRequest = new SpinRequest(_betAmount, _fireball.CurrentSession);
            StartCoroutine(nameof(StartSpinningAnimation));
            OnSpinningStart(spinRequest.Currency, spinRequest.Amount);
            _fireball.SendRequest<SpinRequest, SpinResult>(spinRequest, (response) =>
            {
                _resultSymbols = response.Symbols.Values.ToList();
                _state = States.Stoping;
                OnSpinningStop(response.Currency, response.WinAmount, response.Balance);
            },
            (error) =>
            {
                StopCoroutine(nameof(StartSpinningAnimation));
                OnSpinningStop(_currency, 0, _balance);
                ui.Spinning(false);
                ui.ShowError(error.Reason);
            });
        }

        public void Reset()
        {
            var resetRequest = new BaseRequest("reset", _fireball.CurrentSession);
            _fireball.SendRequest<BaseRequest, BaseResponse>(resetRequest, (response) =>
            {
                ui.ShowSuccess($"Game State Reset!");
            },
            (error) =>
            {
                ui.ShowError(error.Reason);
            });
        }

        public void Test()
        {
            var testRequest = new BaseRequest("test", _fireball.CurrentSession);
            var testAuth = new AuthRequest(_fireball.CurrentSession);
            testAuth.Name = "test-auth";

            _fireball.SendRequest<AuthRequest, BaseResponse>(testAuth, (response) =>
            {
                var time = Fireball.Game.Client.Tools.FireballTools.GetNowTimestampMilliSeconds() - response.MessageTimestamp;
                ui.ShowSuccess($"Test Message Success! Time: {time} ms");
            },
            (error) =>
            {
                ui.ShowError(error.Reason);
            });
        }


        private void OnAuth(string currency, long balance, long bet)
        {
            _state = States.Authorized;
            _betAmount = bet;
            _balance = balance;
            _currency = currency;

            ui.Authorized(true);
            ui.UpdateBalance(currency, balance);
            ui.UpdateBet(currency, bet);
            ui.UpdateWin(currency, 0);
        }

        private void OnSpinningStart(string currency, long bet)
        {
            _balance -= bet;

            ui.Spinning(true);
            ui.UpdateBalance(currency, _balance);
            ui.UpdateBet(currency, bet);
            ui.UpdateWin(currency, 0);
        }

        private void OnSpinningStop(string currency, long win, long balance)
        {
            _balance = balance;

            ui.Spinning(false);
            ui.UpdateBalance(currency, _balance);
            ui.UpdateBet(currency, _betAmount);
            ui.UpdateWin(currency, win);
        }


        private Sprite GetRandomSymbol()
        {
            if (_symbols != null && _symbols.Count > 0)
            {
                return _symbols[UnityEngine.Random.Range(0, _symbols.Count)];
            }
            return null;
        }

        private IEnumerator StartSpinningAnimation()
        {
            var delay = new WaitForSeconds(0.1f);
            while (_state == States.Spinning)
            {
                foreach (var slot in _slots)
                {
                    slot.sprite = GetRandomSymbol();
                    yield return delay;
                }
            }
            yield return StopSpinningAnimation(_resultSymbols);
        }

        private IEnumerator StopSpinningAnimation(List<int> finalSymbols)
        {
            _state = States.Stoping;
            var delay = new WaitForSeconds(0.1f);
            foreach (var slot in _slots)
            {
                slot.sprite = GetRandomSymbol();
                yield return delay;
            }
            _state = States.Authorized;
        }
    }
}

