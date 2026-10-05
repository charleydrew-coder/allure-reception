# Allure Reception for Windows 11

A compact Windows window for https://www.salonallure.co.uk/dashbboard (the spelling with two b's is intentional).

Starts at 400 pixels wide on the right of the main screen, resizes down to 320 pixels, has refresh, zoom and optional keep-on-top controls. Staff login cookies are stored separately from Edge in the current Windows user's local app data. Other browser sessions do not open inside this app. Links requesting a new window open in the default browser.

## Download and install

1. Open this repository's **Actions** tab and select the latest successful **Build Windows app** run.
2. Download **Allure-Reception-Windows-Installer** under Artifacts (GitHub sign-in required).
3. Extract the downloaded ZIP and run `AllureReceptionSetup.exe` on each reception PC.
4. Open the desktop shortcut and sign in with the Wix reception staff member account. Enable dashboard sound if prompted by the page.

The installer is built once and can be copied to other Windows 11 x64 PCs. Each PC still needs installation and staff sign-in. The app includes its .NET runtime. Microsoft Edge WebView2 Runtime is also required and is normally already installed on Windows 11.

This is an unsigned installer, so Windows may show a publisher warning. Use only builds from this repository that you trust.

## Behaviour and limitations

The Wix site remains the source of the dashboard and permissions; site edits appear without rebuilding the app. Existing site submission errors and a fixed-width Wix layout must be fixed in Wix. The app's zoom controls can help with layout but do not make the page responsive. Phorest's staff diary should remain open in a separate window alongside it.

The app is intended to remain open and the PC awake. Polling and sound depend on the dashboard page and its browser audio permission. Login popups that request a separate window open in the normal browser, so prefer the existing Wix member login form in the dashboard.

## Build locally

On Windows with the .NET 8 SDK: `dotnet publish AllureReception/AllureReception.csproj -c Release -r win-x64 --self-contained true -o publish`. Compile `installer.iss` using Inno Setup 6 to make the installer.

Source validation was performed when prepared. Windows compilation and operation need verification through the Actions build and a reception PC.
