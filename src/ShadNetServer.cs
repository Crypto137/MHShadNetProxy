using NLog;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace MHShadNetProxy
{
    internal class ShadNetServer
    {
        private const string BindIP = "0.0.0.0";
        private const int Port = 31313;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private bool _isRunning;
        private CancellationTokenSource _cts;

        private Socket _listener;

        public void Start()
        {
            if (_isRunning)
                return;

            _isRunning = true;

            Logger.Info("Starting ShadNetServer...");

            Debug.Assert(_cts == null);
            _cts = new();

            Debug.Assert(_listener == null);
            _listener = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                LingerState = new(false, 0),
            };

            try
            {
                _listener.Bind(new IPEndPoint(IPAddress.Parse(BindIP), Port));
            }
            catch (SocketException)
            {
                Logger.Fatal($"Cannot bind on {BindIP}:{Port}");
                return;
            }

            _listener.Listen();
            _ = AcceptConnections();
            Logger.Info($"Listening on {BindIP}:{Port}...");
        }

        public void Stop()
        {
            if (_isRunning == false)
                return;

            _isRunning = false;

            Logger.Info("Stopping ShadNetServer...");

            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        private async Task AcceptConnections()
        {
            while (_cts.IsCancellationRequested == false)
            {
                try
                {
                    Socket socket = await _listener.AcceptAsync(_cts.Token);
                    Logger.Info("Accepted client connection");
                    ShadNetClient client = new(socket, _cts.Token);
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
        }
    }
}
