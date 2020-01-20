using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class WinDisplay : MonoBehaviour
{
    public TMP_Text message;

    private void Start()
    {
        message.enabled = false;
    }

    public void DisplayAttackingWon()
    {
        StartCoroutine(DisplayWon("Attackers", Color.white, Color.black));
    }

    public void DisplayDefendingWon()
    {
        StartCoroutine(DisplayWon("Defenders", Color.black, Color.white));
    }

    private IEnumerator DisplayWon(string team, Color colour, Color outline)
    {
        message.text = team + " have won!";
        message.color = colour;
        message.outlineWidth = 0.25f;
        message.outlineColor = outline;
        message.enabled = true;

        yield return new WaitForSeconds(2);

        message.enabled = false;
    }
}
