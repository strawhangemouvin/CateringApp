using System.Net;
using System.Text.RegularExpressions;

namespace CateringApp.Helpers
{
    
    public static class XssSanitizer
    {
        public static string Sanitize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var sanitized = Regex.Replace(input, @"<script[^>]*>[\s\S]*?</script>", string.Empty, RegexOptions.IgnoreCase);

            sanitized = Regex.Replace(sanitized, @"javascript:", string.Empty, RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"vbscript:", string.Empty, RegexOptions.IgnoreCase);

            sanitized = Regex.Replace(sanitized, @"\bon\w+\s*=", string.Empty, RegexOptions.IgnoreCase);

            return WebUtility.HtmlEncode(sanitized.Trim());
        }

        public static string CleanHtml(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            return Regex.Replace(input, @"<[^>]*>", string.Empty).Trim();
        }
    }
}
