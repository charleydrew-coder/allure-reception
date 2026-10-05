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
    private const string Dashboard =
        "https://www.salonallure.co.uk/dashbboard";

    private readonly WebView2 browser = new()
    {
        Dock = DockStyle.Fill
    };

    private readonly Panel content = new()
    {
        Dock = DockStyle.Fill
    };

    private readonly ToolStripLabel status = new("Loading…");

    private readonly ToolStripButton returnButton =
        new("Back to dashboard")
        {
            Visible = false
        };

    private readonly List<WebView2> popups = new();

    private CoreWebView2Environment? environment;

    private WebView2 ActiveBrowser =>
        popups.Count > 0 ? popups[^1] : browser;

    public ReceptionWindow()
    {
        Text = "Allure Reception";
        MinimumSize = new Size(320, 400);
        StartPosition = FormStartPosition.Manual;

        var area = Screen.PrimaryScreen!.WorkingArea;

        Bounds = new Rectangle(
            area.Right - 400,
            area.Top,
            400,
            area.Height
        );

        var toolbar = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            Dock = DockStyle.Top
        };

        var home = new ToolStripButton("Home");
        var refresh = new ToolStripButton("Refresh");

        var pin = new ToolStripButton("Keep on top")
        {
            CheckOnClick = true
        };

        var smaller = new ToolStripButton("−");
        var larger = new ToolStripButton("+");

        home.Click += (_, _) =>
        {
            CloseAllPopups();

            if (browser.CoreWebView2 is not null)
            {
                browser.CoreWebView2.Navigate(Dashboard);
            }
        };

        refresh.Click += (_, _) =>
            ActiveBrowser.CoreWebView2?.Reload();

        pin.CheckedChanged += (_, _) =>
            TopMost = pin.Checked;

        smaller.Click += (_, _) =>
        {
            var active = ActiveBrowser;

            if (active.CoreWebView2 is not null)
            {
                active.ZoomFactor = Math.Max(
                    0.5,
                    active.ZoomFactor - 0.1
                );
            }
        };

        larger.Click += (_, _) =>
        {
            var active = ActiveBrowser;

            if (active.CoreWebView2 is not null)
            {
                active.ZoomFactor = Math.Min(
                    2,
                    active.ZoomFactor + 0.1
                );
            }
        };

        returnButton.Click += (_, _) =>
        {
            CloseAllPopups();
            browser.CoreWebView2?.Reload();
        };

        toolbar.Items.AddRange(new ToolStripItem[]
        {
            home,
            refresh,
            pin,
            smaller,
            larger,
            returnButton
        });

        var footer = new StatusStrip();
        footer.Items.Add(status);

        content.Controls.Add(browser);

        Controls.Add(content);
        Controls.Add(toolbar);
        Controls.Add(footer);

        Shown += async (_, _) => await InitialiseBrowser();
    }

    private async Task InitialiseBrowser()
    {
        try
        {
            var dataFolder = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData
                ),
                "AllureReception",
                "BrowserData"
            );

            environment = await CoreWebView2Environment.CreateAsync(
                userDataFolder: dataFolder
            );

            await browser.EnsureCoreWebView2Async(environment);

            ConnectBrowser(browser);

            browser.CoreWebView2.Navigate(Dashboard);
        }
        catch (WebView2RuntimeNotFoundException)
        {
            status.Text = "Microsoft Edge WebView2 Runtime is required.";

            var answer = MessageBox.Show(
                "Install the Microsoft Edge WebView2 Evergreen Runtime, " +
                "then reopen Allure Reception. Open Microsoft's download page?",
                "Allure Reception",
                MessageBoxButtons.YesNo
            );

            if (answer == DialogResult.Yes)
            {
                Process.Start(new ProcessStartInfo(
                    "https://developer.microsoft.com/en-us/microsoft-edge/webview2/"
                )
                {
                    UseShellExecute = true
                });
            }
        }
        catch (Exception error)
        {
            status.Text = "Unable to start the dashboard.";

            MessageBox.Show(
                error.Message,
                "Allure Reception"
            );
        }
    }

    private void ConnectBrowser(WebView2 view)
    {
        view.CoreWebView2.NewWindowRequested += OpenInsideApp;

        view.CoreWebView2.NavigationStarting += (_, _) =>
        {
            if (view == ActiveBrowser)
            {
                status.Text = "Loading…";
            }
        };

        view.CoreWebView2.NavigationCompleted += (_, e) =>
        {
            if (view != ActiveBrowser)
            {
                return;
            }

            status.Text = e.IsSuccess
                ? "Allure Reception"
                : "Unable to load. Check connection and press Refresh.";
        };
    }

    private async void OpenInsideApp(
        object? sender,
        CoreWebView2NewWindowRequestedEventArgs e
    )
    {
        using var deferral = e.GetDeferral();

        e.Handled = true;

        var popup = new WebView2
        {
            Dock = DockStyle.Fill
        };

        try
        {
            content.Controls.Add(popup);
            popups.Add(popup);

            popup.BringToFront();
            returnButton.Visible = true;

            await popup.EnsureCoreWebView2Async(environment);

            ConnectBrowser(popup);

            popup.CoreWebView2.WindowCloseRequested += (_, _) =>
            {
                // Defer disposal until the browser event has finished.
                BeginInvoke(new Action(() =>
                {
                    ClosePopup(popup);
                    browser.CoreWebView2?.Reload();
                }));
            };

            // Preserve the popup connection to its original page.
            // Both views use the same environment and login storage.
            e.NewWindow = popup.CoreWebView2;

            status.Text =
                "Complete sign-in, then use Back to dashboard if needed.";
        }
        catch (Exception error)
        {
            ClosePopup(popup);

            MessageBox.Show(
                "Unable to open sign-in inside the app:\n\n" +
                error.Message,
                "Allure Reception"
            );
        }
    }

    private void ClosePopup(WebView2 popup)
    {
        if (!popups.Remove(popup))
        {
            return;
        }

        content.Controls.Remove(popup);
        popup.Dispose();

        ActiveBrowser.BringToFront();
        returnButton.Visible = popups.Count > 0;
        status.Text = "Allure Reception";
    }

    private void CloseAllPopups()
    {
        foreach (var popup in popups.ToArray())
        {
            ClosePopup(popup);
        }
    }
}
