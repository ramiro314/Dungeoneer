using System;
using UnityEngine;
using UnityEngine.Networking;

public class MyNetworkManager : NetworkManager
{

    public int Size;
    public RenderDungeon rDungeon;
    private MatchManager _matchManager;

    private void Start()
    {
        Camera.main.transform.position = new Vector3(Size / 2, Size, Size /2);
    }

    public override void OnStartServer() {
        GameObject go = new GameObject("Dungeon");
	    rDungeon = go.AddComponent<RenderDungeon>();
    }

    public override void OnStartClient(NetworkClient client)
    {
//        _matchManager = GameObject.FindWithTag("MatchUI").GetComponent<MatchManager>();
//        _matchManager.Show();
//        _matchManager.AddPlayer(client);
    }
}
