using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class HUD : MonoBehaviour
{
    public TMP_Text winMessage;

    public static event Action OnGameQuit;
    public static event Action OnGameReset;

    private void Awake()
    {
        // Hide the win text
        winMessage.enabled = false;

        // Add the GameOver method to the OnGameOver event call
        BoardManager.OnGameOver += GameOver;

        // Add the quit to menu method to the event
        OnGameQuit += QuitToMenu;
        OnGameReset += BoardManager.Instance.ResetGame;
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
        HideHUD();

        // Quit to menu
        SceneManager.LoadScene(0);
    }

    private void OnDestroy()
    {
        HideHUD();
    }



    public void HideHUD()
    {
        if (SceneManager.GetSceneByName("HUD").isLoaded)
        {
            Debug.Log("HUD Unloaded");
            SceneManager.UnloadSceneAsync("HUD");
        }
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
        // Set the properties and display
        winMessage.text = team + " have won!";
        winMessage.color = colour;
        winMessage.outlineWidth = 0.25f;
        winMessage.outlineColor = outline;
        winMessage.enabled = true;

        // Wait 2 seconds then disable the text
        yield return new WaitForSeconds(2);
        winMessage.enabled = false;
    }

}
