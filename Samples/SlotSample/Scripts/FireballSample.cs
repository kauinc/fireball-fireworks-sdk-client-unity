using System;
using System.Collections.Generic;
using Fireball.Game.Client;
using Fireball.Game.Client.Models;
using Fireball.Game.Client.Modules;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SlotSample
{
    public class FireballSample : MonoBehaviour
    {
        public SampleUI ui;
        public IFireball fireball;

        [Header("Configs")]
        public FireballSettings CustomSettings;
        public LogLevels LogLevel = LogLevels.Information;

        private long _betAmount = 100;
        private long _balance = 100000;
        private string _currency = "USD";

        public void Start()
        {
            fireball = Fireball.Game.Client.Fireball.Instance;
        }

        public void Init()
        {
            FireballConfig.LogLevel = LogLevel;
            ui.Initializing();
            fireball.Init(CustomSettings, (session) =>
            {
                ui.Initialized(true);
            },
            (error) =>
            {
                ui.Initialized(false);
                ui.ShowError(error);
            });
        }

        public void Auth()
        {
            ui.Authorizing();
            fireball.Authorize(new AuthRequest(fireball.CurrentSession), (response) =>
            {
                OnAuth(response.Currency, response.Balance, _betAmount);
            },
            (error) =>
            {
                ui.Authorized(false);
                ui.ShowError(error.Reason);
            });
        }

        public void Spin()
        {
            var spinRequest = new SpinRequest(_betAmount, fireball.CurrentSession);
            OnSpinningStart(spinRequest.Currency, spinRequest.Amount);
            fireball.SendRequest<SpinRequest, SpinResult>(spinRequest, (response) =>
            {
                OnSpinningStop(response.Currency, response.WinAmount, response.Balance);
            },
            (error) =>
            {
                OnSpinningStop(_currency, 0, _balance);
                ui.Spinning(false);
                ui.ShowError(error.Reason);
            });
        }


        private void OnAuth(string currency, long balance, long bet)
        {
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
    }
}

