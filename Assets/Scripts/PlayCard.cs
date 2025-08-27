using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayCard : MonoBehaviour
{
    [SerializeField] private GameObject visuals;
    [SerializeField] private Sprite[] characters;
    [SerializeField] private Image characterIcon;
    [SerializeField] private TMP_Text playerName;

    public void disableDisplay()
    {
        visuals.SetActive(false);
    }
    
    public void updateDisplay(CharacterSelection state)
    {
        if (state.characterId == -1)
        {
            characterIcon.enabled = false;
        }
        else
        {
            characterIcon.sprite =  characters[state.characterId];
            characterIcon.enabled = true;
        }
        visuals.SetActive(true);
    }
}
