using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;

public class ProjectManager : MonoBehaviour
{
    public GameObject projectPrefab;
    public static GameObject currentProject;
    public static string currentProjectName;
    public string newProjectName;
    public GameObject projectContent;
    private string basePath;
    public GameObject newProjectPrompt;
    public GameObject deleteProjectPrompt;
    void Awake()
    {
        newProjectPrompt.SetActive(false);
        deleteProjectPrompt.SetActive(false);
        // Example: C:\Users\<User>\AppData\LocalLow\CompanyName\GameName
        basePath = Path.Combine(Application.persistentDataPath, "Projects");

        // Ensure the Projects folder exists
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
            Debug.Log("Created folder: " + basePath);
        }
        LoadAllProjects();
    }
    public bool CreateProjectFolder()
    {
        string projectPath = Path.Combine(basePath, newProjectName);
        string savePath = Path.Combine(projectPath, "Save");
        string metaPath = Path.Combine(projectPath, "Meta");

        if (!Directory.Exists(projectPath))
        {
            // Create main project directory
            Directory.CreateDirectory(projectPath);

            // Create subdirectories
            Directory.CreateDirectory(savePath);
            Directory.CreateDirectory(metaPath);

            /*Debug.Log($"Created new project folder: {projectPath}");
            Debug.Log($" ├── Data folder: {savePath}");
            Debug.Log($" └── Meta folder: {metaPath}");*/

            return true;
        }
        else
        {
            Debug.LogWarning($"Project folder already exists: {projectPath}");
            return false;
        }
    }

    public List<string> GetAllProjects()
    {
        List<string> projectNames = new List<string>();

        if (Directory.Exists(basePath))
        {
            string[] directories = Directory.GetDirectories(basePath);
            foreach (string dir in directories)
            {
                projectNames.Add(Path.GetFileName(dir));
            }
        }

        return projectNames;
    }
    public void LoadAllProjects()
    {
        List<string> projectNames = GetAllProjects();
        foreach (string projectName in projectNames)
        {
            GameObject project = Instantiate(projectPrefab, projectContent.transform);
            project.GetComponent<Project>().Init(projectName);
            currentProject = project;
        }
    }
    public void NewProject()
    {
        newProjectPrompt.SetActive(true);
    }
    public void CreateProject()
    {
        if (newProjectName.Trim() == "")
        {
            Debug.Log("Project name cannot be empty.");
            return;
        }
        bool folderExists = CreateProjectFolder();
        if (!folderExists)
        {
            Debug.Log("Folder already exists.");
            return;
        }
        GameObject project = Instantiate(projectPrefab, projectContent.transform);
        project.GetComponent<Project>().Init(newProjectName);
        CancelPrompt();
    }
    public void SetNewProjectName(string projectName)
    {
        newProjectName = projectName;
    }
    public void DeleteProject()
    {
        deleteProjectPrompt.SetActive(true);
        TextMeshProUGUI prompt = deleteProjectPrompt.GetComponentInChildren<TextMeshProUGUI>();
        prompt.text = $"Are you sure you want to delete the project '{currentProject.GetComponent<Project>().projectName}'?";
    }
    public void ConfirmDelete()
    {
        if (currentProject != null)
        {
            string projectName = currentProject.GetComponent<Project>().projectName;
            string projectPath = Path.Combine(basePath, projectName);

            if (Directory.Exists(projectPath))
            {
                Directory.Delete(projectPath, true); // true = delete contents too
                Debug.Log("Deleted project folder: " + projectPath);
            }
            else
            {
                Debug.LogWarning("Project folder not found: " + projectPath);
            }
            Destroy(currentProject);
            CancelPrompt();
        }
    }
    public void CancelPrompt()
    {
        newProjectPrompt.SetActive(false);
        deleteProjectPrompt.SetActive(false);
    }
    public void OpenProject()
    {
        if (currentProject != null)
        {
            currentProjectName = currentProject.GetComponent<Project>().projectName;
            SceneManager.LoadScene("MainScene");
        }
    }
    public bool IsValidProjectName(string projectName)
    {
        //should only contain letters, numbers, underscores, and spaces
        foreach (char c in projectName)
        {
            if (!char.IsLetterOrDigit(c) && c != '_' && c != ' ')
            {
                return false;
            }
        }
        return true;
    }
}
