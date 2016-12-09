/*
Game manager del juego.
Se encarga de:
- Generar el dungeon.
- Iniciar el countdown de inicio.
- Spawnear los players en el dungeon.
- Avisar a los clientes cuando el juego llego a su fin.
- Iniciar el countdown de fin.
*/
using UnityEngine;
using UnityEngine.Networking;
using Prototype.NetworkLobby;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityStandardAssets.Utility;

public class NetworkGameManager : NetworkBehaviour
{
    static public NetworkGameManager sInstance;
    public int startSeconds;
    public int endSeconds;
    public Text gameStartText;
    public Text gameEndText;
    public int coinsToWin = 10;
    public bool gameEnded;
    public int playerNumber;

    void Awake()
    {
        sInstance = this;
    }

    void Start()
    {
        // Generate dungeon only in the server, all the objects of the dungeon will spawn in the clients.
        if (isServer)
        {
            GameObject go = new GameObject("Dungeon");
            go.AddComponent<RenderDungeon>();
        }

        playerNumber = GameObject.FindGameObjectsWithTag("Player").Length;
        gameEnded = false;

        // Place the camera on top of the dungeon.
        int size = LobbyManager.s_Singleton._playerNumber * 10 + 1;
        Camera.main.transform.position = new Vector3(size / 2f, size, size / 2f);
        StartCoroutine(GameStartCoroutine());
    }

    [ClientRpc]
    public void RpcEndGame()
    {
        gameEnded = true;
        StartCoroutine(GameEndCoroutine());
    }

    IEnumerator GameStartCoroutine()
    {
        // Display countdown.
        gameStartText.enabled = true;
        for (int i = 0; i < startSeconds; i++)
        {
            gameStartText.text = (startSeconds - i).ToString();
            yield return new WaitForSeconds(1.0f);
        }
        gameStartText.enabled = false;
        var players = GameObject.FindGameObjectsWithTag("Player");

        // The server will tell each client where to spawn the player.
        if(isServer){
            foreach (GameObject player in players)
            {
                player.GetComponent<MyThirdPersonUserControl>().RpcSpawn(NetworkManager.singleton.GetStartPosition().position);
            }
        }

        // Enable UI
        GameObject scores = GameObject.Find("Scores");
        Object scorePrefab = Resources.Load("Prefabs/PlayerScore");
        foreach (GameObject player in players)
        {
            GameObject score = (GameObject)Instantiate(scorePrefab, Vector3.zero, Quaternion.identity, scores.transform);
            score.GetComponent<PlayerScore>().player = player.GetComponent<MyThirdPersonUserControl>();
        }
        GameObject.Find("ItemBorder").GetComponent<RawImage>().enabled = true;

        // Enable camera script
        Camera.main.GetComponent<MySmoothFollow>().enabled = true;
    }

    IEnumerator GameEndCoroutine()
    {
        // Tell each player if they won or lost.
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerObject in players)
        {
            MyThirdPersonUserControl player = playerObject.GetComponent<MyThirdPersonUserControl>();
            if (player.coins >= coinsToWin)
            {
                player.RpcWinMatch();
            }
            else
            {
                player.RpcLoseMatch();
            }
        }

        // Display countdown.
        gameEndText.enabled = true;
        for (int i = 0; i < endSeconds; i++)
        {
            gameEndText.text = string.Format("Going back to looby in {0} seconds", endSeconds - i);
            yield return new WaitForSeconds(1.0f);
        }
        gameEndText.enabled = false;
        LobbyManager.s_Singleton.ServerReturnToLobby();
    }

}
