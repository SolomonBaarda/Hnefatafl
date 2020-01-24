using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class HUD : MonoBehaviour
{
    public TMP_Text winMessage, turnDisplay;
    public TMP_Text resetButton, quitButton;


    public static event Action OnGameQuit;
    public static event Action OnGameReset;

    private void Start()
    {
        // Hide the win text
        winMessage.enabled = false;

        // Add the GameOver method to the OnGameOver event call
        BoardManager.OnGameOver += GameOver;
        // Add the turn indicator
        BoardManager.OnTurnStart += UpdatePlayerTurnDisplay;
        // Add the reset game method to the event
        OnGameReset += BoardManager.Instance.ResetGame;
        // Add the quit to menu method to the event
        OnGameQuit += QuitToMenu;

        SetButton(resetButton, "RESET", Color.white, Color.black, Color.gray, Color.black);
        //SetButton(quitButton, "X", Color.white, Color.black, Color.gray, Color.black);
    }

    public void OnQuitToMenuClicked()
    {
        // Invoke event, currently unused
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
            Debug.Log("HUD Unloaded");

            // Remove all event calls
            BoardManager.OnGameOver -= GameOver;
            OnGameReset -= BoardManager.Instance.ResetGame;
            OnGameQuit -= QuitToMenu;

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

            // Also add the event calls
            // TODO
            
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

        SetText(turnDisplay, team + " turn", colour, outline);
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
        SetText(winMessage, team + " have won!", colour, outline);

        // Wait 2 seconds then disable the text
        yield return new WaitForSeconds(2);
        winMessage.enabled = false;
    }

}
