using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Tungsten.Helpers;

public static partial class ExtensionMethods {

    public static HtmlString NavLink(this IHtmlHelper helper, string title, string href, bool? active = null) {
        bool isActive = false;
        if (active.HasValue) {
            isActive = active.Value;
        } else {
            string? path = helper.ViewContext.HttpContext.Request.Path.ToString();
            if (string.Equals(path, href, StringComparison.OrdinalIgnoreCase)) {
                isActive = true;
            }
        }

        string activeOrBlank = isActive ? " active" : "";
        string aria = isActive ? " aria-current=\"page\" " : "";
        return new HtmlString(@$"<li class=""nav-item""><a class=""nav-link{activeOrBlank}"" {aria} href=""{href}"">{title}</a></li>");
    }

    public static string CheckboxChecked(this bool? b, bool desired) {
        if (b.HasValue) {
            if (b.Value == desired) {
                return " checked=\"checked\"";
            }
        }

        return "";
    }

    public static string CheckboxChecked(this string? s, string val) {
        if (!string.IsNullOrWhiteSpace(s)) {
            if (s.Equals(val, StringComparison.OrdinalIgnoreCase)) {
                return " checked=\"checked\"";
            }
        }

        return "";
    }

    public static HtmlString Hyperlink(this IHtmlHelper helper, string display, string href) {
        return new HtmlString(@$"<a href=""{href}"" target=""_blank"" rel=""nofollow"">{display}</a>");
    }
}
