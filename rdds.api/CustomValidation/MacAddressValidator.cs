using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace rdds.api.CustomValidation
{
    public static class MacAddressValidator
    {
        public static bool IsMacAddressValid(string? mac)
        {
            if (string.IsNullOrEmpty(mac)) return false;

            var regex = new Regex("^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$");
            
            return regex.IsMatch(mac);
        }
    }
}