using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIAnimationController : MonoBehaviour
{
    [SerializeField] float delay = 0;
    [SerializeField] float sceneSwitchDelay = 0;
    [SerializeField] float animationsDelay = 1f;

    bool switchedScene = false;
    [SerializeField] bool playClicked = false;
    [SerializeField]  bool clickable = false;
    [SerializeField] bool leftMainMenu = false;

    [SerializeField] GameObject titleParent = null;
    [SerializeField] GameObject title = null;
    [SerializeField] GameObject playButton = null;
    [SerializeField] GameObject playButtonParent = null;
    [SerializeField] GameObject optionsButton = null;
    [SerializeField] GameObject optionsButtonParent = null;
    [SerializeField] GameObject exitButton = null;
    [SerializeField] GameObject exitButtonParent = null;
    [SerializeField] GameObject singlePlayerButton = null;
    [SerializeField] GameObject multiPlayerButton = null;
    [SerializeField] GameObject escButton = null;
    [SerializeField] GameObject blackScreen = null;

    void Update()
    {
        if(delay > 0)
        {
            delay -= Time.deltaTime;
        }

        if(animationsDelay > 0)
        {
            animationsDelay -= Time.deltaTime;
        }

        if(animationsDelay <= 0)
        {
            clickable = true;
        }

        if (switchedScene)
        {
            if (sceneSwitchDelay > 0)
            {
                sceneSwitchDelay -= Time.deltaTime;
            }
        }

        if(sceneSwitchDelay <= 0)
        {
            SceneManager.LoadScene("TestScene1");
        }

        if(delay <= 0)
        {
            title.GetComponent<Animator>().enabled = true;
            playButton.GetComponent<Animator>().enabled = true;
            optionsButton.GetComponent<Animator>().enabled = true;
            exitButton.GetComponent<Animator>().enabled = true;
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ExitMenu();
        }
    }

    public void Play()
    {
        if(clickable)
        {
            playButtonParent.GetComponent<Animator>().SetTrigger("down");
            optionsButtonParent.GetComponent<Animator>().SetTrigger("down");
            exitButtonParent.GetComponent<Animator>().SetTrigger("down");
            singlePlayerButton.GetComponent<Animator>().SetTrigger("down");
            multiPlayerButton.GetComponent<Animator>().SetTrigger("down");
            escButton.GetComponent<Animator>().SetTrigger("down");

            playClicked = true;
            leftMainMenu = true;
            animationsDelay = 1;
            clickable = false;
        }
    }

    public void ExitMenu()
    {
        if (clickable && leftMainMenu)
        {
            playButtonParent.GetComponent<Animator>().SetTrigger("up");
            optionsButtonParent.GetComponent<Animator>().SetTrigger("up");
            exitButtonParent.GetComponent<Animator>().SetTrigger("up");

            if(playClicked)
            {
                singlePlayerButton.GetComponent<Animator>().SetTrigger("up");
                multiPlayerButton.GetComponent<Animator>().SetTrigger("up");
                playClicked = false;
            }

            escButton.GetComponent<Animator>().SetTrigger("up");
            animationsDelay = 1f;
            clickable = false;
            leftMainMenu = false;
        }
    }

    public void Options()
    {
        if (clickable)
        {
            playButtonParent.GetComponent<Animator>().SetTrigger("down");
            optionsButtonParent.GetComponent<Animator>().SetTrigger("down");
            exitButtonParent.GetComponent<Animator>().SetTrigger("down");
            escButton.GetComponent<Animator>().SetTrigger("down");

            leftMainMenu = true;
            animationsDelay = 1;
            clickable = false;
        }
    }

    public void SwitchScene()
    {
        titleParent.GetComponent<Animator>().enabled = true;
        blackScreen.GetComponent<Animator>().SetTrigger("close");
        switchedScene = true;
        animationsDelay = 1;
        clickable = false;
    }
}
