using Godot;
using RedotUtils;
using System;
using System.Collections.Generic;
using System.IO;

namespace Template.Setup;

public static class SetupManager
{
    /// <summary> 
    /// Moves game assets specific to the selected genre to more accessible locations, 
    /// sets the main project scene, and removes any unnecessary files or folders. 
    /// For example, if the 3D FPS genre was chosen, this method would move the level_3D.tscn 
    /// file to res://Scenes, set it as the new main project scene and remove any other genre 
    /// assets (such as 2D Platformer or 2D Top Down). 
    /// </summary>
    public static void MoveProjectFiles(Genre genre, string pathFrom, string pathTo, bool deleteOtherGenres)
    {
        // Gets the name of the main scene file based on the current genre
        Dictionary<Genre, string> folderNames = SetupUtils.FolderNames;
        string mainSceneName = GetMainSceneName(Path.Combine(pathFrom, folderNames[genre]));

        // Moves the main scene file from its original location to a new location
        File.Move(
            Path.Combine(pathFrom, folderNames[genre], mainSceneName),
            Path.Combine(pathTo, "Scenes", mainSceneName));

        // Move all files relevant to this genre
        MoveFilesAndPreserveFolderStructure(Path.Combine(pathFrom, folderNames[genre]), Path.Combine("Genres", folderNames[genre]));

        if (deleteOtherGenres)
        {
            // If a directory is not associated with the current genre delete that folder
            foreach (KeyValuePair<Genre, string> folder in folderNames)
            {
                if (folder.Key != genre)
                {
                    Directory.Delete(Path.Combine(pathFrom, folder.Value), true);
                }
            }
        }

        // Sets the main scene for the project
        SetMainScene(pathTo, mainSceneName);

        // Deletes any empty directories that were previously moved
        DeleteDirectoryIfEmpty(pathFrom);
    }

    private static void SetMainScene(string path, string scene)
    {
        string text = File.ReadAllText(Path.Combine(path, "project.godot"));

        text = text.Replace(
            "run/main_scene=\"res://Genres/0 Setup/SetupUI.tscn\"",
           $"run/main_scene=\"res://Scenes/{scene}\"");

        File.WriteAllText(Path.Combine(path, "project.godot"), text);
    }

    private static string GetMainSceneName(string fullPath)
    {
        // Get all .tscn files in the directory
        string[] sceneFiles = Directory.GetFiles(fullPath, "*.tscn");

        // Check the number of .tscn files found
        if (sceneFiles.Length == 0)
        {
            throw new FileNotFoundException("No .tscn files found in the directory.");
        }
        else if (sceneFiles.Length > 1)
        {
            throw new InvalidOperationException("There can only be one main scene (.tscn file) in the root directory.");
        }

        // If there is exactly one .tscn file, return its name with the extension
        string sceneName = Path.GetFileName(sceneFiles[0]);

        return sceneName;
    }

    private static void MoveFilesAndPreserveFolderStructure(string path, string folder)
    {
        // Move all assets to res://
        DirectoryUtils.Traverse(path, MoveFile);

        void MoveFile(string fullFilePath)
        {
            string newPath = DirectoryUtils.RemovePathSegment(fullFilePath, folder);

            new FileInfo(newPath).Directory.Create();

            try
            {
                File.Move(fullFilePath, newPath);
            }
            catch (IOException)
            {
                GD.Print($"Failed to move {fullFilePath.GetFile()}");
            }
        }
    }

    private static void DeleteDirectoryIfEmpty(string path)
    {
        if (Directory.Exists(path))
        {
            DirectoryUtils.DeleteEmptyDirectories(path);
            DirectoryUtils.DeleteEmptyDirectory(path);
        }
    }

    /// <summary>
    /// Replaces all instances of the keyword "Template" with the new
    /// specified game name in several project files.
    /// </summary>
    public static void RenameProjectFiles(string path, string name)
    {
        RenameCSProjFile(path, name);
        RenameSolutionFile(path, name);
        RenameProjectGodotFile(path, name);
    }
    
    public static void SetupVSCodeTemplates(string redotExe, string gameName)
    {
        // Normalize the redot.exe path and remove quotes
        redotExe = redotExe.Trim().Replace("\\", "/").Replace("\"", "");
        
        string path = ProjectSettings.GlobalizePath("res://");
        string fullPath = Path.Combine(path, "Genres", "0 Setup", "VSCode Templates");
        string launchFilePath = Path.Combine(fullPath, "launch.json");
        string tasksFilePath = Path.Combine(fullPath, "tasks.json");

        string launchText = File.ReadAllText(launchFilePath);
        launchText = launchText.Replace("ENGINE_EXE", redotExe);
        string launchVSCodePath = Path.Combine(path, ".vscode", "launch.json");
        File.WriteAllText(launchVSCodePath, launchText);

        string tasksText = File.ReadAllText(tasksFilePath);
        tasksText = tasksText.Replace("ENGINE_EXE", redotExe);
        tasksText = tasksText.Replace("Template", gameName);
        string tasksVSCodePath = Path.Combine(path, ".vscode", "tasks.json");
        File.WriteAllText(tasksVSCodePath, tasksText);
    }
    
    private static void RenameProjectGodotFile(string path, string name)
    {
        string fullPath = Path.Combine(path, "project.godot");
        string text = File.ReadAllText(fullPath);

        text = text.Replace(
            "project/assembly_name=\"Template\"",
            $"project/assembly_name=\"{name}\"");

        text = text.Replace(
            "config/name=\"Template\"",
            $"config/name=\"{name}\""
            );

        File.WriteAllText(fullPath, text);
    }
    
    private static void RenameSolutionFile(string path, string name)
    {
        string fullPath = Path.Combine(path, "Template.sln");
        string text = File.ReadAllText(fullPath);
        text = text.Replace("Template", name);
        File.Delete(fullPath);
        File.WriteAllText(Path.Combine(path, name + ".sln"), text);
    }
    
    private static void RenameCSProjFile(string path, string name)
    {
        string fullPath = Path.Combine(path, "Template.csproj");
        string text = File.ReadAllText(fullPath);
        text = text.Replace("<RootNamespace>Template</RootNamespace>", $"<RootNamespace>{name}</RootNamespace>");
        File.Delete(fullPath);
        File.WriteAllText(Path.Combine(path, name + ".csproj"), text);
    }

    /// <summary>
    /// Renames the default "Template" namespace to the new specified game
    /// name in all scripts.
    /// </summary>
    public static void RenameAllNamespaces(string path, string name)
    {
        DirectoryUtils.Traverse(path, RenameNamespaces);

        void RenameNamespaces(string fullFilePath)
        {
            // Ignore these directories
            switch (Path.GetDirectoryName(fullFilePath))
            {
                case ".godot":
                case "RedotUtils":
                case "addons":
                    return;
            }

            // Modify all scripts
            if (fullFilePath.EndsWith(".cs"))
            {
                // Do not modify this script
                if (!fullFilePath.EndsWith("Setup.cs"))
                {
                    string text = File.ReadAllText(fullFilePath);
                    text = text.Replace("namespace Template", $"namespace {name}");
                    text = text.Replace("using Template", $"using {name}");
                    text = text.Replace("Template.", $"{name}.");
                    File.WriteAllText(fullFilePath, text);
                }
            }
        }
    }
}

public enum Genre
{
    None,
    Platformer2D,
    TopDown2D,
    FPS3D
}
