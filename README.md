# 🖥️ CS-Hub

A small **Terminal.Gui-based terminal hub** for C# development.  
CS-Hub provides quick in-terminal workflows for creating projects, browsing and editing `.cs` files, and basic NuGet package management — all from a simple, menu-driven interface.

**Status:** Early / experimental (v0.1.0)

---

## ✨ Features

- **NEW! Flexible Menu System**  
  Choose between a **Full** or **Minimal** menu layout, with the preference saved persistently in a settings file.

- **Improved Cross-Platform Project Creation**  
  Create new C# projects using the .NET CLI (`dotnet new`).  
  Execution now uses a fully cross-platform approach, resolving prior limitations and ensuring consistency on all operating systems.

- **Project Browsing**  
  Open a project folder and recursively browse discovered `.cs` files.

- **Built-in Text Editor (Terminal.Gui TextView)**  
  - Save  
  - Find (search and jump to results)  
  - Prev/Next navigation across project `.cs` files (auto-saves on hop)

- **NuGet Package Management (via dotnet CLI)**  
  - List installed packages  
  - Add packages (specify package ID and optional version)  
  - Remove packages  
  - Command output and errors displayed reliably in **Terminal.Gui dialogs**

- **Cross-Platform Helpers**  
  Open documentation, GitHub, and Discord links in the default browser.

- **Help Viewer**  
  Prefers repository `README.md` if present, otherwise falls back to built-in help.

---

## 🚀 Quickstart

**Prerequisites:**
- .NET SDK (`dotnet`) installed and on PATH — required for project creation and NuGet operations.
- `Terminal.Gui` package referenced in the project.

**Build and run from the repo root:**

```powershell
dotnet build
dotnet run --project .\cshub.csproj
