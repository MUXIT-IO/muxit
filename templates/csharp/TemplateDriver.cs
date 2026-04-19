// SPDX-License-Identifier: MIT
using System.Net;
using System.Text;
using System.Text.Json;
using Muxit.Driver.Sdk;
using static Muxit.Driver.Sdk.DriverConfig;

namespace Muxit.Driver.Template;

/// <summary>
/// A fully functional test/template driver that exercises every data type
/// and feature of the IConnectorDriver interface. No real hardware needed —
/// all values are stored in memory.
///
/// Includes a built-in HTTP dashboard for visual testing.
///
/// Config options:
///   dashboardPort (int, default 9999) — port for the built-in web dashboard
///   label (string, default "Test Device") — initial label
///   temperature (double, default 22.5) — initial temperature
/// </summary>
public sealed class TemplateDriver : IConnectorDriver
{
    // ── Interface properties ─────────────────────────────────────────────

    public string Name { get; }
    public string? Version => "1.0.0";
    public string? Description => "Template/example driver exercising all data types and features. Clone this as a starting point for your own driver.";
    public bool SupportsStreaming => true;
    public Action<string, string>? StreamEmitter { get; set; }

    // ── Internal state ───────────────────────────────────────────────────

    private string _label = "Test Device";
    private int _count;
    private double _temperature = 22.5;
    private bool _enabled;
    private bool _streaming = true;
    private double _threshold = 50.0;
    private DateTime _initTime;
    private readonly List<string> _logs = new();
    private Timer? _streamTimer;
    private HttpListener? _httpListener;
    private CancellationTokenSource? _httpCts;
    private int _dashboardPort = 9999;

    public TemplateDriver() : this("MyDevice") { }

    public TemplateDriver(string name) { Name = name; }

    // ── Lifecycle ────────────────────────────────────────────────────────

    public Task InitAsync(Dictionary<string, object?>? config)
    {
        _dashboardPort = GetInt(config, "dashboardPort", 9999);
        _label = GetString(config, "label", "Test Device");
        _temperature = GetDouble(config, "temperature", 22.5);
        _initTime = DateTime.UtcNow;
        _logs.Clear();
        _logs.Add($"[{DateTime.Now:HH:mm:ss}] Initialized");

        // Start streaming timer (every 500ms)
        _streamTimer = new Timer(_ => EmitStreamData(), null, 500, 500);

        // Start HTTP dashboard
        StartDashboard();

        _logs.Add($"[{DateTime.Now:HH:mm:ss}] Dashboard at http://localhost:{_dashboardPort}/");
        return Task.CompletedTask;
    }

    public Task ShutdownAsync()
    {
        _streamTimer?.Dispose();
        _streamTimer = null;
        StopDashboard();
        return Task.CompletedTask;
    }

    // ── Schema ───────────────────────────────────────────────────────────

    public IEnumerable<PropertyDescriptor> GetProperties() => new[]
    {
        new PropertyDescriptor("label",       "string",   "R/W", "",   "Device label"),
        new PropertyDescriptor("count",       "int",      "R/W", "",   "Event counter"),
        new PropertyDescriptor("temperature", "double",   "R/W", "°C", "Current temperature"),
        new PropertyDescriptor("enabled",     "bool",     "R/W", "",   "Whether device is active"),
        new PropertyDescriptor("streaming",   "bool",     "R/W", "",   "Enable/disable stream output"),
        new PropertyDescriptor("threshold",   "double",   "R/W", "",   "Alert threshold"),
        new PropertyDescriptor("uptime",      "double",   "R",   "s",  "Seconds since init"),
        new PropertyDescriptor("spectrum",    "double[]", "R",   "",   "Sample spectrum data (10 points)"),
        new PropertyDescriptor("histogram",   "int[]",    "R",   "",   "Sample histogram (8 bins)"),
        new PropertyDescriptor("logs",        "string[]", "R",   "",   "Recent log messages"),
        new PropertyDescriptor("status",      "object",   "R",   "",   "Composite status object"),
    };

