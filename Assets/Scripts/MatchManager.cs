using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;

public class MatchManager : NetworkBehaviour
{
    private CanvasGroup _canvasGroup;
    private List<NetworkClient> _playerList;

    private void Awake()
    {
        _playerList = new List<NetworkClient>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show()
    {
        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = true;
    }

    public void Hide()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
    }

    public void AddPlayer(NetworkClient client)
    {
        _playerList.Insert(client.connection.connectionId, client);
        AddPlayerInUI("Player " + _playerList.Count);
    }

    public void AddPlayerInUI(String playerName)
    {
        GameObject playerStringObject = new GameObject();
        Text playerText = playerStringObject.AddComponent<Text>();
        playerText.text = playerName;
        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        playerText.font = ArialFont;
        playerText.material = ArialFont.material;
        playerStringObject.transform.SetParent(transform.FindChild("Players"));
    }


}
