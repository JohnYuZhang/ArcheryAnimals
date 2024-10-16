public class Constants
{
    public static string HOSTNAME = "127.0.0.1";
    public static ushort PORT = 7777;
    public static ushort MAX_CLIENTS = 3;
}

public enum ServerToClientId : ushort
{
    playerSpawned = 1,
    playerPosition = 2,
}

public enum ClientToServerId : ushort
{
    name = 1,
    position = 2,
    input = 3,
}