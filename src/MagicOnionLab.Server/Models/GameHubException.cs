namespace MagicOnionLab.Server.Models;

public class GameHubException : Exception
{
    public GameHubException(string message) : base(message) { }
}
