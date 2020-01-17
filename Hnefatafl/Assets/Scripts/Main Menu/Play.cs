using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Play : MonoBehaviour
{
    public void PlayHnefatafl()
    {
        SceneManager.LoadScene("Hnefatafl");
    }

    public void PlayTablut()
    {
        SceneManager.LoadScene("Tablut");
    }

}
