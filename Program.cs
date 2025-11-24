using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Terminal.Gui;

namespace Gohub
{
    class Program
    {
        #region Fields
        static string? selectedFile;
        static List<string>? projectCsFiles;
        static int currentFileIndex = -1;
        static string? pendingOpenFile = null;
        static int pendingOpenIndex = -1;
        #endregion

        #region Entry / App Control

        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                Edit(args[0]);
                return;
            }

            Console.WriteLine("Welcome to C# Hub!");
            Menu();
        }

        static void Exit()
        {
            Application.Shutdown();
            Environment.Exit(0);
        }

        #endregion

        #region Menu

        static void Menu()
        {
            Console.WriteLine("\nChoose an option to continue:");
            Console.WriteLine("1. Edit A File");
            Console.WriteLine("2. Create A File");
            Console.WriteLine("3. Exit");
            Console.WriteLine("4. Create a new C# Project");
            Console.WriteLine("5. Open a C# Project");
            Console.WriteLine("6. Manage NuGet Packages");
            Console.WriteLine("7. View Documentation");
            Console.WriteLine("8. Settings(comming soon...)");
            Console.WriteLine("9. About");
            Console.WriteLine("10. Help");
            Console.WriteLine("11. Discord Server");
            Console.WriteLine("12. Github page");

            Console.Write("Your Choice: ");
            string? option = Console.ReadLine();

            if (option == "1")
            {
                SelectFile();
                if (!string.IsNullOrEmpty(selectedFile))
                {
                    Edit(selectedFile);
                }
                Menu();
            }
            else if (option == "2")
            {
                CreateFile();
                Menu();
            }
            else if (option == "3")
            {
                Exit();
            }
            else if (option == "4")
            {
                CreateCSharpProjectIntro();
                Menu();
            }
            else if (option == "5")
            {
                OpenCSharpProject();
                Menu();
            }
            else if (option == "6")
            {
                ManageNuGetPackages();
                Menu();
            }
            else if (option == "7")
            {
                OpenUrl("https://github.com/ikobiz/CS-Hub/blob/main/README.md");
                Menu();
            }
            else if (option == "8")
            {
                // Settings placeholder
                Console.WriteLine("Settings are coming soon...");
                Menu();
            }
            else if (option == "9")
            {
                // About placeholder
                Console.WriteLine("CS-Hub — A terminal GUI hub for C# (v0.0.2+).");
                Menu();
            }
            else if (option == "10")
            {
                ShowHelp();
                Menu();
            }
            else if (option == "11")
            {
                OpenUrl("https://discord.gg/BXMzNu4t");
                Menu();
            }
            else if (option == "12")
            {
                OpenUrl("https://github.com/ikobiz/CS-Hub");
                Menu();
            }
            else
            {
                Console.WriteLine("Invalid option. Please try again.");
                Menu();
            }
        }

        #endregion

        #region Project Management

        static void OpenCSharpProject()
        {
            Application.Init();

            var folderDialog = new OpenDialog("Open C# Project Folder", "Select project folder")
            {
                AllowsMultipleSelection = false,
                CanChooseDirectories = true,
                CanChooseFiles = false
            };

            Application.Run(folderDialog);
            Application.Shutdown();

            if (folderDialog.FilePaths.Count == 0)
            {
                Console.WriteLine("No folder selected.");
                return;
            }

            string selectedFolder = folderDialog.FilePaths[0];

            string[] csFiles;
            try
            {
                csFiles = Directory.GetFiles(selectedFolder, "*.cs", SearchOption.AllDirectories)
                                   .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                                   .ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to enumerate .cs files: {ex.Message}");
                return;
            }

            if (csFiles.Length == 0)
            {
                Console.WriteLine("No .cs files found in the selected folder.");
                return;
            }

            // Build items for list view: show relative paths (relative to selected folder)
            var items = csFiles.Select(p => Path.GetRelativePath(selectedFolder, p)).ToArray();

            Application.Init();
            var dlg = new Dialog("Project .cs Files", 80, 20);

            var listView = new ListView(items)
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 2
            };

            dlg.Add(listView);

            var openBtn = new Button("Open")
            {
                X = Pos.Center() - 10,
                Y = Pos.AnchorEnd(1)
            };
            openBtn.Clicked += () =>
            {
                int idx = listView.SelectedItem;
                if (idx >= 0 && idx < csFiles.Length)
                {
                    projectCsFiles = csFiles.ToList();
                    currentFileIndex = idx;
                    Application.RequestStop();
                }
            };
            dlg.AddButton(openBtn);

            var cancelBtn = new Button("Cancel")
            {
                X = Pos.Center() + 2,
                Y = Pos.AnchorEnd(1)
            };
            cancelBtn.Clicked += () => Application.RequestStop();
            dlg.AddButton(cancelBtn);

            Application.Run(dlg);
            Application.Shutdown();

            if (projectCsFiles != null && currentFileIndex >= 0 && currentFileIndex < projectCsFiles.Count)
            {
                // Open selected file in editor with navigation enabled
                Edit(projectCsFiles[currentFileIndex], projectCsFiles, currentFileIndex);
            }
        }

        static void CreateCSharpProjectIntro()
        {
            Console.WriteLine("Enter project name: ");
            string projectName = Console.ReadLine();

            Console.WriteLine("Enter project location (full path): ");
            string projectLocation = Console.ReadLine();

            Console.WriteLine("Creating a new C# Project...");

            string projectPath = Path.Combine(projectLocation, projectName);
            Directory.CreateDirectory(projectPath);

            Console.WriteLine("What type of project would you like to create?");

            Console.WriteLine("1. Console Application");
            Console.WriteLine("2. Class Library");
            Console.WriteLine("3. Blazor Web App");
            Console.WriteLine("4. ASP.NET Core Web API");
            Console.WriteLine("5. WPF Application");
            Console.WriteLine("6. Windows Forms App");
            Console.Write("Your Choice: ");
            string projectTypeOption = Console.ReadLine();

            if (projectTypeOption == "1" ||
                projectTypeOption == "2" ||
                projectTypeOption == "3" ||
                projectTypeOption == "4" ||
                projectTypeOption == "5" ||
                projectTypeOption == "6")
            {
                CreateCSharpProject(projectPath, projectName, projectTypeOption);
            }
            else
            {
                Console.WriteLine("Invalid option. Defaulting to Console Application.");
                projectTypeOption = "1";
                CreateCSharpProject(projectPath, projectName, projectTypeOption);
            }
        }

        static void CreateCSharpProject(string projectPath, string projectName, string projectType)
        {

            #region Create C# Project Using dotnet CLI

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = psi };
            process.Start();

            // 4. Pass the commands
            process.StandardInput.WriteLine($"cd /d \"{projectPath}\"");  // Command 1: Change directory
            process.StandardInput.WriteLine($"dotnet new {(projectType == "1" ? "console" : projectType == "2" ? "classlib" : projectType == "3" ? "blazorwasm" : projectType == "4" ? "webapi" : projectType == "5" ? "wpf" : "winforms")} -n \"{projectName}\""); // Command 2: Create new project
            /// process.StandardInput.WriteLine("echo %CD%");   // Command 2: Print current directory

            process.StandardInput.Flush();
            process.StandardInput.Close();

            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();

            Console.WriteLine("--- OUTPUT ---");
            // The output will contain "C:\Users"
            Console.WriteLine(output);

            #endregion
        }

        #endregion

        #region File Operations

        #region Create File
        static void CreateFile()
        {
            Application.Init();

            var folderDialog = new OpenDialog("Select Folder", "Choose where to create the file")
            {
                AllowsMultipleSelection = false,
                CanChooseDirectories = true,
                CanChooseFiles = false
            };

            Application.Run(folderDialog);

            if (folderDialog.FilePaths.Count == 0)
            {
                Application.Shutdown();
                Console.WriteLine("No folder selected.");
                return;
            }

            string selectedFolder = folderDialog.FilePaths[0];

            var filenameDialog = new Dialog("New File Name", 60, 10);
            var input = new TextField("")
            {
                X = 1,
                Y = 1,
                Width = 50
            };
            filenameDialog.Add(new Label("Filename:") { X = 1, Y = 0 });
            filenameDialog.Add(input);

            string? filename = null;
            var ok = new Button("Create", is_default: true);
            ok.Clicked += () =>
            {
                filename = input.Text?.ToString();
                Application.RequestStop();
            };
            filenameDialog.AddButton(ok);
            var cancelBtn2 = new Button("Cancel");
            cancelBtn2.Clicked += () => Application.RequestStop();
            filenameDialog.AddButton(cancelBtn2);

            Application.Run(filenameDialog);
            Application.Shutdown();

            if (!string.IsNullOrWhiteSpace(filename))
            {
                string fullPath = Path.Combine(selectedFolder, filename);
                try
                {
                    File.WriteAllText(fullPath, "");
                    Console.WriteLine($"Created file: {fullPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating file: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("No filename provided.");
            }
        }
        #endregion

        static void SelectFile()
        {
            Application.Init();

            var dialog = new OpenDialog("Select a File", "Choose one file")
            {
                AllowsMultipleSelection = false,
                CanChooseDirectories = false
            };

            Application.Run(dialog);

            if (dialog.FilePaths.Count > 0)
            {
                selectedFile = dialog.FilePaths[0];
                Console.WriteLine($"Selected file: {selectedFile}");
            }

            Application.Shutdown();
        }

        #endregion

        #region Editor

        static void Edit(string file, List<string>? projectFiles = null, int index = -1)
        {
            // Track passed-in project files/index
            projectCsFiles = projectFiles;
            currentFileIndex = index;

            Application.Init();

            var top = Application.Top;
            var win = new Window($"Editing: {Path.GetFileName(file)}")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            top.Add(win);

            var textView = new TextView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            win.Add(textView);

            // Load file contents
            try
            {
                textView.Text = File.Exists(file) ? File.ReadAllText(file) : "";
            }
            catch (Exception ex)
            {
                textView.Text = $"// Error reading file: {ex.Message}";
            }

            #region Find (inner)
            // Find feature (kept as in your original code)
            void Find()
            {
                var findDialog = new Dialog("Find Text", 60, 20);
                var input = new TextField("")
                {
                    X = 1,
                    Y = 1,
                    Width = 50
                };
                findDialog.Add(new Label("Search for:") { X = 1, Y = 0 });
                findDialog.Add(input);

                ListView? resultsList = null;
                List<int> matchPositions = new();

                var findButton = new Button("Find", is_default: true);
                findButton.Clicked += () =>
                {
                    string? searchTerm = input.Text?.ToString();
                    matchPositions.Clear();

                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        string content = textView.Text.ToString();
                        int idx = 0;
                        while ((idx = content.IndexOf(searchTerm, idx, StringComparison.OrdinalIgnoreCase)) != -1)
                        {
                            matchPositions.Add(idx);
                            idx += searchTerm.Length;
                        }

                        if (matchPositions.Count > 0)
                        {
                            var items = matchPositions.Select(pos =>
                            {
                                int line = content.Substring(0, pos).Count(c => c == '\n') + 1;
                                return $"Line {line}, Pos {pos}";
                            }).ToList();

                            resultsList.SetSource(items);
                        }
                        else
                        {
                            MessageBox.ErrorQuery(40, 7, "Not Found", $"'{searchTerm}' not found.", "OK");
                        }
                    }
                };

                resultsList = new ListView()
                {
                    X = 1,
                    Y = 3,
                    Width = 55,
                    Height = 10
                };
                resultsList.OpenSelectedItem += (args) =>
                {
                    int pos = matchPositions[args.Item];
                    textView.CursorPosition = new Point(pos, 0);
                    textView.ScrollTo(pos);
                    Application.RequestStop();
                };

                findDialog.Add(resultsList);
                findDialog.AddButton(findButton);
                var closeButton = new Button("Close");
                closeButton.Clicked += () => Application.RequestStop();
                findDialog.AddButton(closeButton);

                Application.Run(findDialog);
            }
            #endregion

            // Add menu bar with Save, Find, Prev, Next, Quit
            top.Add(new MenuBar(new MenuBarItem[]
            {
                new MenuBarItem("_File", new MenuItem[]
                {
                    new MenuItem("_Save", "Ctrl+S", () =>
                    {
                        try
                        {
                            File.WriteAllText(file, textView.Text.ToString());
                            MessageBox.Query(40, 7, "Saved", "File saved successfully.", "OK");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.ErrorQuery(50, 7, "Save Error", $"Failed to save: {ex.Message}", "OK");
                        }
                    }),
                    new MenuItem("_Find", "Ctrl+F", () => Find()),
                    new MenuItem("_Prev", "Ctrl+Left", () =>
                    {
                        if (projectCsFiles != null && index > 0)
                        {
                            // Save current file and schedule previous for opening
                            try { File.WriteAllText(file, textView.Text.ToString()); } catch { }
                            pendingOpenFile = projectCsFiles[index - 1];
                            pendingOpenIndex = index - 1;
                            Application.RequestStop();
                        }
                        else
                        {
                            MessageBox.Query(40, 7, "Info", "No previous file.", "OK");
                        }
                    }),
                    new MenuItem("_Next", "Ctrl+Right", () =>
                    {
                        if (projectCsFiles != null && index >= 0 && index < projectCsFiles.Count - 1)
                        {
                            // Save current file and schedule next for opening
                            try { File.WriteAllText(file, textView.Text.ToString()); } catch { }
                            pendingOpenFile = projectCsFiles[index + 1];
                            pendingOpenIndex = index + 1;
                            Application.RequestStop();
                        }
                        else
                        {
                            MessageBox.Query(40, 7, "Info", "No next file.", "OK");
                        }
                    }),
                    new MenuItem("_Quit", "Ctrl+Q", () => Application.RequestStop())
                })
            }));

            Application.Run();
            Application.Shutdown();

            // If navigation was requested, clear pending and open the next file
            if (!string.IsNullOrEmpty(pendingOpenFile))
            {
                var next = pendingOpenFile;
                var nextIndex = pendingOpenIndex;
                pendingOpenFile = null;
                pendingOpenIndex = -1;
                if (File.Exists(next))
                {
                    Edit(next, projectCsFiles, nextIndex);
                    return;
                }
                else
                {
                    Console.WriteLine($"File not found: {next}");
                }
            }
        }

        #endregion

        #region Process & UI Helpers

        static (string Stdout, string Stderr, int ExitCode) RunProcessCapture(string fileName, string arguments, string? workingDirectory = null, int timeoutMs = 120_000)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory
            };

            try
            {
                using var proc = Process.Start(psi);
                if (proc == null) return (string.Empty, "Failed to start process.", -1);

                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();

                bool exited = proc.WaitForExit(timeoutMs);
                if (!exited)
                {
                    try { proc.Kill(true); } catch { }
                    return (stdout, stderr + "\n(Process killed due to timeout)", proc.HasExited ? proc.ExitCode : -1);
                }

                return (stdout, stderr, proc.ExitCode);
            }
            catch (Exception ex)
            {
                return (string.Empty, $"Exception running process: {ex.Message}", -1);
            }
        }

        static void ShowTextOutputDialog(string title, string text, int width = 80, int height = 20)
        {
            Application.Init();
            var dlg = new Dialog(title, Math.Min(width, Console.WindowWidth - 2), Math.Min(height, Console.WindowHeight - 2));

            var tv = new TextView()
            {
                Text = text,
                ReadOnly = true,
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 2
            };
            dlg.Add(tv);

            var close = new Button("Close") { X = Pos.Center(), Y = Pos.AnchorEnd(1) };
            close.Clicked += () => Application.RequestStop();
            dlg.AddButton(close);

            Application.Run(dlg);
            Application.Shutdown();
        }

        #endregion

        #region NuGet Management

        static void ManageNuGetPackages()
        {
            // 1) Ask user to pick a folder (project root)
            Application.Init();
            var folderDialog = new OpenDialog("Select Project Folder", "Choose a folder containing .csproj")
            {
                AllowsMultipleSelection = false,
                CanChooseDirectories = true,
                CanChooseFiles = false
            };
            Application.Run(folderDialog);
            Application.Shutdown();

            if (folderDialog.FilePaths.Count == 0)
            {
                Console.WriteLine("No folder selected.");
                return;
            }

            string selectedFolder = folderDialog.FilePaths[0];

            // 2) Find .csproj files
            string[] csprojFiles;
            try
            {
                csprojFiles = Directory.GetFiles(selectedFolder, "*.csproj", SearchOption.AllDirectories)
                                       .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                                       .ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to enumerate .csproj files: {ex.Message}");
                return;
            }

            if (csprojFiles.Length == 0)
            {
                Console.WriteLine("No .csproj files found under selected folder.");
                return;
            }

            // 3) If multiple projects, let user pick; if single, use it
            string chosenProject;
            if (csprojFiles.Length == 1)
            {
                chosenProject = csprojFiles[0];
            }
            else
            {
                var items = csprojFiles.Select(p => Path.GetRelativePath(selectedFolder, p)).ToArray();

                Application.Init();
                var dlg = new Dialog("Choose Project", 80, 20);
                var listView = new ListView(items) { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() - 2 };
                dlg.Add(listView);

                var ok = new Button("Open") { X = Pos.Center() - 10, Y = Pos.AnchorEnd(1) };
                ok.Clicked += () => Application.RequestStop();
                dlg.AddButton(ok);

                var cancel = new Button("Cancel") { X = Pos.Center() + 2, Y = Pos.AnchorEnd(1) };
                cancel.Clicked += () =>
                {
                    listView.SelectedItem = -1;
                    Application.RequestStop();
                };
                dlg.AddButton(cancel);

                Application.Run(dlg);
                Application.Shutdown();

                int sel = listView.SelectedItem;
                if (sel < 0 || sel >= csprojFiles.Length)
                {
                    Console.WriteLine("No project selected.");
                    return;
                }
                chosenProject = csprojFiles[sel];
            }

            // 4) Present a small menu: List, Add, Remove, Back
            while (true)
            {
                Console.WriteLine($"\nManaging packages for: {chosenProject}");
                Console.WriteLine("Choose an action:");
                Console.WriteLine("1. List installed packages");
                Console.WriteLine("2. Add package");
                Console.WriteLine("3. Remove package");
                Console.WriteLine("4. Back");

                Console.Write("Your choice: ");
                string? opt = Console.ReadLine();
                if (opt == "1")
                {
                    // dotnet list <proj> package
                    var (outp, err, code) = RunProcessCapture("dotnet", $"list {QuotePath(chosenProject)} package", Path.GetDirectoryName(chosenProject));
                    string full = $"Exit code: {code}\n\nSTDOUT:\n{outp}\n\nSTDERR:\n{err}";
                    ShowTextOutputDialog("Installed Packages", full, 100, 30);
                }
                else if (opt == "2")
                {
                    // Prompt for package name and optional version
                    Console.Write("Package id (e.g. Newtonsoft.Json): ");
                    string? pkg = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(pkg))
                    {
                        Console.WriteLine("No package id provided.");
                        continue;
                    }
                    Console.Write("Version (leave empty for latest): ");
                    string? version = Console.ReadLine();

                    string args = $"add {QuotePath(chosenProject)} package {pkg}";
                    if (!string.IsNullOrWhiteSpace(version))
                    {
                        args += $" --version {version}";
                    }

                    Console.WriteLine($"Running: dotnet {args}");
                    var (outp, err, code) = RunProcessCapture("dotnet", args, Path.GetDirectoryName(chosenProject));
                    string full = $"Exit code: {code}\n\nSTDOUT:\n{outp}\n\nSTDERR:\n{err}";
                    ShowTextOutputDialog("Add Package Result", full, 100, 20);
                }
                else if (opt == "3")
                {
                    Console.Write("Package id to remove (e.g. Newtonsoft.Json): ");
                    string? pkg = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(pkg))
                    {
                        Console.WriteLine("No package id provided.");
                        continue;
                    }

                    string args = $"remove {QuotePath(chosenProject)} package {pkg}";
                    Console.WriteLine($"Running: dotnet {args}");
                    var (outp, err, code) = RunProcessCapture("dotnet", args, Path.GetDirectoryName(chosenProject));
                    string full = $"Exit code: {code}\n\nSTDOUT:\n{outp}\n\nSTDERR:\n{err}";
                    ShowTextOutputDialog("Remove Package Result", full, 100, 20);
                }
                else if (opt == "4")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid option.");
                }
            }
        }

        #endregion

        #region Utilities

        // Helper to quote a path if it contains spaces (used when passing a project path)
        static string QuotePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "\"\"";
            if (path.Contains(" ")) return $"\"{path}\"";
            return path;
        }

        static void OpenUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                Console.WriteLine("No URL provided.");
                return;
            }

            // Basic validation
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                Console.WriteLine("Not a valid HTTP/HTTPS URL.");
                return;
            }

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // On .NET Core/5+ this is the recommended way on Windows
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // xdg-open opens the URL in the default browser on most Linux distros
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // open uses the default handler on macOS
                    Process.Start("open", url);
                }
                else
                {
                    // Fallback: try UseShellExecute
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open URL: {ex.Message}");
            }
        }

        static void ShowHelp()
        {
            // Prefer showing README.md if it exists in the current working directory
            string readmePath = Path.Combine(Environment.CurrentDirectory, "README.md");
            string helpText;

            if (File.Exists(readmePath))
            {
                try
                {
                    helpText = File.ReadAllText(readmePath);
                }
                catch (Exception ex)
                {
                    helpText = $"Failed to read README.md: {ex.Message}\n\nFalling back to built-in help.\n\n";
                }
            }
            else
            {
                helpText = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(helpText))
            {
                // Fallback built-in help summary
                helpText = @"CS-Hub - Help (v0.0.2)

Main menu:
  1  Edit A File         - Open a file in the built-in editor.
  2  Create A File       - Create a new empty file.
  3  Exit                - Quit the app.
  4  Create a new C# Project - Uses 'dotnet new' to scaffold projects.
  5  Open a C# Project   - Select a folder, open any .cs files found (recursive).
  6  Manage NuGet Packages - List, add, remove NuGet packages via the dotnet CLI.
  7  View Documentation  - Opens the README on GitHub (if wired to OpenUrl).
 10  Help                - Show this help screen.
 12  Github page         - Open project GitHub page in default browser.

Editor controls:
  - Save: File -> Save (or Save menu item)
  - Find: File -> Find (search and jump to results)
  - Prev/Next: File -> Prev / Next (or Ctrl+Left / Ctrl+Right) to hop between .cs files in the project list (auto-saves on hop)
  - Quit editor: File -> Quit

Notes:
  - .NET SDK (dotnet) must be installed and on PATH for project creation and NuGet management.
  - Add/Remove NuGet currently prompts on the console; results are shown in dialogs.
  - If you'd like the add/remove prompts converted to Terminal.Gui dialogs, or if you want package lists shown as selectable lists, ask and I will patch it.

If you need more detail for any item, pick a menu number and I can expand the help for it.";
            }

            ShowTextOutputDialog("Help - CS-Hub", helpText, 100, 30);
        }

        #endregion
    }
}