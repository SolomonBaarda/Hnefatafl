﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class HUD : MonoBehaviour
{
    public TMP_Text resetButton, quitButton;

    public static event Action OnGameQuit;
    public static event Action OnGameReset;

    private void Start()
    {
        // Add the GameOver method to the OnGameOver event call
        BoardManager.OnGameOver += GameOver;
        // Add the turn indicator
        BoardManager.OnTurnStart += UpdatePlayerTurnDisplay;
        // Add the reset game method to the event
        OnGameReset += BoardManager.Instance.ResetGame;
        OnGameReset += ResetHud;
        // Add the quit to menu method to the event
        OnGameQuit += QuitToMenu;

        // Set up buttons
        SetButton(resetButton, "RESET", Color.white, Color.black, Color.gray, Color.black);
        SetButton(quitButton, "X", Color.white, Color.black, Color.gray, Color.black);
        // Add listeners
        resetButton.GetComponent<Button>().onClick.AddListener(() => OnResetGameClicked());
        quitButton.GetComponent<Button>().onClick.AddListener(() => OnQuitToMenuClicked());

        // Hide the win text
        GameObject.FindGameObjectWithTag("HUDGameOverDisplay").GetComponent<TMP_Text>().enabled = false;
    }

    public void ResetHud()
    {
        BoardManager.Team t;
        if(BoardManager.Instance.isAttackingTurn)
        {
            t = BoardManager.Team.Attacking;
        }
        else
        {
            t = BoardManager.Team.Defending;
        }
        UpdatePlayerTurnDisplay(t);
    }

    public void OnQuitToMenuClicked()
    {
        OnGameQuit.Invoke();
    }

    public void OnResetGameClicked()
    {
        OnGameReset.Invoke();
    }

    private void QuitToMenu()
    {
        UnloadHUD();

        // Quit to menu
        SceneManager.LoadScene(0);
    }

    private void OnDestroy()
    {
        UnloadHUD();
    }

    public void UnloadHUD()
    {
        if (SceneManager.GetSceneByName("HUD").isLoaded)
        {
            // Remove all event calls
            BoardManager.OnGameOver -= GameOver;
            OnGameReset -= BoardManager.Instance.ResetGame;
            OnGameReset -= ResetHud;
            OnGameQuit -= QuitToMenu;
            resetButton.GetComponent<Button>().onClick.RemoveAllListeners();
            quitButton.GetComponent<Button>().onClick.RemoveAllListeners();

            // Unload this scene
            SceneManager.UnloadSceneAsync("HUD");
        }
    }

    private void SetButton(TMP_Text t, String message, Color colour, Color outline, Color hover, Color pressed)
    {
        SetText(t, message, colour, outline);

        Button b = t.GetComponent<Button>();
        if (b)
        {
            // Set the colours here
            ColorBlock c = ColorBlock.defaultColorBlock;
            c.normalColor = colour;
            c.highlightedColor = hover;
            c.pressedColor = pressed;
            b.colors = c;
        }
    }

    private void SetText(TMP_Text t, String message, Color c, Color outline)
    {
        // Set the properties and display
        t.text = message;
        t.color = c;
        t.outlineWidth = 0.25f;
        t.outlineColor = outline;
        t.enabled = true;
    }


    private void UpdatePlayerTurnDisplay(BoardManager.Team t)
    {
        String team;
        Color colour, outline;
        if (t.Equals(BoardManager.Team.Attacking))
        {
            team = "Attackers";
            colour = Color.white;
            outline = Color.black;
        }
        else
        {
            team = "Defenders";
            colour = Color.black;
            outline = Color.white;
        }

        SetText(GameObject.FindGameObjectWithTag("HUDTurnDisplay").GetComponent<TMP_Text>(), team + " turn", colour, outline);
    }

    private void GameOver(BoardManager.Team team)
    {
        Debug.Log("OnGameOver called with team " + team);
        if(team.Equals(BoardManager.Team.Attacking))
        {
            StartCoroutine(DisplayWon("Attackers", Color.white, Color.black));
        }
        else
        {
            StartCoroutine(DisplayWon("Defenders", Color.black, Color.white));
        }
    }

    private IEnumerator DisplayWon(string team, Color colour, Color outline)
    {
        SetText(GameObject.FindGameObjectWithTag("HUDGameOverDisplay").GetComponent<TMP_Text>(), team + " have won!", colour, outline);

        // Wait 2 seconds then disable the text
        yield return new WaitForSeconds(2);
        GameObject.FindGameObjectWithTag("HUDGameOverDisplay").GetComponent<TMP_Text>().enabled = false;
    }

}
