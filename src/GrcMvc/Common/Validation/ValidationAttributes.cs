using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GrcMvc.Common.Validation
{
    /// <summary>
    /// Validates that a string is not null, empty, or whitespace
    /// </summary>
    public class RequiredNotEmptyAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required and cannot be empty.");
        }
    }

    /// <summary>
    /// Validates that a GUID is not empty
    /// </summary>
    public class RequiredGuidAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is Guid guid && guid != Guid.Empty)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be a valid GUID.");
        }
    }

    /// <summary>
    /// Validates email format
    /// </summary>
    public class EmailFormatAttribute : ValidationAttribute
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                return ValidationResult.Success; // Use [Required] for null checks
            }

            if (value is string email && EmailRegex.IsMatch(email))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be a valid email address.");
        }
    }

    /// <summary>
    /// Validates URL format
    /// </summary>
    public class UrlFormatAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                return ValidationResult.Success; // Use [Required] for null checks
            }

            if (value is string url && Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be a valid HTTP or HTTPS URL.");
        }
    }

    /// <summary>
    /// Validates that a date is not in the past
    /// </summary>
    public class NotPastDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Use [Required] for null checks
            }

            if (value is DateTime dateTime && dateTime.Date >= DateTime.UtcNow.Date)
            {
                return ValidationResult.Success;
            }

            if (value is DateTimeOffset dateTimeOffset && dateTimeOffset.Date >= DateTimeOffset.UtcNow.Date)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} cannot be in the past.");
        }
    }

    /// <summary>
    /// Validates that a date is not in the future
    /// </summary>
    public class NotFutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Use [Required] for null checks
            }

            if (value is DateTime dateTime && dateTime.Date <= DateTime.UtcNow.Date)
            {
                return ValidationResult.Success;
            }

            if (value is DateTimeOffset dateTimeOffset && dateTimeOffset.Date <= DateTimeOffset.UtcNow.Date)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} cannot be in the future.");
        }
    }

    /// <summary>
    /// Validates that a number is positive
    /// </summary>
    public class PositiveNumberAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success; // Use [Required] for null checks
            }

            if (value is int intValue && intValue > 0)
            {
                return ValidationResult.Success;
            }

            if (value is long longValue && longValue > 0)
            {
                return ValidationResult.Success;
            }

            if (value is decimal decimalValue && decimalValue > 0)
            {
                return ValidationResult.Success;
            }

            if (value is double doubleValue && doubleValue > 0)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be a positive number.");
        }
    }

    /// <summary>
    /// Validates that a string matches a specific pattern
    /// </summary>
    public class RegexPatternAttribute : ValidationAttribute
    {
        private readonly string _pattern;
        private readonly Regex _regex;

        public RegexPatternAttribute(string pattern)
        {
            _pattern = pattern;
            _regex = new Regex(pattern, RegexOptions.Compiled);
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                return ValidationResult.Success; // Use [Required] for null checks
            }

            if (value is string stringValue && _regex.IsMatch(stringValue))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} does not match the required pattern: {_pattern}");
        }
    }

    /// <summary>
    /// Validates that a collection is not empty
    /// </summary>
    public class NotEmptyCollectionAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} cannot be null.");
            }

            if (value is System.Collections.IEnumerable enumerable)
            {
                var enumerator = enumerable.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} cannot be empty.");
        }
    }

    /// <summary>
    /// Validates tenant ID format (32 character hex string)
    /// </summary>
    public class TenantIdFormatAttribute : ValidationAttribute
    {
        private static readonly Regex TenantIdRegex = new Regex(
            @"^[a-f0-9]{32}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                return ValidationResult.Success; // Use [Required] for null checks
            }

            if (value is string tenantId && TenantIdRegex.IsMatch(tenantId))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be a 32-character hexadecimal string.");
        }
    }
}
