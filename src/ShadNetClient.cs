using Google.Protobuf;
using NLog;
using Shadnet;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MHShadNetProxy
{
    internal class ShadNetClient
    {
        private const int ReceiveBufferSize = 8192;
        private const byte ShadNetProtocolVersion = 1;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Socket _socket;
        private readonly CancellationToken _cancellationToken;
        private readonly byte[] _receiveBuffer = new byte[ReceiveBufferSize];

        private int _receivePosition = 0;
        private ulong _currentPacketId = 0;

        public ShadNetClient(Socket socket, CancellationToken cancellationToken)
        {
            _socket = socket;
            _cancellationToken = cancellationToken;
            _ = ReceiveData();

            Span<byte> serverInfo = stackalloc byte[sizeof(int)];
            serverInfo[0] = ShadNetProtocolVersion;
            SendPacket(ShadNetPacketType.ServerInfo, 0, 0, serverInfo);
        }

        private async Task ReceiveData()
        {
            Logger.Info("Connected");

            while (_cancellationToken.IsCancellationRequested == false)
            {
                try
                {
                    int received = await _socket.ReceiveAsync(_receiveBuffer.AsMemory(_receivePosition), _cancellationToken);
                    if (received == 0)
                        break;

                    _receivePosition += received;

                    int consumed = ConsumeData(_receiveBuffer.AsSpan(0, _receivePosition));
                    if (consumed > 0)
                    {
                        _receivePosition -= consumed;
                        if (_receivePosition > 0)
                            Buffer.BlockCopy(_receiveBuffer, consumed, _receiveBuffer, 0, _receivePosition);
                    }
                    else if (_receivePosition == ReceiveBufferSize)
                    {
                        throw new InternalBufferOverflowException();
                    }
                }
                catch (SocketException)
                {
                    break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    break;
                }
            }

            _socket?.Disconnect(false);
            Logger.Info("Disconnected");
        }

        private ulong SendPacket(ShadNetPacketType type, ShadNetCommandType command, ulong packetId, Span<byte> payload)
        {
            ShadNetPacketHeader header = new()
            {
                Type = type,
                Command = command,
                Size = Unsafe.SizeOf<ShadNetPacketHeader>() + payload.Length,
                PacketId = packetId,
            };

            using MemoryStream stream = new();
            stream.Write(header);
            stream.Write(payload);
            _socket.Send(stream.ToArray());

            return _currentPacketId;
        }

        private ulong SendPacket(ShadNetPacketType type, ShadNetCommandType command, ulong packetId, IMessage payload)
        {
            int size = payload.CalculateSize();
            
            using MemoryStream stream = new();

            if (type == ShadNetPacketType.Reply)
                stream.Write((byte)0);

            stream.Write(size);

            using CodedOutputStream cos = new(stream);
            payload.WriteTo(stream);
            cos.Flush();

            byte[] buffer = stream.ToArray();
            return SendPacket(type, command, packetId, buffer);
        }

        private int ConsumeData(Span<byte> bytes)
        {
            int consumedTotal = 0;

            int consumed;
            while ((consumed = ReadPacket(bytes)) > 0)
            {
                consumedTotal += consumed;
                bytes = bytes[consumed..];
            }

            return consumedTotal;
        }

        private int ReadPacket(Span<byte> bytes)
        {
            int headerSize = Unsafe.SizeOf<ShadNetPacketHeader>();

            if (bytes.Length < headerSize)
                return 0;

            ShadNetPacketHeader header = MemoryMarshal.Read<ShadNetPacketHeader>(bytes);
            if (header.Size < 0 || header.Size > ReceiveBufferSize)
                throw new InternalBufferOverflowException();

            if (bytes.Length < header.Size)
                return 0;

            switch (header.Type)
            {
                case ShadNetPacketType.Request:
                    HandleRequest(header.Command, header.PacketId, bytes[headerSize..]);
                    break;

                default:
                    Logger.Warn($"ReadPacket(): Unhandled packet type {header.Type}");
                    break;
            }

            return header.Size;
        }

        private void HandleRequest(ShadNetCommandType command, ulong requestId, Span<byte> payload)
        {
            switch (command)
            {
                case ShadNetCommandType.Login:
                    LoginReply loginReply = new()
                    {
                        AvatarUrl = string.Empty,
                        UserId = 1000,
                    };
                    SendPacket(ShadNetPacketType.Reply, command, requestId, loginReply);
                    break;

                case ShadNetCommandType.GetToken:
                    GetTokenReply getTokenReply = new()
                    {
                        Token = string.Empty,
                        UserId = 1000,
                        Npid = string.Empty,
                    };
                    SendPacket(ShadNetPacketType.Reply, command, requestId, getTokenReply);
                    break;

                case ShadNetCommandType.GetServerFeatures:
                    ServerFeaturesReply serverFeaturesReply = new()
                    {
                        Matching2Enabled = false,
                        TrophiesEnabled = false,
                    };
                    SendPacket(ShadNetPacketType.Reply, command, requestId, serverFeaturesReply);
                    break;

                default:
                    Logger.Warn($"ReceivePacket(): Unhandled command {command}");
                    break;
            }
        }
    }
}
