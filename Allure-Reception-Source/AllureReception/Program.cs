using System.Diagnostics;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace AllureReception;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new ReceptionWindow());
    }
}

internal sealed class ReceptionWindow : Form
{
    private const string Dashboard = "https://www.salonallure.co.uk/dashbboard";
    private readonly WebView2 browser = new() { Dock = DockStyle.Fill };
    private readonly ToolStripLabel status = new("Loading…");

    public ReceptionWindow()
    {
        Text = "Allure Reception";
        MinimumSize = new Size(320, 400);
        StartPosition = FormStartPosition.Manual;
        var area = Screen.PrimaryScreen!.WorkingArea;
        Bounds = new Rectangle(area.Right - 400, area.Top, 400, area.Height);

        var toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
        var home = new ToolStripButton("Home");
        var refresh = new ToolStripButton("Refresh");
        var pin = new ToolStripButton("Keep on top") { CheckOnClick = true };
        var smaller = new ToolStripButton("−");
        var larger = new ToolStripButton("+");
        home.Click += (_, _) => { if (browser.CoreWebView2 is not null) browser.CoreWebView2.Navigate(Dashboard); };
        refresh.Click += (_, _) => browser.CoreWebView2?.Reload();
        pin.CheckedChanged += (_, _) => TopMost = pin.Checked;
        smaller.Click += (_, _) => { if (browser.CoreWebView2 is not null) browser.ZoomFactor = Math.Max(0.5, browser.ZoomFactor - 0.1); };
        larger.Click += (_, _) => { if (browser.CoreWebView2 is not null) browser.ZoomFactor = Math.Min(2, browser.ZoomFactor + 0.1); };
        toolbar.Items.AddRange(new ToolStripItem[] { home, refresh, pin, smaller, larger });
        var footer = new StatusStrip();
        footer.Items.Add(status);
        Controls.Add(browser);
        Controls.Add(toolbar);
        Controls.Add(footer);
        Shown += async (_, _) => await InitialiseBrowser();
    }

    private async Task InitialiseBrowser()
    {
        try
        {
            var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AllureReception", "BrowserData");
            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: dataFolder);
            await browser.EnsureCoreWebView2Async(environment);
            browser.CoreWebView2.DocumentTitleChanged += (_, _) => Text = "Allure Reception";
            browser.CoreWebView2.NavigationStarting += (_, _) => status.Text = "Loading…";
            browser.CoreWebView2.NavigationCompleted += (_, e) => status.Text = e.IsSuccess ? "Allure Reception" : "Unable to load. Check connection and press Refresh.";
            browser.CoreWebView2.NewWindowRequested += (_, e) =>
            {
                e.Handled = true;
                if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri) && (uri.Scheme == "https" || uri.Scheme == "http"))
                {
                    try { Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true }); }
                    catch { MessageBox.Show("Unable to open this link in your browser.", "Allure Reception"); }
                }
            };
            browser.CoreWebView2.Navigate(Dashboard);
        }
        catch (WebView2RuntimeNotFoundException)
        {
            status.Text = "Microsoft Edge WebView2 Runtime is required.";
            if (MessageBox.Show("Install the Microsoft Edge WebView2 Evergreen Runtime, then reopen Allure Reception. Open Microsoft's download page?", "Allure Reception", MessageBoxButtons.YesNo) == DialogResult.Yes)
                Process.Start(new ProcessStartInfo("https://developer.microsoft.com/en-us/microsoft-edge/webview2/") { UseShellExecute = true });
        }
        catch (Exception error)
        {
            status.Text = "Unable to start the dashboard.";
            MessageBox.Show(error.Message, "Allure Reception");
        }
    }
}
