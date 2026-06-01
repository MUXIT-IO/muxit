// SPDX-License-Identifier: Apache-2.0
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
    /// Extract a named int from dictionary-style action args.
    /// If <paramref name="args"/> is not a dictionary or the key is missing/null,
    /// returns <paramref name="def"/>. Throws <see cref="ArgumentException"/> when
    /// the value is present but is a non-scalar (dictionary / collection / host
    /// script object) — silently <c>ToString()</c>ing those would let a
    /// marshalling bug create nonsense values, so we fail loudly instead.
    /// </summary>
    public static int ArgInt(object? args, string key, int def)
    {
        if (!TryGetArg(args, key, out var v)) return def;
        RejectComplex(key, v, "int");
        try { return Convert.ToInt32(v); }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
        { throw new ArgumentException($"Argument '{key}' expected int, got {Describe(v)}", ex); }
    }

    /// <summary>Extract a named double from dictionary-style action args. See <see cref="ArgInt"/> for failure semantics.</summary>
    public static double ArgDouble(object? args, string key, double def)
    {
        if (!TryGetArg(args, key, out var v)) return def;
        RejectComplex(key, v, "double");
        try { return Convert.ToDouble(v); }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
        { throw new ArgumentException($"Argument '{key}' expected double, got {Describe(v)}", ex); }
    }

    /// <summary>Extract a named string from dictionary-style action args. See <see cref="ArgInt"/> for failure semantics.</summary>
    public static string ArgString(object? args, string key, string def)
    {
        if (!TryGetArg(args, key, out var v)) return def;
        RejectComplex(key, v, "string");
        return v switch
        {
            string s => s,
            // Primitives have a meaningful ToString — keep the lenient behaviour
            // so e.g. writeText({ path: 42 }) still produces "42".
            bool or sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal => v!.ToString()!,
            _ => throw new ArgumentException($"Argument '{key}' expected string, got {Describe(v)}"),
        };
    }

    /// <summary>Extract a named bool from dictionary-style action args. See <see cref="ArgInt"/> for failure semantics.</summary>
    public static bool ArgBool(object? args, string key, bool def)
    {
        if (!TryGetArg(args, key, out var v)) return def;
        RejectComplex(key, v, "bool");
        try { return Convert.ToBoolean(v); }
        catch (Exception ex) when (ex is FormatException or InvalidCastException)
        { throw new ArgumentException($"Argument '{key}' expected bool, got {Describe(v)}", ex); }
    }

    private static bool TryGetArg(object? args, string key, out object? value)
    {
        if (args is IDictionary<string, object?> dict && dict.TryGetValue(key, out var v) && v != null)
        {
            value = v;
            return true;
        }
        value = null;
        return false;
    }

    /// <summary>
    /// Reject dictionary, collection, and host script-object values up-front so the
    /// caller gets a clear "expected X, got Y" error instead of a class-name string
    /// or a confusing InvalidCastException from Convert.ToXxx.
    /// </summary>
    private static void RejectComplex(string key, object? value, string expected)
    {
        if (value is null) return;
        if (value is string) return;
        if (value is System.Collections.IDictionary
            || value is System.Collections.IEnumerable
            // The host marshals JS args via ClearScript; a V8 ScriptObject lives in
            // Microsoft.ClearScript and would create an awkward dependency to
            // reference directly. The type-name check stays robust without coupling
            // the SDK to it.
            || value.GetType().FullName?.StartsWith("Microsoft.ClearScript.", StringComparison.Ordinal) == true)
        {
            throw new ArgumentException($"Argument '{key}' expected {expected}, got {Describe(value)}");
        }
    }

    private static string Describe(object? value) =>
        value switch
        {
            null => "null",
            string s => $"string \"{s}\"",
            _ => value.GetType().Name,
        };

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
