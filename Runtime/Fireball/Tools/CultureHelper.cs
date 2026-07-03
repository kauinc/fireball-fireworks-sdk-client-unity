using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Fireball.Game.Client.Tools
{
    public static class CultureHelper
    {
        public static CultureInfo CultureFromCountry(string countryISO)
        {
            if (countryISO.Length == 2)
            {
                //Debug.Log($"[Culture] Search Country: {countryISO}...");
                return CultureInfo.GetCultures(CultureTypes.AllCultures)?
                    .Where(c => c.Name.EndsWith(countryISO.ToUpper()))?
                    .FirstOrDefault();
            }

            Debug.LogWarning($"[Culture] Wrong country ISO code: {countryISO}! Required two letters code!");
            return null;
        }

        public static CultureInfo CultureFromCurrency(string currencyISO, string countryISO = null)
        {
            //Debug.Log($"[Culture] Search Currency: {currencyISO}...");
            var regions = RegionsFromCurrency(currencyISO);

            if (regions != null && regions.Count > 0)
            {
                RegionInfo region = null;
                if (string.IsNullOrEmpty(countryISO))
                {
                    region = regions.First();
                }
                else
                {
                    //Debug.Log($"[Culture] Search Country: {countryISO}...");
                    var r = regions.Where(r => r.TwoLetterISORegionName.Equals(countryISO.ToUpper()));
                    if (r != null && r.Count() > 0)
                    {
                        region = r.First();
                    }

                    if (region == null)
                    {
                        //Debug.Log($"[Culture] No country: {countryISO} with currency: {currencyISO}");
                        region = regions.First();
                    }
                }

                //Debug.Log($"[Culture] Found Region: {_region.TwoLetterISORegionName}");
                var culture = CultureFromCountry(region.TwoLetterISORegionName);
                if (culture != null)
                {
                    //Debug.Log($"[Culture] Found Culture: {_culture.Name}");
                    return culture;
                }

            }

            Debug.LogWarning($"[Culture] Can't find culture from: {currencyISO}! Country code {countryISO}");
            return null;
        }

        public static List<RegionInfo> RegionsFromCurrency(string currencyISO)
        {
            if (currencyISO.Length == 3)
            {
                return CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .Where(c => !c.Equals(CultureInfo.InvariantCulture)) // Remove the invariant culture as a region cannot be created from it.
                    .Where(c => !c.IsNeutralCulture) // Remove neutral cultures as a region cannot be created from them.
                    .Select(c => new RegionInfo(c.LCID))
                    .Where(r => r.ISOCurrencySymbol.Equals(currencyISO.ToUpper()))?
                    .ToList();
            }

            Debug.LogWarning($"[Culture] Wrong currency ISO code: {currencyISO}! Requireded three letters code!");
            return null;
        }

        public static RegionInfo GetRegionInfo(this CultureInfo cultureInfo)
        {
            return new RegionInfo(cultureInfo.LCID);
        }

        public static List<RegionInfo> GetAllRegions()
        {
            return CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(c => !c.Equals(CultureInfo.InvariantCulture)) // Remove the invariant culture as a region cannot be created from it.
                .Where(c => !c.IsNeutralCulture) // Remove neutral cultures as a region cannot be created from them.
                .Select(c => new RegionInfo(c.LCID))
                .ToList();
        }
    }
}