using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class PlayerScore : MonoBehaviour
{
    public MyThirdPersonUserControl player;
    private Text playerName;
    private Text playerScore;
    private Image playerColor;
    // Use this for initialization
	void Start ()
	{
	    Debug.Log(string.Format("Creating Score for player {0}", player.playerName));
	    playerName = transform.FindChild("Name").gameObject.GetComponent<Text>();
	    playerScore = transform.FindChild("Score").gameObject.GetComponent<Text>();
	    playerColor = transform.FindChild("ImageWraper").transform.FindChild("Color").gameObject.GetComponent<Image>();
	    playerName.text = player.playerName;
	    playerScore.text = player.coins.ToString();
	    playerColor.color = player.color;
//	    if (player.isLocalPlayer)
//	    {
//	        playerName.fontSize = 55;
//	        playerScore.fontSize = 55;
//	    }
	}
	
	// Update is called once per frame
	void Update () {
	    playerScore.text = player.coins.ToString();
	}
}
