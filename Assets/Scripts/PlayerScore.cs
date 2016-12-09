/*
Se encarga de armar el player score en la UI.
*/
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class PlayerScore : MonoBehaviour
{
    public MyThirdPersonUserControl player;
    private Text playerName;
    private Text playerScore;
    private Image playerColor;

	void Start ()
	{
	    Debug.Log(string.Format("Creating Score for player {0}", player.playerName));
	    playerName = transform.FindChild("Name").gameObject.GetComponent<Text>();
	    playerScore = transform.FindChild("Score").gameObject.GetComponent<Text>();
	    playerColor = transform.FindChild("ImageWraper").transform.FindChild("Color").gameObject.GetComponent<Image>();
	    playerName.text = player.playerName;
	    playerScore.text = player.coins.ToString();
	    playerColor.color = player.color;
	}

	void Update () {
	    playerScore.text = player.coins.ToString();
	}
}
