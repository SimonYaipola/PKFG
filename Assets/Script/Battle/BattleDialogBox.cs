using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI dialogText;
    [SerializeField] int lettersPerSeconds;
    [SerializeField] Color highlightedColor;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] List<TextMeshProUGUI> actionTexts;
    [SerializeField] List<TextMeshProUGUI> moveTexts;
    [SerializeField] TextMeshProUGUI ppText;
    [SerializeField] TextMeshProUGUI typeText;



    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }
    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text ="";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f/lettersPerSeconds);
        }
        yield return new WaitForSeconds(1f);
    }
    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }
        public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }
        public void EnablemoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }
    public void UpdateActionSelection(int selectedAction)
    {
        for (int i=0; i<actionTexts.Count; ++i)
        {
            if (i == selectedAction)
                actionTexts[i].color = highlightedColor;
                else
                    actionTexts[i].color = Color.black;
        }
    }
    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        for (int i=0; i<moveTexts.Count; i++)
        {
            if (i == selectedMove)
                moveTexts[i].color = highlightedColor;
                else
                    moveTexts[i].color = Color.black;
        }
        ppText.text = $"PP {move.PP}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString();

        if (move.PP == 0)
            ppText.color = Color.red;
        else
            ppText.color = Color.black;
    }
    public void SetMoveNames(List<Move> moves)
    {
        for (int i=0; i<moveTexts.Count; i++)
        {
            if (i < moves.Count)
                moveTexts[i].text = moves[i].Base.Name;
            else    moveTexts[i].text = "-";
        }
    }
}
