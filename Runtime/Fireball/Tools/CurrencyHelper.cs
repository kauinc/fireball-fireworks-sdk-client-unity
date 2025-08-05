using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Fireball.Game.Client.Tools
{
    public static class CurrencyHelper
    {
        private class CurrencyData
        {
            public readonly string Code;
            public readonly string SymbolFormat;
            public readonly int Decimals;
            public readonly double DecimalsMultiplier;
            public readonly string Desc;

            public CurrencyData(string code, string symbolFormat, int decimals = 2, string desc = null)
            {
                Code = code;
                SymbolFormat = symbolFormat;
                Decimals = decimals;
                DecimalsMultiplier = System.Math.Pow(0.1d, decimals);
                Desc = desc;
            }
        }

        private static FireballSession _session = null;
        private static CultureInfo _culture = null;

        private static List<string> _currencies = new List<string>();
        private static readonly Dictionary<string, CurrencyData> _virtualCurrencies = new Dictionary<string, CurrencyData>
        {
            { "GHC", new CurrencyData("GHC", "SC {0}", 2, "Markor Sweepstakes Coin") },
            { "WOC", new CurrencyData("WOC", "SC {0}", 2, "Markor WOC Sweepstakes Coin") },
            { "SSC", new CurrencyData("SSC", "SC {0}", 2, "Markor Sweepstakes Coin") },
            { "SS1", new CurrencyData("SS1", "SC {0}", 2, "Hub88 Sweepstakes Coin") },
            { "SC.", new CurrencyData("SC.", "SC {0}", 2, "Sweepstakes Coin via Relax P2P") },
            { "XSC", new CurrencyData("XSC", "SC {0}", 2, "Generic Sweepstakes Coin") },
            { "GCC", new CurrencyData("GCC", "GC {0}", 2, "Markor Gold Coin") },
            { "XGC", new CurrencyData("XGC", "GC {0}", 2, "Generic Gold Coin") },
            { "GC.", new CurrencyData("GC.", "GC {0}", 2, "Gold Coin via Relax P2P") },
            { "GLD", new CurrencyData("GLD", "GC {0}", 2, "Hub88 Gold Coin") },
            { "SFC", new CurrencyData("SFC", "FC {0}", 2, "Markor Sweepstakes Coin (Casimba)") },
            { "FCC", new CurrencyData("FCC", "FC {0}", 2, "(Fortune) Sweepstakes Coin") },
            { "FC.", new CurrencyData("FC.", "FC {0}", 2, "Relax Fortune Coin") },
            { "BT.", new CurrencyData("BT.", "BT {0}", 2, "Relax Bit") },
            { "BK.", new CurrencyData("BK.", "BK {0}", 2, "Relax Buck") },
            { "VBC", new CurrencyData("VBC", "WOW {0}", 2, "Markor VBC Fun Coin") },
        };

        public static void SetSession(FireballSession session)
        {
            _session = session;
            _culture = GetCulture(session);
        }

        public static bool IsFiatCurrency(string currencyCode)
        {
            var currencies = GetFiatCurrencies();
            return currencies != null && currencies.Contains(currencyCode.ToUpper());
        }
        
        public static bool IsVirtualCurrency(string currencyCode)
        {
            return _virtualCurrencies.Keys.Contains(currencyCode.ToUpper());
        }

        public static string FormatMoney(long money, string customCurrency = null, CultureInfo customCulture = null)
        {
            var currency = customCurrency ?? GetCurrentSession()?.Currency;
            var culture = customCulture ?? GetCurrentCulture();
            
            // format fiat currencies
            if (currency != null && culture != null && IsFiatCurrency(currency))
            {
                return money.ToString("C", culture);
            }
            
            // format virtual currencies
            if (currency != null && _virtualCurrencies.TryGetValue(currency.ToUpper(), out var data))
            {
                if (data != null)
                {
                    return string.Format(data.SymbolFormat, (money * data.DecimalsMultiplier).ToString($"N{data.Decimals}", culture));
                }
                return (money * 0.01d).ToString("N2", culture);
            }
            
            var session = GetCurrentSession();
            var decimalsMultiplier = session?.Multiplier != null ? 1.0d / session.Multiplier.Value : 0.01d;
            var decimals = session?.Multiplier != null ? CalculateDecimals(session.Multiplier.Value) : 2;
            return string.Format(currency + " {0}", (money * decimalsMultiplier).ToString($"N{decimals}", culture));
        }

        private static FireballSession GetCurrentSession()
        {
            if (_session == null)
            {
                if (Fireball.Instance != null)
                {
                    _session = Fireball.Instance.CurrentSession;
                }
            }

            return _session;
        }

        private static CultureInfo GetCurrentCulture()
        {
            if (_culture == null)
            {
                _culture = GetCulture(GetCurrentSession());
            }

            return _culture;
        }

        private static List<string> GetFiatCurrencies()
        {
            if (_currencies == null || _currencies.Count == 0)
            {
                _currencies = GetAllCurrencies();
            }
            return _currencies;
        }
        
        private static List<string> GetAllCurrencies()
        {
            var currencyCodes = new List<string>();
            foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                try
                {
                    RegionInfo region = new RegionInfo(culture.Name);
                    if (currencyCodes.Contains(region.ISOCurrencySymbol))
                    {
                        continue;
                    }
                    
                    currencyCodes.Add(region.ISOCurrencySymbol);
                }
                catch
                {
                    // Some cultures may not have region info
                    continue;
                }
            }
            return currencyCodes;
        }

        private static CultureInfo GetCulture(FireballSession session)
        {
            if (session == null)
            {
                return CultureInfo.CurrentCulture;
            }
            
            // find culture by currency for fiat currencies
            if (IsFiatCurrency(session.Currency))
            {
                return CultureHelper.CultureFromCurrency(session.Currency, session.Country);
            }

            // find culture by country for coins, virtual, crypto currencies
            return CultureHelper.CultureFromCountry(session.Country);
        }
        
        private static int CalculateDecimals(long multiplier)
        {
            int decimals = 0;
            while (multiplier >= 10)
            {
                decimals++;
                multiplier /= 10;
            }
            return decimals;
        }
    }
}