    public IEnumerable<ActionDescriptor> GetActions() => new[]
    {
        new ActionDescriptor("reset",        "Reset all values to defaults"),
        new ActionDescriptor("setThreshold", "Set the alert threshold", [
            new ArgDescriptor("value", "double", "Threshold value (0-100)")]),
        new ActionDescriptor("configure",    "Set a named config value", [
            new ArgDescriptor("key", "string", "Configuration key name"),
            new ArgDescriptor("value", "string", "Configuration value")]),
        new ActionDescriptor("loadProfile",  "Load a numeric profile array", [
            new ArgDescriptor("values", "double[]", "Array of numeric profile values")]),
        new ActionDescriptor("calculate",    "Add two numbers and return the result", [
            new ArgDescriptor("a", "double", "First operand"),
            new ArgDescriptor("b", "double", "Second operand")]),
    };

    public IEnumerable<string> GetStreams() => new[] { "data" };

    // ── Property access ──────────────────────────────────────────────────

    public Task<object?> GetAsync(string property) => Task.FromResult<object?>(property switch
    {
        "label" => _label,
        "count" => _count,
        "temperature" => _temperature,
        "enabled" => _enabled,
        "streaming" => _streaming,
        "threshold" => _threshold,
        "uptime" => (DateTime.UtcNow - _initTime).TotalSeconds,
        "spectrum" => GenerateSpectrum(),
        "histogram" => GenerateHistogram(),
        "logs" => _logs.TakeLast(20).ToArray(),
        "status" => new Dictionary<string, object?>
        {
            ["label"] = _label,
            ["enabled"] = _enabled,
            ["temperature"] = _temperature,
            ["count"] = _count,
            ["uptime"] = (DateTime.UtcNow - _initTime).TotalSeconds,
        },
        _ => throw new ArgumentException($"Unknown property: {property}"),
    });

    public Task SetAsync(string property, object? value)
    {
        switch (property)
        {
            case "label":       _label = DriverConfig.ToString(value, _label); break;
            case "count":       _count = ToInt(value); break;
            case "temperature": _temperature = ToDouble(value); break;
            case "enabled":     _enabled = ToBool(value); break;
            case "streaming":   _streaming = ToBool(value); break;
            case "threshold":   _threshold = ToDouble(value); break;
            default: throw new ArgumentException($"Unknown or read-only property: {property}");
        }
        Log($"Set {property} = {value}");
        return Task.CompletedTask;
    }

    // ── Actions ──────────────────────────────────────────────────────────

    public Task<object?> ExecuteAsync(string action, object? args)
    {
        switch (action)
        {
            case "reset":
                _label = "Test Device";
                _count = 0;
                _temperature = 22.5;
                _enabled = false;
                _streaming = true;
                _threshold = 50.0;
                Log("Reset to defaults");
                return Task.FromResult<object?>("OK");

            case "setThreshold":
                _threshold = ArgDouble(args, "value", _threshold);
                Log($"Threshold set to {_threshold}");
                return Task.FromResult<object?>("OK");

            case "configure":
                var key = ArgString(args, "key", "");
                var val = ArgString(args, "value", "");
                Log($"Configure: {key} = {val}");
                return Task.FromResult<object?>($"Set {key}={val}");

            case "loadProfile":
                if (args is object?[] arr)
                {
                    Log($"Loaded profile with {arr.Length} values");
                    return Task.FromResult<object?>(arr.Length);
                }
                return Task.FromResult<object?>(0);

            case "calculate":
                var a = ArgDouble(args, "a", 0);
                var b = ArgDouble(args, "b", 0);
                var result = a + b;
                Log($"Calculate: {a} + {b} = {result}");
                return Task.FromResult<object?>(result);

            default:
                throw new ArgumentException($"Unknown action: {action}");
        }
    }

    // ── Streaming ────────────────────────────────────────────────────────

    private void EmitStreamData()
    {
        if (StreamEmitter == null || !_streaming) return;
        _count++;
        var data = JsonSerializer.Serialize(new
        {
            timestamp = DateTime.UtcNow,
            temperature = _temperature + (Random.Shared.NextDouble() - 0.5) * 2,
            count = _count,
            enabled = _enabled,
        });
        StreamEmitter("data", data);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private double[] GenerateSpectrum()
    {
        var spectrum = new double[10];
        for (int i = 0; i < spectrum.Length; i++)
            spectrum[i] = Math.Sin(i * 0.5 + _count * 0.01) * 100 + 500;
        return spectrum;
    }

    private int[] GenerateHistogram()
    {
        var hist = new int[8];
        for (int i = 0; i < hist.Length; i++)
            hist[i] = Random.Shared.Next(0, 100);
        return hist;
    }

    private void Log(string message)
    {
        _logs.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        if (_logs.Count > 100) _logs.RemoveRange(0, _logs.Count - 100);
    }

    // ── HTTP Dashboard ───────────────────────────────────────────────────

    private void StartDashboard()
    {
        try
        {
            _httpCts = new CancellationTokenSource();
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add($"http://localhost:{_dashboardPort}/");
            _httpListener.Start();
            Task.Run(() => DashboardLoop(_httpCts.Token));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{Name}] Dashboard failed to start on port {_dashboardPort}: {ex.Message}");
        }
    }

