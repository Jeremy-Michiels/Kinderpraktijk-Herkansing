using System;
using System.ComponentModel.DataAnnotations;

namespace src.Helpers
{
    public class SixteenAndOlderAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            value = (DateTime)value;
            if (DateTime.Now.AddYears(-16).CompareTo(value) >= 0)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Je moet ouder dan 16 jaar zijn om aan te kunnen melden");
            }
        }
    }
}
