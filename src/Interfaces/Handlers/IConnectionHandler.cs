using System.Net.Sockets;
using MultiplayerLibrary.Interfaces.Models;

namespace MultiplayerLibrary.Interfaces.Handlers;

public interface IConnectionHandler
{
    event Action<Guid> ClientDisconnected;
    event Action<Guid, byte[]> PackageReceived;
    event Action<Exception, string> PackageReceivedError;

    Guid Id { get; set; }

    void Initialize(TcpClient tcpClient);
    Task<int> SendAsync(IPackage package);
}
