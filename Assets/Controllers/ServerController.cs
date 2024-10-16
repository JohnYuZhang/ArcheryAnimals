using System;
using System.Collections;
using System.Collections.Generic;
using Riptide;
using Riptide.Utils;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ServerController : MonoBehaviour
{
    public GameObject PlayerPrefab;

    // Start is called before the first frame update
    private Server _server;
    private Dictionary<ushort, Player> _players = new Dictionary<ushort, Player>();
    void Start()
    {
        Application.targetFrameRate = 60;
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        _server = new Server();
        _server.Start(Constants.PORT, Constants.MAX_CLIENTS);
        _server.ClientDisconnected += PlayerLeft;
        _server.MessageReceived += MessageReceived;
    }

    private void PlayerLeft(object sender, ServerDisconnectedEventArgs e)
    {
        var player = _players[e.Client.Id];
        _players.Remove(e.Client.Id);
        Destroy(player.gameObject);
    }

    private void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        var id = e.FromConnection.Id;
        switch ((ClientToServerId) e.MessageId)
        {
            case ClientToServerId.name:
                var username = e.Message.GetString();
                var player = Player.InflatePlayer(PlayerPrefab, id, username);
                foreach (Player existingPlayer in _players.Values)
                    // notify the new player of all the existing players in the lobby
                    SendSpawn(id, existingPlayer);
                BroadcastSpawn(player);
                _players.Add(id, player);
                break;
            case ClientToServerId.position:
                //e.Message.GetUShort();
                //var position = e.Message.GetVector3();
                //var playerToMove = _players[id];
                //playerToMove.transform.position = position;
                break;
            case ClientToServerId.input:
                var inputState = PlayerInputState.CreateFromMessage(e.Message);
                var playerToMove2 = _players[id];
                playerToMove2.ApplyPlayerInputState(inputState);
                break;
        }
    }

    private void SendSpawn(ushort toClientId, Player player)
    {
        var message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientId.playerSpawned);
        message = player.AppendSpawnData(message); // functional programmers are literally quaking in their boots rn
        _server.Send(message, toClientId);
    }

    private void BroadcastSpawn(Player player)
    {
        var message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClientId.playerSpawned);
        message = player.AppendSpawnData(message);
        _server.SendToAll(message);
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        _server.Update();

        // update all positions
        foreach (Player player in _players.Values)
        {
            var message = Message.Create(MessageSendMode.Unreliable, (ushort)ServerToClientId.playerPosition);
            message = player.AppendCurrentPosition(message);
            _server.SendToAll(message);
        }
    }

    private void OnApplicationQuit()
    {
        _server.Stop();
    }

}
