using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UziSport.Service
{
    public static class VietText
    {
        public static string ToAsciiVietnamese(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Normalize tách dấu
            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);

            foreach (var ch in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }

            // Ghép lại và thay Đ/đ
            return sb.ToString()
                     .Normalize(NormalizationForm.FormC)
                     .Replace('Đ', 'D')
                     .Replace('đ', 'd');
        }
    }
}
