// SPDX-License-Identifier: MIT
namespace Muxit.Driver.Sdk;

/// <summary>
/// Helper methods for safely extracting typed values from config dictionaries
/// and action arguments. All values are native C# types (int, double, string, bool, etc.)
/// — no JSON parsing needed.
/// </summary>
public static class DriverConfig
{
    // ── Config extraction (for InitAsync config dictionary) ──────────────

    /// <summary>Extract a string from config, or return default.</summary>
    public static string GetString(Dictionary<string, object?>? config, string key, string def)
        => config?.TryGetValue(key, out var val) == true ? val?.ToString() ?? def : def;

    /// <summary>Extract an int from config, or return default.</summary>
    public static int GetInt(Dictionary<string, object?>? config, string key, int def)
        => config?.TryGetValue(key, out var val) == true && val != null ? Convert.ToInt32(val) : def;

    /// <summary>Extract a double from config, or return default.</summary>
    public static double GetDouble(Dictionary<string, object?>? config, string key, double def)
        => config?.TryGetValue(key, out var val) == true && val != null ? Convert.ToDouble(val) : def;

    /// <summary>Extract a bool from config, or return default.</summary>
    public static bool GetBool(Dictionary<string, object?>? config, string key, bool def)
        => config?.TryGetValue(key, out var val) == true && val != null ? Convert.ToBoolean(val) : def;

    // ── Argument extraction (for ExecuteAsync args) ─────────────────────

    /// <summary>
    /// Extract a named value from dictionary-style action args.
    /// If args is not a dictionary or key is missing, returns default.
    /// </summary>
    public static int ArgInt(object? args, string key, int def)
        => args is IDictionary<string, object?> dict && dict.TryGetValue(key, out var v) && v != null
            ? Convert.ToInt32(v) : def;

    /// <summary>Extract a named double from dictionary-style action args.</summary>
    public static double ArgDouble(object? args, string key, double def)
        => args is IDictionary<string, object?> dict && dict.TryGetValue(key, out var v) && v != null
            ? Convert.ToDouble(v) : def;

    /// <summary>Extract a named string from dictionary-style action args.</summary>
    public static string ArgString(object? args, string key, string def)
        => args is IDictionary<string, object?> dict && dict.TryGetValue(key, out var v) && v != null
            ? v.ToString() ?? def : def;

    /// <summary>Extract a named bool from dictionary-style action args.</summary>
    public static bool ArgBool(object? args, string key, bool def)
        => args is IDictionary<string, object?> dict && dict.TryGetValue(key, out var v) && v != null
            ? Convert.ToBoolean(v) : def;

    /// <summary>
    /// Extract a double array from action args.
    /// Handles double[], object[] with numeric elements, and single values.
    /// Optionally validates expected length.
    /// </summary>
    public static double[] ArgDoubleArray(object? args, int expectedLength = -1)
    {
        double[] result;
        if (args is double[] da)
            result = da;
        else if (args is object?[] oa)
            result = oa.Select(x => Convert.ToDouble(x)).ToArray();
        else if (args is IList<object?> list)
            result = list.Select(x => Convert.ToDouble(x)).ToArray();
        else
            throw new ArgumentException($"Expected a numeric array, got {args?.GetType().Name ?? "null"}");

        if (expectedLength >= 0 && result.Length != expectedLength)
            throw new ArgumentException($"Expected array of length {expectedLength}, got {result.Length}");

        return result;
    }

    /// <summary>
    /// Try to convert a value to int. Handles int, long, double, string.
    /// </summary>
    public static int ToInt(object? value) => Convert.ToInt32(value);

    /// <summary>Try to convert a value to double.</summary>
    public static double ToDouble(object? value) => Convert.ToDouble(value);

    /// <summary>Try to convert a value to bool.</summary>
    public static bool ToBool(object? value) => Convert.ToBoolean(value);

    /// <summary>Try to convert a value to string.</summary>
    public static string ToString(object? value, string def = "")
        => value?.ToString() ?? def;
}
