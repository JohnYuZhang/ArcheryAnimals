using System;
using System.Collections.Generic;
using Riptide;
using UnityEngine;

public class Player : MonoBehaviour
{
    public ushort Id { get; private set; }
    public string Username { get; private set; }

    public static Player InflatePlayer(GameObject playerPrefab, ushort id, string username)
    {
        Player player = Instantiate(playerPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<Player>();
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;
        return player;
    }

    public Message AppendSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddVector3(transform.position);
        return message;
    }

    public Message AppendCurrentPosition(Message message)
    {
        message.AddUShort(Id);
        message.AddVector3(transform.position);
        return message;
    }

    void Start()
    {

    }

    private void Update()
    {
        
    }
}