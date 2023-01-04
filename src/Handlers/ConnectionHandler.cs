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

    public async Task<int> SendAsync(IPackage package)
    {
        if (NetworkStream is null || TcpClient is null) throw new InvalidOperationException("ConnectionHandler is not initialized, run Initialize");
        if (TcpClient.Connected)
        {
            byte[] packageBytes = await package.ToByteArrayAsync();
            if (package.Compressed)
            {
                packageBytes = await CompressionService.GzipPackageAsync(packageBytes);
            }
            byte[] bytes = new byte[3 + packageBytes.Length]; // size (2 bytes) + is compressed (1 byte) + package
            byte[] packageLengthBytes = ((short)packageBytes.Length).ToByteArray();
            Array.Copy(packageLengthBytes, 0, bytes, 0, 2);
            bytes[2] = (byte)(package.Compressed ? 1 : 0);
            Array.Copy(packageBytes, 0, bytes, 3, packageBytes.Length);
            await NetworkStream.WriteAsync(bytes);
            return bytes.Length;
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
