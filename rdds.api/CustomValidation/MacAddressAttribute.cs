using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace rdds.api.CustomValidation
{

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MacAddressAttribute: ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return true;  // Consider empty/null valid or adjust as required.

            var macAddress = value.ToString();

            if (string.IsNullOrEmpty(macAddress)) return true;

            var regex = new Regex("^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$");
            
            return regex.IsMatch(macAddress);
        }
    }
}