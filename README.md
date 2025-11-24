# CS-Hub

A small terminal GUI "hub" for working with C# files and projects.  
This tool provides a lightweight TUI editor, project creation (via `dotnet new`), recursive project browsing of `.cs` files with next/previous navigation, and basic NuGet package management using the `dotnet` CLI.

Release: v0.0.2

---

## Highlights (v0.0.2)

- Create new C# projects from templates (console, classlib, blazor, webapi, wpf, winforms).
- Open a folder and browse all `.cs` files recursively.
- Built-in editor (Terminal.Gui TextView) with:
  - Save
  - Find dialog
  - Previous / Next navigation (auto-save on hop)
- Manage NuGet packages for a selected `.csproj` using `dotnet list/add/remove`.
- Minimal, keyboard-driven Terminal.Gui UI combined with small console prompts.

---

## Requirements

- .NET SDK (dotnet) — confirm with:
```powershell
dotnet --info
```
- Windows recommended for WPF/WinForms template creation.
- `dotnet` must be available on PATH (used for project creation and package management).
- The project uses `Terminal.Gui` for the TUI (already referenced in the project).

---

## Build & Run

From the repository root (PowerShell):

```powershell
dotnet build
dotnet run
```

When launched you'll see a console menu with numbered options. Enter the number to choose an action.

---

## Main Menu (quick overview)

1. Edit A File  
2. Create A File  
3. Exit  
4. Create a new C# Project  
5. Open a C# Project  
6. Manage NuGet Packages  
7–11. Placeholder items (coming soon)

---

## Features & Usage

### Create new C# project (Menu → 4)
- Enter a project name and a full path for project location.
- Pick a template from a small menu (console, class library, Blazor, webapi, WPF, WinForms).
- The tool will run `dotnet new <template>` to create the project and print the `dotnet` output to the console.

Notes:
- WPF/WinForms templates require Windows and the appropriate SDKs/workloads.
- Paths containing spaces are quoted when passed to the CLI.

### Open a C# Project and Edit (Menu → 5)
- Select a folder; the app will recursively gather `*.cs` files under the folder.
- Pick a `.cs` file from a list to open it in the built-in editor.
- Editor capabilities:
  - File → Save (Ctrl+S)
  - File → Find (Ctrl+F) to search and navigate to matches
  - File → Prev / Next (Ctrl+Left / Ctrl+Right) — automatically saves current buffer and opens the previous/next `.cs` in the project list
  - Quit editor to return to main menu

Implementation note:
- Each open/close of the editor currently re-initializes the Terminal.Gui Application. This is simple and reliable; a smoother single-runtime buffer-swap refactor is possible.

### Manage NuGet Packages (Menu → 6)
- Choose a folder containing one or more `.csproj` files.
- If multiple `.csproj` files are found, pick the project to manage.
- Available actions:
  - List installed packages — runs `dotnet list <proj> package` and displays results in a dialog.
  - Add package — prompts for package id and optional version, runs `dotnet add <proj> package <id> [--version <v>]`.
  - Remove package — prompts for package id, runs `dotnet remove <proj> package <id>`.

Notes:
- The UI currently uses Terminal.Gui dialogs for folder/project selection and result display, but Add/Remove prompts are console prompts. Converting these prompts into TUI dialogs is straightforward and planned.
- All `dotnet` command output (stdout/stderr) is captured and shown to the user, along with exit code.
- Commands run with a default timeout (120s). Increase timeout if needed.

---

## Developer Notes

Key code locations (single-file app):
- `Program.cs` — main app source and UI implementation.
  - `CreateCSharpProjectIntro()` / `CreateCSharpProject(...)` — project creation flow.
  - `OpenCSharpProject()` — folder picker and `.cs` file discovery.
  - `Edit(string file, List<string>? projectFiles = null, int index = -1)` — editor with find and prev/next navigation.
  - `ManageNuGetPackages()` — nuget management using `dotnet` CLI.
  - `RunProcessCapture(...)` — helper that starts a process, returns stdout, stderr and exit code.

Quality & limitations
- Add/Remove package prompts use `Console.ReadLine()`; mixing console IO and Terminal.Gui is functional but inconsistent. Consider replacing console prompts with Terminal.Gui dialogs for a unified TUI experience.
- `dotnet` CLI must be non-interactive for the invoked commands; if the CLI prompts for extra input, the process may block.
- The app quotes project paths with spaces but doesn't deeply validate user input (avoid passing untrusted raw input to shell commands).

---

## License

See `LICENSE` in repository root.

---

Thanks for using CS‑Hub — you can always dm me in discord, my username in ikobiz
