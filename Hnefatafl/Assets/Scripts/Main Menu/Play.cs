using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Play : MonoBehaviour
{
    public void PlayHnefatafl()
    {
        LoadGame();

        // Set the scene
        BoardManager.Instance.SelectGamemode(BoardManager.GameMode.Hnefatafl);
    }

    public void PlayTablut()
    {
        LoadGame();
        BoardManager.Instance.SelectGamemode(BoardManager.GameMode.Tablut);
    }

    private void LoadGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
