using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject guidePanel;
    public GameObject projectPanel;
    public GameObject aboutPanel;
    public void Awake()
    {
        guidePanel.SetActive(false);
        projectPanel.SetActive(false);
        aboutPanel.SetActive(false);
    }
    /*public void NewProjectPanel()
    {
        SceneManager.LoadScene("MainScene");
    }*/
    public void OpenProjectPanel()
    {
        projectPanel.SetActive(true);
    }
    /*public void SettingsPanel()
    {
        // Future implementation for settings menu
    }*/
    public void OpenGuidePanel()
    {
        guidePanel.SetActive(true);
    }
    public void AboutPanel()
    {
        aboutPanel.SetActive(true);
    }
    public void CloseAboutPanel()
    {
        aboutPanel.SetActive(false);
    }
    public void Exit()
    {
        Application.Quit();
    }
    public void CloseProjectPanel()
    {
        projectPanel.SetActive(false);
        //hello
    }
    public void CloseGuidePanel()
    {
        guidePanel.SetActive(false);
    }
    public void OpenGithub()
    {
        Application.OpenURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
    }
    public void OpenTutorial()
    {
        SceneManager.LoadScene("TutorialScene");
    }
}
