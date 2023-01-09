using PicoController.Gui.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PicoController.Gui.Extensions.InterfaceEditors;
using SettingsIEnumerable = IEnumerable<ReactiveKeyValuePair<string, JsonElement>>;

internal static class InterfaceEditorExtensions
{
    public static JsonElement? Find(this SettingsIEnumerable settings, string key)
        => settings.FirstOrDefault(x => x.Key == key)?.Value;

    public static bool TryFindOfKind(this SettingsIEnumerable settings, string key, JsonValueKind kind, out JsonElement? element)
    {
        var el = Find(settings, key);
        element = el is JsonElement e && e.ValueKind == kind ? el : null;
        return element is not null;
    }

    public static int GetInt32(this SettingsIEnumerable settings, string key, int defaultValue = default)
        => settings.TryFindOfKind(key, JsonValueKind.Number, out var item)
        && item?.TryGetInt32(out int value) == true
            ? value : defaultValue;

    public static string? GetString(this SettingsIEnumerable settings, string key, string? defaultValue = default)
        => settings.TryFindOfKind(key, JsonValueKind.String, out var item)
            ? item?.GetString() : defaultValue;
}
