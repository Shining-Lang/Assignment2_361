using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public GameObject game_MenuPanel;
    public GameObject HelpPanel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

     public void StartGame(){
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }

    public void BackMenu(){
        HelpPanel.SetActive(false);
    }

    public void HelpMenu(){
        HelpPanel.SetActive(true);
    }

    public void Exit(){
         #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
}
