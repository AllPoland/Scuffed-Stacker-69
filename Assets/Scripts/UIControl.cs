using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIControl : MonoBehaviour
{
    [SerializeField] private Button StartButton;
    [SerializeField] private Button OptionsButton;
    [SerializeField] private Button HelpButton;
    [SerializeField] private Button CreditsButton;
    [SerializeField] private Button QuitButton;
    [SerializeField] private Button HelpBack;
    [SerializeField] private Button CreditsBack;
    [SerializeField] private Button GitHubButton;
    [SerializeField] private Button YoutubeButton;
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject OptionsMenu;
    [SerializeField] private GameObject HelpMenu;
    [SerializeField] private GameObject CreditsMenu;

    void LoadGame() //Load the game screen when the play button is pressed
    {
        SceneManager.LoadScene("GameBoard", LoadSceneMode.Single);
        Debug.Log("Game Started!");
    }

    void ExitGame()
    {
        Application.Quit();
        Debug.Log("Closing Game");
    }

    void OpenOptions() //Enable or disable the options menu when the screen change button is pressed
    {
        OptionsMenu.SetActive(true);
        MainMenu.SetActive(false);
    }

    void ToggleHelp()
    {
        bool helpOpen = HelpMenu.activeInHierarchy;
        HelpMenu.SetActive(!helpOpen);
        MainMenu.SetActive(helpOpen);
    }

    void ToggleCredits()
    {
        bool creditsOpen = CreditsMenu.activeInHierarchy;
        CreditsMenu.SetActive(!creditsOpen);
        MainMenu.SetActive(creditsOpen);
    }

    void OpenGitHub()
    {
        Application.OpenURL("https://github.com/AllPoland");
    }

    void OpenYoutube()
    {
        //Link to Leo Music youtube channel
        Application.OpenURL("https://www.youtube.com/channel/UCdtxWg3YBpokI9lhOY5JaCQ");
    }

    void Start()
    {
        //Subscribe to all the buttons
        StartButton.onClick.AddListener(LoadGame);
        QuitButton.onClick.AddListener(ExitGame);
        OptionsButton.onClick.AddListener(OpenOptions);
        HelpButton.onClick.AddListener(ToggleHelp);
        HelpBack.onClick.AddListener(ToggleHelp);
        CreditsButton.onClick.AddListener(ToggleCredits);
        CreditsBack.onClick.AddListener(ToggleCredits);
        GitHubButton.onClick.AddListener(OpenGitHub);
        YoutubeButton.onClick.AddListener(OpenYoutube);
    }
}
