using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnDisplay : MonoBehaviour
{
    public Text turn;

    // Update is called once per frame
    private void Update()
    {
        string team;
        Color colour;
        if (BoardManager.Instance.isAttackingTurn)
        {
            team = "Attacking";
            colour = Color.white;
        }
        else
        {
            team = "Defending";
            colour = Color.black;
        }

        turn.text = team + " turn";
        turn.color = colour;
    }
}
