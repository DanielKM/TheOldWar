using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenu;
    UIController UI;
    // public GameObject pauseMenuUI;
    // Start is called before the first frame update
    void Start()
    {
        UI = gameObject.GetComponent<UIController>();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F10))
        {
            if(GameIsPaused) 
            {
                Resume();
            } else 
            {
                Pause();
            }
        }
    }

    // Update is called once per frame
    public void Resume()
    {
        pauseMenu.SetActive(false);
        GameIsPaused = false;
        // Time.timeScale = 1f;
    }

    // Update is called once per frame
    public void Pause()
    {
        pauseMenu.SetActive(true);
        GameIsPaused = true;
        // Time.timeScale = 0f;
    }
    
    public void Quit()
    {
        Application.Quit();
    }
}
