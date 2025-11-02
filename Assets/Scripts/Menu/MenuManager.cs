using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject guidePanel;
    public void Awake()
    {
        guidePanel.SetActive(false);
    }
    /*public void NewProjectPanel()
    {
        SceneManager.LoadScene("MainScene");
    }*/
    public void OpenProjectPanel()
    {
        SceneManager.LoadScene("MainScene");
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
        // Future implementation for about menu
    }
    public void Exit()
    {
        Application.Quit();
    }
    public void CloseGuidePanel()
    {
        guidePanel.SetActive(false);
    }
    public void OpenGithub()
    {
        Application.OpenURL("https://github.com/hanimuhamed/Blogic");
    }
    public void OpenTutorial()
    {
        SceneManager.LoadScene("TutorialScene");
    }
}
