using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private Button PauseMainMenu;
    [SerializeField] private Button FailMainMenu;

    void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        Debug.Log("Returned to Menu");
    }

    void Start()
    {
        PauseMainMenu.onClick.AddListener(LoadMainMenu);
        FailMainMenu.onClick.AddListener(LoadMainMenu);
    }
}
