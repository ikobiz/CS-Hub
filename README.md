# CS-Hub

A small Terminal.Gui-based terminal hub for C# development. CS-Hub provides quick in-terminal workflows for creating projects, browsing and editing .cs files, and basic NuGet package management — all from a simple menu-driven interface.

Status: Early / experimental (v0.0.3)

## Features

- Create new C# projects using the .NET CLI (`dotnet new`).
- Open a project folder and browse discovered `.cs` files (recursive).
- Built-in text editor (Terminal.Gui `TextView`) with:
  - Save
  - Find (search and jump to results)
  - Prev/Next navigation across project `.cs` files (auto-saves on hop)
- Manage NuGet packages with the `dotnet` CLI:
  - List installed packages
  - Add packages (specify package id and optional version)
  - Remove packages
  - Command output displayed in Terminal.Gui dialogs
- Open documentation, GitHub, Discord links in the default browser (cross-platform helper).
- Help viewer: prefers repository `README.md` if present, otherwise built-in help.

## Quickstart

Prerequisites:
- .NET SDK (dotnet) installed and on PATH — required for project creation and NuGet operations.
- Terminal.Gui package (the project should reference this; see "Dependencies" below).

Build and run from repo root in PowerShell:

```powershell
dotnet build
dotnet run --project .\cshub.csproj
```

When you run the app you'll see a simple numbered menu:
- `1` Edit a file
- `2` Create a file
- `4` Create a new C# project (scaffolds via `dotnet new`)
- `5` Open a C# project (pick a folder, enumerate `.cs` files)
- `6` Manage NuGet packages (list/add/remove using `dotnet` CLI)
- `7` View Documentation (opens README on GitHub)
- `10` Help (shows README.md if present, otherwise built-in help)
- `12` Github page (opens project page)

Editor notes:
- Use the File menu in the editor to Save, Find, Prev, Next, or Quit.
- Prev/Next will auto-save and open the previous/next `.cs` file from the project file list.

## Dependencies

- Terminal.Gui — the TUI library used by the app.
  - If the project doesn't already reference it, add with:
    ```powershell
    dotnet add .\cshub.csproj package Terminal.Gui
    ```

The app also relies on the `dotnet` CLI being available for creating projects and managing NuGet packages.

## Known Limitations

- Some UI flows still mix console prompts and Terminal.Gui dialogs (notably the Add/Remove NuGet prompt inputs). Results are shown in dialogs.
- `CreateCSharpProject()` currently uses a `cmd.exe` wrapper to run `dotnet new`. Refactoring to call `dotnet` directly via `ProcessStartInfo` is recommended for more consistent cross-platform behavior.
- The editor re-initializes Terminal.Gui per edit session; keeping a single Application instance could improve UX.
- This is an early experiment — expect occasional rough edges.

## Troubleshooting

- "dotnet not found": ensure the .NET SDK is installed and `dotnet` is on your PATH.
- Terminal.Gui errors: make sure the package is referenced in `cshub.csproj` and that the app runs in a terminal that supports the library.
- If the app appears to freeze while running an external command, the RunProcess helper uses a timeout; rebuild after modifying any process code.

## Contributing

Contributions are welcome. Small, focused PRs are easiest to review. Ideas for improvements:
- Convert Add/Remove NuGet prompts to Terminal.Gui dialogs.
- Parse `dotnet list package` output into a selectable UI for package removal.
- Replace `cmd.exe` usage with direct `dotnet` ProcessStartInfo calls.
- Keep a single Terminal.Gui `Application` instance across editor sessions.

Please open issues or PRs on the repository.

## License

See the `LICENSE` file in this repository for license terms.

## Credits

- Author / Maintainer: ikobiz
- Terminal UI powered by Terminal.Gui
