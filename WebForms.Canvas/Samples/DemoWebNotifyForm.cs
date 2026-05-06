using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

/// <summary>
/// Demonstrates <see cref="WebBrowser"/> (iframe overlay navigation) and
/// <see cref="NotifyIcon"/> (canvas system-tray icon with balloon tips and context menu).
/// </summary>
public class DemoWebNotifyForm : Form
{
    private WebBrowser  _browser      = null!;
    private TextBox     _addressBar   = null!;
    private Label       _statusLabel  = null!;
    private NotifyIcon  _notifyIcon   = null!;
    private Label       _notifyStatus = null!;
    private Button      _btnBack      = null!;
    private Button      _btnForward   = null!;

    public DemoWebNotifyForm()
    {
        Text          = "WebBrowser & NotifyIcon Demo";
        Width         = 860;
        Height        = 680;
        BackColor     = Color.FromArgb(245, 245, 245);
        AllowResize   = true;
        AllowMove     = true;
        MinimumWidth  = 600;
        MinimumHeight = 500;

        InitializeControls();
        Load += OnLoad;
    }

    private void InitializeControls()
    {
        // ── Title ────────────────────────────────────────────────────────────
        Controls.Add(new Label
        {
            Text      = "WebBrowser & NotifyIcon Demo",
            Left      = 10, Top = 10, Width = 830, Height = 30,
            Font      = new Font("Arial", 16),
            ForeColor = Color.FromArgb(0, 51, 153),
            TextAlign = ContentAlignment.TopCenter
        });

        // ════════════════════════════════════════════════════════════════════
        // TOP SECTION — NotifyIcon controls
        // ════════════════════════════════════════════════════════════════════
        int y = 50;

        Controls.Add(SectionLabel("NotifyIcon (canvas system tray)", 20, y)); y += 25;

        Controls.Add(new Label
        {
            Text      = "The tray icon below is registered in the canvas taskbar (bottom-right).",
            Left      = 20, Top = y, Width = 820, Height = 20,
            ForeColor = Color.DimGray
        });
        y += 25;

        // Row of NotifyIcon control buttons
        var btnShowIcon = new Button { Text = "Show Tray Icon", Left = 20, Top = y, Width = 150, Height = 32 };
        var btnHideIcon = new Button { Text = "Hide Tray Icon", Left = 180, Top = y, Width = 150, Height = 32 };
        var btnBalloon  = new Button { Text = "Show Balloon", Left = 340, Top = y, Width = 150, Height = 32 };
        var btnBalloonWarn = new Button { Text = "Balloon (Warning)", Left = 500, Top = y, Width = 160, Height = 32 };

        btnShowIcon.Click    += (s, e) => { _notifyIcon.Visible = true;  UpdateNotifyStatus(); };
        btnHideIcon.Click    += (s, e) => { _notifyIcon.Visible = false; UpdateNotifyStatus(); };
        btnBalloon.Click     += (s, e) => ShowBalloon(BalloonTipIcon.Info,    "CanvasForms", "Hello from the demo! The WebBrowser is just below.");
        btnBalloonWarn.Click += (s, e) => ShowBalloon(BalloonTipIcon.Warning, "Warning", "This is a warning balloon tip.");

        Controls.Add(btnShowIcon);
        Controls.Add(btnHideIcon);
        Controls.Add(btnBalloon);
        Controls.Add(btnBalloonWarn);
        y += 42;

        _notifyStatus = new Label
        {
            Text      = "Tray icon: visible",
            Left      = 20, Top = y, Width = 400, Height = 20,
            ForeColor = Color.FromArgb(0, 128, 0)
        };
        Controls.Add(_notifyStatus);
        y += 30;

        // ════════════════════════════════════════════════════════════════════
        // BOTTOM SECTION — WebBrowser
        // ════════════════════════════════════════════════════════════════════
        Controls.Add(SectionLabel("WebBrowser (iframe overlay)", 20, y)); y += 22;

        Controls.Add(new Label
        {
            Text      = "The browser area below is a live <iframe> positioned over the canvas. Same-origin pages are fully interactive.",
            Left      = 20, Top = y, Width = 820, Height = 20,
            ForeColor = Color.DimGray
        });
        y += 24;

        // Navigation toolbar ─────────────────────────────────────────────
        _btnBack    = new Button { Text = "◄ Back",    Left = 20, Top = y, Width = 80, Height = 28 };
        _btnForward = new Button { Text = "Forward ►", Left = 108, Top = y, Width = 80, Height = 28 };
        var btnStop    = new Button { Text = "Stop",   Left = 196, Top = y, Width = 60, Height = 28 };
        var btnRefresh = new Button { Text = "↺",      Left = 264, Top = y, Width = 40, Height = 28 };

        _addressBar = new TextBox
        {
            Left   = 312, Top = y + 2, Width = 430, Height = 24,
            Text   = "https://example.com"
        };

        var btnGo = new Button { Text = "Go", Left = 750, Top = y, Width = 50, Height = 28 };

        _btnBack.Click    += (s, e) => _browser.GoBack();
        _btnForward.Click += (s, e) => _browser.GoForward();
        btnStop.Click     += (s, e) => _browser.Stop();
        btnRefresh.Click  += (s, e) => _browser.Refresh();
        btnGo.Click       += (s, e) => NavigateTo(_addressBar.Text);
        _addressBar.KeyDown += (s, e) => { if (e.KeyCode == Keys.Return) NavigateTo(_addressBar.Text); };

        Controls.Add(_btnBack);
        Controls.Add(_btnForward);
        Controls.Add(btnStop);
        Controls.Add(btnRefresh);
        Controls.Add(_addressBar);
        Controls.Add(btnGo);
        y += 36;

        // Quick-nav buttons ──────────────────────────────────────────────
        var btnExample  = new Button { Text = "example.com", Left = 20,  Top = y, Width = 110, Height = 26 };
        var btnMDN      = new Button { Text = "MDN Docs",    Left = 138, Top = y, Width = 110, Height = 26 };
        var btnHtml     = new Button { Text = "HTML Page",   Left = 256, Top = y, Width = 110, Height = 26 };
        var btnBlank    = new Button { Text = "Blank",        Left = 374, Top = y, Width = 80,  Height = 26 };

        btnExample.Click += (s, e) => NavigateTo("https://example.com");
        btnMDN.Click     += (s, e) => NavigateTo("https://developer.mozilla.org");
        btnHtml.Click    += (s, e) => LoadLocalHtml();
        btnBlank.Click   += (s, e) => { _browser.DocumentText = "<html><body style='font-family:sans-serif;padding:20px'><h2>Blank Page</h2></body></html>"; _addressBar.Text = "about:blank"; };

        Controls.Add(btnExample);
        Controls.Add(btnMDN);
        Controls.Add(btnHtml);
        Controls.Add(btnBlank);
        y += 34;

        // Status bar ─────────────────────────────────────────────────────
        _statusLabel = new Label
        {
            Text      = "Ready",
            Left      = 20, Top = y, Width = 780, Height = 20,
            ForeColor = Color.DimGray
        };
        Controls.Add(_statusLabel);
        y += 24;

        // WebBrowser control ─────────────────────────────────────────────
        _browser = new WebBrowser
        {
            Left   = 20, Top = y,
            Width  = 810, Height = Height - y - 50,
            ScriptEnabled = true
        };

        _browser.Navigating       += (s, e) => _statusLabel.Text = $"Navigating to {e.Url}…";
        _browser.Navigated        += (s, e) => { _statusLabel.Text = $"Navigated: {e.Url}"; _addressBar.Text = e.Url?.ToString() ?? ""; };
        _browser.DocumentCompleted += (s, e) => _statusLabel.Text = $"Done: {e.Url}";

        Controls.Add(_browser);
    }

