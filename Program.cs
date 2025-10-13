using System;
using System.IO;
using Terminal.Gui;

namespace Gohub
{
    class Program
    {
        static string? selectedFile;

        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                Edit(args[0]);
                return;
            }

            Console.WriteLine("Welcome to Gohub!");
            Menu();
        }




        static void Exit()
        {
            Application.Shutdown();
            Environment.Exit(0);
        }

        static void Menu()
        {
            Console.WriteLine("\nChoose an option to continue:");
            Console.WriteLine("1. Edit A File");
            Console.WriteLine("2. Create A File");
            Console.WriteLine("3. Exit");
            Console.WriteLine("4. Create A Go Project");
            Console.WriteLine("5. Open A Go Project");

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
                CreateGoProject();
                Menu();
            }
            else if (option == "5")
            {
                OpenGoProject();
                Menu();
            }
            else
            {
                Console.WriteLine("Invalid option. Please try again.");
                Menu();
            }
        }

        #region Open Go Project
            
        static void OpenGoProject()
        {
            Application.Init();

            var folderDialog = new OpenDialog("Open Go Project", "Select the root folder of a Go project")
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

            string projectPath = folderDialog.FilePaths[0];
            string goModPath = Path.Combine(projectPath, "go.mod");

            if (!File.Exists(goModPath))
            {
                Console.WriteLine("This folder does not contain a go.mod file. Not a valid Go project.");
                return;
            }

            string mainGoPath = Path.Combine(projectPath, "main.go");

            if (File.Exists(mainGoPath))
            {
                Console.WriteLine("Opening main.go...");
                Edit(mainGoPath);
            }
            else
            {
                Console.WriteLine("main.go not found. Let’s pick a .go file to open.");
                Application.Init();

                var fileDialog = new OpenDialog("Select a Go File", "Choose a .go file to open")
                {
                    AllowsMultipleSelection = false,
                    CanChooseDirectories = false,
                    AllowedFileTypes = new string[] { ".go" },
                    DirectoryPath = projectPath
                };

                Application.Run(fileDialog);
                Application.Shutdown();

                if (fileDialog.FilePaths.Count > 0)
                {
                    Edit(fileDialog.FilePaths[0]);
                }
                else
                {
                    Console.WriteLine("No file selected.");
                }
            }
        }
        #endregion

        #region Create Go Project

        static void CreateGoProject()
        {
            Application.Init();

            var folderDialog = new OpenDialog("Select Parent Folder", "Choose where to create the Go project")
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

            string parentFolder = folderDialog.FilePaths[0];

            var nameDialog = new Dialog("New Go Project", 60, 10);
            var input = new TextField("")
            {
                X = 1,
                Y = 1,
                Width = 50
            };
            nameDialog.Add(new Label("Project name:") { X = 1, Y = 0 });
            nameDialog.Add(input);

            string? projectName = null;
            var ok = new Button("Create", is_default: true);
            ok.Clicked += () =>
            {
                projectName = input.Text?.ToString();
                Application.RequestStop();
            };
            nameDialog.AddButton(ok);
            var cancelBtn = new Button("Cancel");
            cancelBtn.Clicked += () => Application.RequestStop();
            nameDialog.AddButton(cancelBtn);

            Application.Run(nameDialog);
            Application.Shutdown();

            if (!string.IsNullOrWhiteSpace(projectName))
            {
                string projectPath = Path.Combine(parentFolder, projectName);
                try
                {
                    Directory.CreateDirectory(projectPath);

                    string goMod = $"module {projectName}\n\ngo 1.21\n";
                    File.WriteAllText(Path.Combine(projectPath, "go.mod"), goMod);

                    string mainGo = @"package main

import ""fmt""

func main() {
    fmt.Println(""Hello from GoHub!"")
}";
                    string mainGoPath = Path.Combine(projectPath, "main.go");
                    File.WriteAllText(mainGoPath, mainGo);

                    Console.WriteLine($"Go project created at: {projectPath}");
                    Edit(mainGoPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating Go project: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("No project name provided.");
            }
        }


        #endregion


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

        static void Edit(string file)
{
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
    if (File.Exists(file))
    {
        textView.Text = File.ReadAllText(file);
    }
    else
    {
        textView.Text = "";
    }

    // Find feature
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
            int index = 0;
            while ((index = content.IndexOf(searchTerm, index, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                matchPositions.Add(index);
                index += searchTerm.Length;
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

    // Menu bar with Save, Find, and Quit
    top.Add(new MenuBar(new MenuBarItem[]
    {
        new MenuBarItem("_File", new MenuItem[]
        {
            new MenuItem("_Save", "Ctrl+S", () =>
            {
                File.WriteAllText(file, textView.Text.ToString());
                MessageBox.Query(40, 7, "Saved", "File saved successfully.", "OK");
            }),
            new MenuItem("_Find", "Ctrl+F", () => Find()),
            new MenuItem("_Quit", "Ctrl+Q", () => Application.RequestStop())
        })
    }));

    Application.Run();
    Application.Shutdown();
}
    }
}