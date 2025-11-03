using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Project : MonoBehaviour
{
    private static int idCounter = 0;
    public int projectID;
    public string projectName;
    private Button button;
    private ProjectManager projectManager;
    public void Awake()
    {
        projectManager = FindFirstObjectByType<ProjectManager>();       
    }
    public void Init(string projectName)
    {
        this.projectName = projectName;
        projectID = idCounter++;
        button = GetComponent<Button>();
        button.GetComponentInChildren<TextMeshProUGUI>().text = projectName;
        button.onClick.AddListener(() => SelectProject());
    }
    public void SelectProject()
    {
        ProjectManager.currentProject = this.gameObject;
    }
    
}
