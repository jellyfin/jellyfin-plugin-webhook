using System;
using System.Globalization;
using HandlebarsDotNet;

namespace Jellyfin.Plugin.Webhook.Helpers;

/// <summary>
/// Handlebar helpers.
/// </summary>
public static class HandlebarsFunctionHelpers
{
    private static readonly HandlebarsBlockHelper StringEqualityHelper = (output, options, context, arguments) =>
    {
        if (arguments.Length != 2)
        {
            throw new HandlebarsException("{{if_equals}} helper must have exactly two arguments");
        }

        var left = GetStringValue(arguments[0]);
        var right = GetStringValue(arguments[1]);
        if (string.Equals(left, right, StringComparison.OrdinalIgnoreCase))
        {
            options.Template(output, context);
        }
        else
        {
            options.Inverse(output, context);
        }
    };

    private static readonly HandlebarsBlockHelper StringExistHelper = (output, options, context, arguments) =>
    {
        if (arguments.Length != 1)
        {
            throw new HandlebarsException("{{if_exist}} helper must have exactly one argument");
        }

        var arg = GetStringValue(arguments[0]);
        if (string.IsNullOrEmpty(arg))
        {
            options.Inverse(output, context);
        }
        else
        {
            options.Template(output, context);
        }
    };

    /// <summary>
    /// Register handlebars helpers.
    /// </summary>
    public static void RegisterHelpers()
    {
        Handlebars.RegisterHelper("if_equals", StringEqualityHelper);
        Handlebars.RegisterHelper("if_exist", StringExistHelper);
        Handlebars.RegisterHelper("link_to", (writer, context, parameters) =>
        {
            writer.WriteSafeString($"<a href='{parameters["url"]}'>{context["text"]}</a>");
        });
    }

    /// <summary>
    /// Base 64 decode.
    /// </summary>
    /// <remarks>
    /// The template is stored as base64 in config.
    /// </remarks>
    /// <param name="base64EncodedData">The encoded data.</param>
    /// <returns>The decoded string.</returns>
    public static string Base64Decode(string? base64EncodedData)
    {
        if (string.IsNullOrEmpty(base64EncodedData))
        {
            return string.Empty;
        }

        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }

    private static string? GetStringValue(object? input)
    {
        // UndefinedBindingResult means the parameter was a part of the provided dataset.
        return input is UndefinedBindingResult or null
            ? null
            : Convert.ToString(input, CultureInfo.InvariantCulture);
    }
}
