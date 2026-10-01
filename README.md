# Disable Mouse Side Buttons

A tiny Windows tray app that swallows both mouse side buttons (XButton1 / XButton2, usually Back and Forward).

It uses a low-level mouse hook, so it applies to every mouse on the machine — browsers, File Explorer, and most desktop apps. No vendor software required.

## Requirements

- Windows
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) to build
- [.NET 9 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) to run a framework-dependent build

## Build and run

```powershell
dotnet publish src/DisableMouseSideButtons.csproj -c Release -r win-x64 --self-contained false -o publish
.\publish\DisableMouseSideButtons.exe
```

A shield icon appears in the notification area.

## Tray

- **Disable side buttons** — checked means the buttons are blocked
- Double-click the icon to toggle
- **Exit** — quit the app (side buttons work again)

## Start with Windows

Copy a shortcut to:

```
%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup
```

## Notes

Some games read the mouse through Raw Input and will still see the side buttons. The hook covers the usual Back/Forward navigation in Windows and browsers.

Only one instance runs at a time.
