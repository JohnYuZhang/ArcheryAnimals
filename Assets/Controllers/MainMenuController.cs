using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public TMP_InputField NameInput;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    void LaunchClient()
    {
        var username = string.IsNullOrWhiteSpace(NameInput.text) ? "Guest" : NameInput.text;
        SceneManager.LoadScene("ClientGameScene", LoadSceneMode.Single);
    }

    void LaunchServer()
    {
        SceneManager.LoadScene("ServerGameScene", LoadSceneMode.Single);
    }
}
