using UnityEngine;
using Prototype.NetworkLobby;
using System.Collections;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.ThirdPerson;

public class MyLobbyHook : LobbyHook
{
    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
    {
        LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
        MyThirdPersonUserControl playerChar = gamePlayer.GetComponent<MyThirdPersonUserControl>();

        playerChar.playerName = lobby.playerName;
        playerChar.color = lobby.playerColor;
    }
}
