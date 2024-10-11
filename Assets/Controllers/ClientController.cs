using System;
using System.Collections;
using System.Collections.Generic;
using Riptide;
using Riptide.Utils;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ClientController : MonoBehaviour
{
    public GameObject PlayerPrefab;
    public GameObject MainPlayerPrefab;

    // Start is called before the first frame update
    private Client _client;
    private Dictionary<ushort, Player> _players = new Dictionary<ushort, Player>();
    private Player mainPlayer;
    void Start()
    {
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        _client = new Client();
        _client.Connect($"{Constants.HOSTNAME}:{Constants.PORT}");
        _client.MessageReceived += MessageReceived;
        _client.Connected += DidConnect;
    }

    private void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        switch ((ServerToClientId) e.MessageId)
        {
            case ServerToClientId.playerSpawned:
                var id = e.Message.GetUShort();
                var username = e.Message.GetString();
                var position = e.Message.GetVector3();

                Player player;
                if (id == _client.Id)
                {
                    player = Player.InflatePlayer(MainPlayerPrefab, id, username);
                    mainPlayer = player;
                }
                else
                {
                    player = Player.InflatePlayer(PlayerPrefab, id, username);
                }
                _players.Add(id, player);
                break;
            case ServerToClientId.playerPosition:
                var id1 = e.Message.GetUShort();
                var position1 = e.Message.GetVector3();
                if (id1 == _client.Id)
                {
                    // no-op for now because we're doing client movement
                }
                else if (_players.ContainsKey(id1))
                {
                    _players[id1].transform.position = position1;
                }
                break;
        }
    }

    private void DidConnect(object sender, EventArgs e)
    {
        // todo: capture name from main menu entry
        var message = Message.Create(MessageSendMode.Reliable, ClientToServerId.name);
        message.AddString("yeetus");
        _client.Send(message);
    }

    private void FixedUpdate()
    {
        _client.Update();
        if (mainPlayer != null)
        {
            var myPositionMessage = Message.Create(MessageSendMode.Reliable, ClientToServerId.position);
            myPositionMessage = mainPlayer.AppendCurrentPosition(myPositionMessage);
            _client.Send(myPositionMessage);
        }

    }

    private void OnApplicationQuit()
    {
        _client.Disconnect();
    }

}
