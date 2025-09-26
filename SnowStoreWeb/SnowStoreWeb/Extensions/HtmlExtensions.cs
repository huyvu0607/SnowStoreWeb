using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;
namespace SnowStoreWeb.Extensions
{
    public static class HtmlExtensions
    {
        /// <summary>
        /// Hiển thị text với line breaks được chuyển đổi thành HTML <br /> tags
        /// </summary>
        /// <param name="htmlHelper">HTML helper instance</param>
        /// <param name="text">Text cần hiển thị</param>
        /// <returns>HTML content với line breaks</returns>
        public static IHtmlContent DisplayWithBreaks(this IHtmlHelper htmlHelper, string text)
        {
            if (string.IsNullOrEmpty(text))
                return new HtmlString(string.Empty);

            // Encode text để tránh XSS attacks
            var encodedText = HtmlEncoder.Default.Encode(text);

            // Chuyển đổi line breaks thành HTML
            var formattedText = encodedText
                .Replace("\r\n", "<br />")  // Windows line breaks
                .Replace("\r", "<br />")    // Mac line breaks  
                .Replace("\n", "<br />");   // Unix line breaks

            return new HtmlString(formattedText);
        }

        /// <summary>
        /// Hiển thị text với line breaks và tùy chọn CSS class
        /// </summary>
        /// <param name="htmlHelper">HTML helper instance</param>
        /// <param name="text">Text cần hiển thị</param>
        /// <param name="cssClass">CSS class để áp dụng</param>
        /// <returns>HTML content với line breaks trong div có class</returns>
        public static IHtmlContent DisplayWithBreaks(this IHtmlHelper htmlHelper, string text, string cssClass)
        {
            if (string.IsNullOrEmpty(text))
                return new HtmlString($"<div class=\"{cssClass}\"></div>");

            var encodedText = HtmlEncoder.Default.Encode(text);
            var formattedText = encodedText
                .Replace("\r\n", "<br />")
                .Replace("\r", "<br />")
                .Replace("\n", "<br />");

            return new HtmlString($"<div class=\"{cssClass}\">{formattedText}</div>");
        }

        /// <summary>
        /// Hiển thị text với line breaks và giới hạn độ dài
        /// </summary>
        /// <param name="htmlHelper">HTML helper instance</param>
        /// <param name="text">Text cần hiển thị</param>
        /// <param name="maxLength">Độ dài tối đa (0 = không giới hạn)</param>
        /// <returns>HTML content với line breaks</returns>
        public static IHtmlContent DisplayWithBreaks(this IHtmlHelper htmlHelper, string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return new HtmlString(string.Empty);

            // Cắt ngắn text nếu cần
            var displayText = maxLength > 0 && text.Length > maxLength
                ? text.Substring(0, maxLength) + "..."
                : text;

            var encodedText = HtmlEncoder.Default.Encode(displayText);
            var formattedText = encodedText
                .Replace("\r\n", "<br />")
                .Replace("\r", "<br />")
                .Replace("\n", "<br />");

            return new HtmlString(formattedText);
        }

        /// <summary>
        /// Hiển thị text dưới dạng paragraph với line breaks
        /// </summary>
        /// <param name="htmlHelper">HTML helper instance</param>
        /// <param name="text">Text cần hiển thị</param>
        /// <param name="paragraphClass">CSS class cho thẻ p</param>
        /// <returns>HTML paragraph với line breaks</returns>
        public static IHtmlContent DisplayAsParagraph(this IHtmlHelper htmlHelper, string text, string paragraphClass = "")
        {
            if (string.IsNullOrEmpty(text))
                return new HtmlString($"<p class=\"{paragraphClass}\"></p>");

            var encodedText = HtmlEncoder.Default.Encode(text);
            var formattedText = encodedText
                .Replace("\r\n", "<br />")
                .Replace("\r", "<br />")
                .Replace("\n", "<br />");

            var classAttribute = string.IsNullOrEmpty(paragraphClass) ? "" : $" class=\"{paragraphClass}\"";
            return new HtmlString($"<p{classAttribute}>{formattedText}</p>");
        }

        /// <summary>
        /// Chuyển đổi multiple line breaks thành single line break
        /// </summary>
        /// <param name="htmlHelper">HTML helper instance</param>
        /// <param name="text">Text cần hiển thị</param>
        /// <returns>HTML content với line breaks được làm sạch</returns>
        public static IHtmlContent DisplayWithCleanBreaks(this IHtmlHelper htmlHelper, string text)
        {
            if (string.IsNullOrEmpty(text))
                return new HtmlString(string.Empty);

            var encodedText = HtmlEncoder.Default.Encode(text);

            // Loại bỏ multiple line breaks liên tiếp
            var cleanedText = System.Text.RegularExpressions.Regex.Replace(
                encodedText, @"[\r\n]+", "\n");

            var formattedText = cleanedText.Replace("\n", "<br />");

            return new HtmlString(formattedText);
        }
    }
}
