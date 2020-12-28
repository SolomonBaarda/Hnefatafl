using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public TMP_Text resetButton, quitButton;
    public TMP_Text turnText, winText;

    public static event Action OnGameQuit;
    public static event Action OnGameReset;

    private void Start()
    {
        // Add the GameOver method to the OnGameOver event call
        BoardManager.OnGameWon += GameOver;
        // Add the turn indicator
        BoardManager.OnTurnStart += UpdatePlayerTurnDisplay;
        // Add the reset game method to the event
        OnGameReset += BoardManager.Instance.ResetGame;
        OnGameReset += ResetHud;
        // Add the quit to menu method to the event
        OnGameQuit += QuitGame;

        // Set up buttons
        SetButton(resetButton, "RESET", Color.white, Color.black, Color.gray, Color.black);
        SetButton(quitButton, "X", Color.white, Color.black, Color.gray, Color.black);
        // Add listeners
        resetButton.GetComponent<Button>().onClick.AddListener(() => OnResetGameClicked());
        quitButton.GetComponent<Button>().onClick.AddListener(() => OnQuitToMenuClicked());

        // Hide the win text
        winText.gameObject.SetActive(false);
    }

    public void ResetHud()
    {
        UpdatePlayerTurnDisplay(BoardManager.Instance.State);
    }

    public void OnQuitToMenuClicked()
    {
        OnGameQuit.Invoke();
    }

    public void OnResetGameClicked()
    {
        OnGameReset.Invoke();
    }


    private void QuitGame()
    {
        Application.Quit();
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
            BoardManager.OnGameWon -= GameOver;
            OnGameReset -= BoardManager.Instance.ResetGame;
            OnGameReset -= ResetHud;
            OnGameQuit -= QuitToMenu;
            resetButton.GetComponent<Button>().onClick.RemoveAllListeners();
            quitButton.GetComponent<Button>().onClick.RemoveAllListeners();

            // Unload this scene
            SceneManager.UnloadSceneAsync("HUD");
        }
    }

    private void SetButton(TMP_Text t, string message, Color colour, Color outline, Color hover, Color pressed)
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


    private void UpdatePlayerTurnDisplay(BoardManager.GameState state)
    {
        string team;
        Color colour, outline;
        if (state == BoardManager.GameState.AttackingTurn)
        {
            team = "Attackers";
            colour = Color.white;
            outline = Color.black;
        }
        else if(state == BoardManager.GameState.AttackingTurn)
        {
            team = "Defenders";
            colour = Color.black;
            outline = Color.white;
        }
        else
        {
            return;
        }

        SetText(turnText, team + " turn", colour, outline);
    }

    private void GameOver(BoardManager.Team team)
    {
        int waitForSeconds = 4;

        Debug.Log("OnGameOver called with team " + team);
        if (team.Equals(BoardManager.Team.Attacking))
        {
            StartCoroutine(DisplayWon("Attackers", Color.white, Color.black, waitForSeconds));
        }
        else
        {
            StartCoroutine(DisplayWon("Defenders", Color.black, Color.white, waitForSeconds));
        }
    }

    private IEnumerator DisplayWon(string team, Color colour, Color outline, int seconds)
    {
        SetText(winText, team + " have won!", colour, outline);
        winText.gameObject.SetActive(true);

        // Wait seconds seconds then disable the text
        yield return new WaitForSeconds(seconds);
        winText.gameObject.SetActive(false);
    }

}
