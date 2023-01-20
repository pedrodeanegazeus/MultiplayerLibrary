using System.Net.Sockets;
using MultiplayerLibrary.Extensions;
using MultiplayerLibrary.Interfaces.Handlers;
using MultiplayerLibrary.Interfaces.Models;
using MultiplayerLibrary.Interfaces.Services;

namespace MultiplayerLibrary.Handlers;

internal class ConnectionHandler : IConnectionHandler
{
    public event Action<Guid>? ClientDisconnected;
    public event Action<IConnectionHandler, byte[]>? PackageReceived;
    public event Action<Exception, string>? PackageReceivedError;

    public Guid Id { get; set; }

    private ICompressionService CompressionService { get; }
    private NetworkStream? NetworkStream { get; set; }
    private TcpClient? TcpClient { get; set; }

    public ConnectionHandler(ICompressionService compressionService)
    {
        CompressionService = compressionService;
    }

    public void Initialize(TcpClient tcpClient)
    {
        TcpClient = tcpClient;
        NetworkStream = tcpClient.GetStream();
        _ = ReceivePackagesAsync();
    }

    /// <exception cref="InvalidOperationException" />
    public async Task<int> SendAsync(IPackage package)
    {
        if (NetworkStream is null || TcpClient is null) throw new InvalidOperationException("ConnectionHandler is not initialized, run Initialize");
        if (TcpClient.Connected)
        {
            byte[] payloadBytes = await package.ToByteArrayAsync();
            byte[] packageBytes = new byte[3 + payloadBytes.Length];
            Array.Copy(((short)package.Type).ToByteArray(), packageBytes, 2);
            packageBytes[2] = package.Version;
            Array.Copy(payloadBytes, 0, packageBytes, 3, payloadBytes.Length);
            if (package.Compressed)
            {
                packageBytes = await CompressionService.GzipPackageAsync(packageBytes);
            }
            byte[] bytesToSend = new byte[3 + packageBytes.Length]; // size (2 bytes) + is compressed (1 byte) + package
            Array.Copy(((short)packageBytes.Length).ToByteArray(), bytesToSend, 2);
            bytesToSend[2] = (byte)(package.Compressed ? 1 : 0);
            Array.Copy(packageBytes, 0, bytesToSend, 3, packageBytes.Length);
            await NetworkStream.WriteAsync(bytesToSend);
            return bytesToSend.Length;
        }
        return 0;
    }

    private async Task HandlePackageAsync(byte[] bytes)
    {
        int index = 0;
        while (index < bytes.Length)
        {
            short packageSize = bytes[index..(index + 2)].ToShort();
            bool isCompressed = bytes[index + 2] == 1;
            index += 3;
            byte[] packageBytes = isCompressed
                ? await CompressionService.GunzipPackageAsync(bytes[index..(index + packageSize)])
                : bytes[index..(index + packageSize)];
            index += packageSize;
            PackageReceived?.Invoke(this, packageBytes);
        }
    }

    private async Task ReceivePackagesAsync()
    {
        if (NetworkStream is null || TcpClient is null) throw new InvalidOperationException("ConnectionHandler is not initialized, run Initialize");
        byte[] buffer = new byte[TcpClient.ReceiveBufferSize];
        int bytesRead = 0;
        try
        {
            while (TcpClient.Connected)
            {
                bytesRead = 0;
                bytesRead = await NetworkStream.ReadAsync(buffer);
                if (bytesRead == 0)
                {
                    TcpClient.Close();
                }
                else
                {
                    await HandlePackageAsync(buffer[0..bytesRead]);
                }
            }
        }
        catch (Exception ex)
        {
            string package = Convert.ToBase64String(buffer.AsSpan(0..bytesRead));
            PackageReceivedError?.Invoke(ex, $"Package error: '{package}'");
        }
        finally
        {
            TcpClient.Close();
            ClientDisconnected?.Invoke(Id);
        }
    }
}