    private void StopDashboard()
    {
        _httpCts?.Cancel();
        try { _httpListener?.Stop(); } catch { }
        _httpListener = null;
    }

    private async Task DashboardLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _httpListener?.IsListening == true)
        {
            try
            {
                var ctx = await _httpListener.GetContextAsync();
                _ = Task.Run(() => HandleHttpRequest(ctx), ct);
            }
            catch (ObjectDisposedException) { break; }
            catch (HttpListenerException) { break; }
            catch { }
        }
    }

    private async Task HandleHttpRequest(HttpListenerContext ctx)
    {
        var path = ctx.Request.Url?.AbsolutePath ?? "/";
        var method = ctx.Request.HttpMethod;

        try
        {
            if (path == "/" && method == "GET")
            {
                await Respond(ctx, "text/html", DashboardHtml);
            }
            else if (path == "/api/properties" && method == "GET")
            {
                var props = new Dictionary<string, object?>();
                foreach (var p in GetProperties())
                {
                    try { props[p.Name] = await GetAsync(p.Name); }
                    catch { props[p.Name] = null; }
                }
                await RespondJson(ctx, props);
            }
            else if (path.StartsWith("/api/set/") && method == "POST")
            {
                var propName = path["/api/set/".Length..];
                var body = await ReadBody(ctx);
                var el = JsonSerializer.Deserialize<JsonElement>(body);
                await SetAsync(propName, NormalizeJsonElement(el));
                await RespondJson(ctx, new { ok = true });
            }
            else if (path.StartsWith("/api/exec/") && method == "POST")
            {
                var actionName = path["/api/exec/".Length..];
                var body = await ReadBody(ctx);
                object? args = null;
                if (!string.IsNullOrWhiteSpace(body))
                {
                    var el = JsonSerializer.Deserialize<JsonElement>(body);
                    args = NormalizeJsonElement(el);
                }
                var result = await ExecuteAsync(actionName, args);
                await RespondJson(ctx, new { result });
            }
            else
            {
                ctx.Response.StatusCode = 404;
                await Respond(ctx, "text/plain", "Not Found");
            }
        }
        catch (Exception ex)
        {
            ctx.Response.StatusCode = 500;
            await Respond(ctx, "application/json",
                JsonSerializer.Serialize(new { error = ex.Message }));
        }
    }

    /// <summary>
    /// Normalize a JsonElement from the HTTP dashboard to native C# types.
    /// This is a local copy — in normal operation, the server handles this
    /// before values reach the driver. The dashboard bypasses that path.
    /// </summary>
    private static object? NormalizeJsonElement(JsonElement el) => el.ValueKind switch
    {
        JsonValueKind.Number => el.TryGetInt32(out var i) ? i : el.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        JsonValueKind.Undefined => null,
        JsonValueKind.String => el.GetString(),
        JsonValueKind.Array => el.EnumerateArray().Select(NormalizeJsonElement).ToArray(),
        JsonValueKind.Object => el.EnumerateObject()
            .ToDictionary(p => p.Name, p => NormalizeJsonElement(p.Value)),
        _ => null,
    };

    private static async Task<string> ReadBody(HttpListenerContext ctx)
    {
        using var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding);
        return await reader.ReadToEndAsync();
    }

    private static async Task Respond(HttpListenerContext ctx, string contentType, string body)
    {
        ctx.Response.ContentType = contentType;
        ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        var bytes = Encoding.UTF8.GetBytes(body);
        ctx.Response.ContentLength64 = bytes.Length;
        await ctx.Response.OutputStream.WriteAsync(bytes);
        ctx.Response.Close();
    }

    private static async Task RespondJson(HttpListenerContext ctx, object value)
    {
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        });
        await Respond(ctx, "application/json", json);
    }

    // ── Dashboard HTML (single-page app) ─────────────────────────────────

    private const string DashboardHtml = """
<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<title>Muxit Test Driver Dashboard</title>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body { font-family: system-ui, sans-serif; background: #1a1a2e; color: #e0e0e0; padding: 20px; }
  h1 { color: #00d4ff; margin-bottom: 20px; font-size: 1.4em; }
  .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; max-width: 900px; }
  .card { background: #16213e; border-radius: 8px; padding: 16px; }
  .card h2 { font-size: 0.9em; color: #888; margin-bottom: 12px; text-transform: uppercase; letter-spacing: 1px; }
  .prop { display: flex; justify-content: space-between; align-items: center; padding: 6px 0; border-bottom: 1px solid #1a1a3e; }
  .prop-name { font-weight: 600; color: #aaa; }
  .prop-value { font-family: monospace; color: #00d4ff; max-width: 60%; text-align: right; overflow: hidden; text-overflow: ellipsis; }
  .prop-value.bool-true { color: #4caf50; }
  .prop-value.bool-false { color: #f44336; }
  input[type="text"], input[type="number"] { background: #0f3460; border: 1px solid #333; color: #e0e0e0; padding: 4px 8px; border-radius: 4px; width: 120px; font-family: monospace; }
  button { background: #0f3460; color: #00d4ff; border: 1px solid #00d4ff33; padding: 6px 14px; border-radius: 4px; cursor: pointer; font-size: 0.85em; }
  button:hover { background: #00d4ff22; }
  .actions { display: flex; flex-direction: column; gap: 8px; }
  .action-row { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; }
  .logs { font-family: monospace; font-size: 0.8em; max-height: 200px; overflow-y: auto; background: #0a0a1a; padding: 10px; border-radius: 4px; }
  .logs div { padding: 2px 0; color: #8888aa; }
  .cli-guide { grid-column: 1 / -1; }
  .cli-guide pre { background: #0a0a1a; padding: 14px; border-radius: 4px; font-size: 0.82em; line-height: 1.6; overflow-x: auto; color: #b0b0c0; }
  .cli-guide code { color: #00d4ff; }
  .cli-guide .comment { color: #555; }
</style>
</head>
<body>
<h1>Muxit Test Driver Dashboard</h1>
<div class="grid">
  <div class="card">
    <h2>Properties</h2>
    <div id="props"></div>
  </div>
  <div class="card">
    <h2>Actions</h2>
    <div class="actions">
      <div class="action-row">
        <button onclick="exec('reset')">Reset</button>
      </div>
      <div class="action-row">
        <button onclick="exec('setThreshold',{value:parseFloat(document.getElementById('tv').value)})">setThreshold</button>
        <input type="number" id="tv" value="50" step="0.1">
      </div>
      <div class="action-row">
        <button onclick="exec('configure',{key:document.getElementById('ck').value,value:document.getElementById('cv').value})">configure</button>
        <input type="text" id="ck" placeholder="key" style="width:80px">
        <input type="text" id="cv" placeholder="value" style="width:80px">
      </div>
      <div class="action-row">
        <button onclick="exec('calculate',{a:parseFloat(document.getElementById('ca').value),b:parseFloat(document.getElementById('cb').value)})">calculate</button>
        <input type="number" id="ca" value="3" style="width:60px"> +
        <input type="number" id="cb" value="4" style="width:60px">
        <span id="calcResult" style="color:#4caf50"></span>
      </div>
    </div>
  </div>
  <div class="card">
    <h2>Logs</h2>
    <div class="logs" id="logs"></div>
  </div>
  <div class="card">
    <h2>Writable Properties</h2>
    <div class="actions">
      <div class="action-row">
        <span style="width:90px">label:</span>
        <input type="text" id="wl" value="">
        <button onclick="setProp('label',document.getElementById('wl').value)">Set</button>
      </div>
      <div class="action-row">
        <span style="width:90px">count:</span>
        <input type="number" id="wc" value="0">
        <button onclick="setProp('count',parseInt(document.getElementById('wc').value))">Set</button>
      </div>
      <div class="action-row">
        <span style="width:90px">temperature:</span>
        <input type="number" id="wt" value="22.5" step="0.1">
        <button onclick="setProp('temperature',parseFloat(document.getElementById('wt').value))">Set</button>
      </div>
      <div class="action-row">
        <span style="width:90px">enabled:</span>
        <button id="we" onclick="toggleBool('enabled')">false</button>
      </div>
      <div class="action-row">
        <span style="width:90px">streaming:</span>
        <button id="ws" onclick="toggleBool('streaming')">true</button>
      </div>
    </div>
  </div>
  <div class="card cli-guide">
    <h2>Testing with Muxit CLI</h2>
    <p style="color:#888;margin-bottom:10px;font-size:0.85em">You can also test this driver from the command line. Run <code>node start.js cli</code> and try these commands:</p>
    <pre><span class="comment"># Discover drivers</span>
<code>scan ../workspace/drivers/native</code>

<span class="comment"># View schema (properties, actions, streams)</span>
<code>meta MyDevice</code>

<span class="comment"># Initialize the driver (starts this dashboard)</span>
<code>init MyDevice</code>
<code>init MyDevice {dashboardPort: 8080}</code>          <span class="comment"># custom port</span>

<span class="comment"># Read properties</span>
<code>get MyDevice temperature</code>                      <span class="comment"># double</span>
<code>get MyDevice label</code>                            <span class="comment"># string</span>
<code>get MyDevice enabled</code>                          <span class="comment"># bool</span>
<code>get MyDevice spectrum</code>                         <span class="comment"># double[]</span>
<code>get MyDevice histogram</code>                        <span class="comment"># int[]</span>
<code>get MyDevice status</code>                           <span class="comment"># object (dict)</span>
<code>get MyDevice logs</code>                             <span class="comment"># string[]</span>
<code>get MyDevice uptime</code>                           <span class="comment"># computed read-only</span>

<span class="comment"># Write properties</span>
<code>set MyDevice temperature 99.9</code>
<code>set MyDevice label Hello</code>
<code>set MyDevice enabled true</code>
<code>set MyDevice count 42</code>

<span class="comment"># Execute actions</span>
<code>exec MyDevice reset</code>                           <span class="comment"># no args</span>
<code>exec MyDevice setThreshold {value: 75.0}</code>      <span class="comment"># named arg</span>
<code>exec MyDevice calculate {a: 3, b: 4}</code>          <span class="comment"># returns result</span>
<code>exec MyDevice configure {key: mode, value: fast}</code>

<span class="comment"># Clean up</span>
<code>shutdown MyDevice</code></pre>
  </div>
</div>
<script>
const BASE = '';
let enabled = false;

async function poll() {
  try {
    const r = await fetch(BASE + '/api/properties');
    const data = await r.json();
    const el = document.getElementById('props');
    el.innerHTML = '';
    for (const [k, v] of Object.entries(data)) {
      if (['spectrum','histogram','status'].includes(k)) {
        const s = JSON.stringify(v);
        el.innerHTML += `<div class="prop"><span class="prop-name">${k}</span><span class="prop-value" title="${s.replace(/"/g,'&quot;')}">${s.length > 40 ? s.slice(0,40)+'...' : s}</span></div>`;
      } else if (typeof v === 'boolean') {
        el.innerHTML += `<div class="prop"><span class="prop-name">${k}</span><span class="prop-value ${v?'bool-true':'bool-false'}">${v}</span></div>`;
      } else {
        const display = typeof v === 'number' ? (Number.isInteger(v) ? v : v.toFixed(2)) : v;
        el.innerHTML += `<div class="prop"><span class="prop-name">${k}</span><span class="prop-value">${display}</span></div>`;
      }
    }
    if (data.logs) {
      const logsEl = document.getElementById('logs');
      logsEl.innerHTML = data.logs.map(l => `<div>${l}</div>`).join('');
      logsEl.scrollTop = logsEl.scrollHeight;
    }
    enabled = data.enabled;
    updateBoolBtn('we', data.enabled);
    updateBoolBtn('ws', data.streaming);
  } catch(e) { console.error(e); }
}

function updateBoolBtn(id, val) {
  const el = document.getElementById(id);
  el.textContent = val;
  el.style.color = val ? '#4caf50' : '#f44336';
}

async function setProp(name, value) {
  await fetch(BASE + '/api/set/' + name, {method:'POST',body:JSON.stringify(value),headers:{'Content-Type':'application/json'}});
}

async function exec(name, args) {
  const r = await fetch(BASE + '/api/exec/' + name, {method:'POST',body:args?JSON.stringify(args):'{}',headers:{'Content-Type':'application/json'}});
  const data = await r.json();
  if (name === 'calculate') document.getElementById('calcResult').textContent = '= ' + data.result;
}

async function toggleBool(name) {
  const r = await fetch(BASE + '/api/properties');
  const data = await r.json();
  await setProp(name, !data[name]);
}

setInterval(poll, 500);
poll();
</script>
</body>
</html>
""";
}
