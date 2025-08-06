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

            public CurrencyData(string code, string symbolFormat, int decimals = 2, double multiplier = 0.01d, string desc = null)
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
            { "GHC", new CurrencyData("GHC", "SC {0}", desc: "Markor Sweepstakes Coin") },
            { "WOC", new CurrencyData("WOC", "SC {0}", desc: "Markor WOC Sweepstakes Coin") },
            { "SSC", new CurrencyData("SSC", "SC {0}", desc: "Markor Sweepstakes Coin") },
            { "SS1", new CurrencyData("SS1", "SC {0}", desc: "Hub88 Sweepstakes Coin") },
            { "SC.", new CurrencyData("SC.", "SC {0}", desc: "Sweepstakes Coin via Relax P2P") },
            { "XSC", new CurrencyData("XSC", "SC {0}", desc: "Generic Sweepstakes Coin") },
            { "GCC", new CurrencyData("GCC", "GC {0}", desc: "Markor Gold Coin") },
            { "XGC", new CurrencyData("XGC", "GC {0}", desc: "Generic Gold Coin") },
            { "GC.", new CurrencyData("GC.", "GC {0}", desc: "Gold Coin via Relax P2P") },
            { "GLD", new CurrencyData("GLD", "GC {0}", desc: "Hub88 Gold Coin") },
            { "SFC", new CurrencyData("SFC", "FC {0}", desc: "Markor Sweepstakes Coin (Casimba)") },
            { "FCC", new CurrencyData("FCC", "FC {0}", desc: "(Fortune) Sweepstakes Coin") },
            { "FC.", new CurrencyData("FC.", "FC {0}", desc: "Relax Fortune Coin") },
            { "BT.", new CurrencyData("BT.", "BT {0}", desc: "Relax Bit") },
            { "BK.", new CurrencyData("BK.", "BK {0}", desc: "Relax Buck") },
            { "VBC", new CurrencyData("VBC", "WOW {0}", desc: "Markor VBC Fun Coin") },
        };

        private static readonly Dictionary<string, CurrencyData> _cryptoCurrencies = new Dictionary<string, CurrencyData>
        {
            { "MBTC", new CurrencyData("MBTC", "mBTC {0}", 5, 0.00001d, "Milli Bitcoin") },
            { "BTC", new CurrencyData("BTC", "BTC {0}", 8, 0.00000001d, "Bitcoin") },
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
        
        public static bool IsCryptoCurrency(string currencyCode)
        {
            return _cryptoCurrencies.Keys.Contains(currencyCode.ToUpper());
        }

        public static string FormatMoney(long money, string customCurrency = null, CultureInfo customCulture = null)
        {
            var session = GetCurrentSession();
            var currency = customCurrency ?? session?.Currency;
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
                    var virtualMultiplier = session?.Multiplier != null ? 1.0d / session.Multiplier.Value : data.DecimalsMultiplier;
                    return string.Format(data.SymbolFormat, (money * virtualMultiplier).ToString($"N{data.Decimals}", culture));
                }
                return (money * 0.01d).ToString("N2", culture);
            }
            
            // format crypto currencies
            if (currency != null && _cryptoCurrencies.TryGetValue(currency.ToUpper(), out data))
            {
                if (data != null)
                {
                    var cryptoMultiplier = session?.Multiplier != null ? 1.0d / session.Multiplier.Value : data.DecimalsMultiplier;
                    return string.Format(data.SymbolFormat, (money * cryptoMultiplier).ToString($"N{data.Decimals}", culture));
                }
            }
            
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