    // ── NotifyIcon setup ─────────────────────────────────────────────────────

    private void OnLoad(object? sender, EventArgs e)
    {
        // Build context menu for the tray icon
        var trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("Open Demo Form",  null, (s2, e2) => Activate());
        trayMenu.Items.Add("Show Balloon",    null, (s2, e2) => ShowBalloon(BalloonTipIcon.Info, "CanvasForms", "Tray icon is working!"));
        trayMenu.Items.Add("-");
        trayMenu.Items.Add("Hide Icon",       null, (s2, e2) => { _notifyIcon.Visible = false; UpdateNotifyStatus(); });

        _notifyIcon = new NotifyIcon
        {
            Text              = "CanvasForms Demo",
            Visible           = true,
            BalloonTipTitle   = "CanvasForms",
            BalloonTipText    = "Demo NotifyIcon is active!",
            BalloonTipIcon    = BalloonTipIcon.Info,
            ContextMenuStrip  = trayMenu
        };

        _notifyIcon.Click       += (s, e) => _statusLabel.Text = "Tray icon clicked!";
        _notifyIcon.DoubleClick += (s, e) => Activate();
        _notifyIcon.BalloonTipClicked += (s, e) => _statusLabel.Text = "Balloon tip clicked!";

        UpdateNotifyStatus();

        // Load default page
        _browser.DocumentText = BuildWelcomeHtml();
        _addressBar.Text = "about:welcome";
        _statusLabel.Text = "Loaded welcome page.";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _notifyIcon?.Dispose();
        base.Dispose(disposing);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void NavigateTo(string url)
    {
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            url = "https://" + url;

        _addressBar.Text  = url;
        _statusLabel.Text = $"Navigating to {url}…";
        _browser.Navigate(url);
    }

    private void LoadLocalHtml()
    {
        _browser.DocumentText = BuildDemoHtml();
        _addressBar.Text = "about:demo";
        _statusLabel.Text = "Loaded local HTML page.";
    }

    private void ShowBalloon(BalloonTipIcon icon, string title, string text)
    {
        _notifyIcon.BalloonTipIcon  = icon;
        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText  = text;
        _notifyIcon.ShowBalloonTip(3000);
        _statusLabel.Text = $"Showing balloon: \"{title}\"";
    }

    private void UpdateNotifyStatus()
    {
        _notifyStatus.Text      = _notifyIcon?.Visible == true ? "Tray icon: visible ✔" : "Tray icon: hidden";
        _notifyStatus.ForeColor = _notifyIcon?.Visible == true ? Color.FromArgb(0, 128, 0) : Color.Gray;
    }

    private static Label SectionLabel(string text, int x, int y) => new Label
    {
        Text      = text,
        Left      = x, Top = y, Width = 600, Height = 20,
        ForeColor = Color.FromArgb(0, 80, 160),
        Font      = new Font("Arial", 10)
    };

    // ── Inline HTML pages ────────────────────────────────────────────────────

    private static string BuildWelcomeHtml() => """
        <!DOCTYPE html>
        <html>
        <head>
          <meta charset="utf-8">
          <style>
            body { font-family: Segoe UI, Arial, sans-serif; padding: 24px; background: #f8f9fa; color: #333; }
            h1 { color: #1a73e8; }
            p  { line-height: 1.6; }
            .box { background: #fff; border: 1px solid #ddd; border-radius: 8px; padding: 16px; margin-top: 12px; }
            .note { font-size: 0.9em; color: #666; border-left: 3px solid #1a73e8; padding-left: 8px; }
          </style>
        </head>
        <body>
          <h1>WebBrowser Demo</h1>
          <div class="box">
            <p>This area is a real <code>&lt;iframe&gt;</code> rendered as an HTML overlay
               on top of the canvas. It is <strong>not</strong> painted into the canvas —
               it is a live, interactive browser frame.</p>
            <p>Use the navigation bar above to:</p>
            <ul>
              <li>Type a URL and press <b>Go</b> or <kbd>Enter</kbd></li>
              <li>Click the quick-nav buttons (example.com, MDN Docs, …)</li>
              <li>Load a local HTML demo page</li>
            </ul>
            <p class="note">Cross-origin navigation is supported. DOM access via
            <code>ExecuteScriptAsync</code> only works for same-origin content
            (browser sandbox restriction).</p>
          </div>
        </body>
        </html>
        """;

    private static string BuildDemoHtml() => """
        <!DOCTYPE html>
        <html>
        <head>
          <meta charset="utf-8">
          <style>
            body { font-family: Segoe UI, Arial, sans-serif; padding: 24px; background: #fff; }
            h2 { color: #1a73e8; }
            button { padding: 8px 16px; margin: 4px; border: none; background: #1a73e8; color: #fff; border-radius: 4px; cursor: pointer; }
            button:hover { background: #1558b0; }
            #output { margin-top: 12px; padding: 10px; background: #f0f4ff; border-radius: 4px; min-height: 40px; }
          </style>
        </head>
        <body>
          <h2>Local HTML Demo Page</h2>
          <p>This page is set via <code>DocumentText</code> (srcdoc). Scripts run inside the iframe.</p>
          <button onclick="document.getElementById('output').innerText='Button 1 clicked at ' + new Date().toLocaleTimeString()">Click Me 1</button>
          <button onclick="document.getElementById('output').innerText='Button 2 clicked at ' + new Date().toLocaleTimeString()">Click Me 2</button>
          <button onclick="document.getElementById('output').style.background = '#' + Math.floor(Math.random()*16777215).toString(16).padStart(6,'0')">Random Colour</button>
          <div id="output">Output will appear here…</div>
        </body>
        </html>
        """;
}
