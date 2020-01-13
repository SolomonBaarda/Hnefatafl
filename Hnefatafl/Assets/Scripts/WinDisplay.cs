using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class WinDisplay : MonoBehaviour
{
    public Text message;

    public void Start()
    {
        message.enabled = false;
    }

    

    public void DisplayAttackingWon()
    {
        DisplayWon("Attacking", Color.white);
    }

    public void DisplayDefendingWon()
    {
        DisplayWon("Defending", Color.black);
    }

    private IEnumerator DisplayWon(string team, Color colour)
    {
        message.text = team + " team has won";
        message.color = colour;
        message.enabled = true;

        yield return new WaitForSeconds(2);

        message.enabled = false;
    }
}
