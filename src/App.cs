using NLog;

namespace MHShadNetProxy
{
    internal class App
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private bool _isRunning = false;

        public ShadNetServer ShadNetServer { get; } = new();

        public App() { }

        public void Run()
        {
            if (_isRunning)
                return;

            _isRunning = true;

            Logger.Info("Starting MHShadnetProxy...");

            ShadNetServer.Start();

            while (_isRunning)
            {
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (string.Equals(input, "stop", StringComparison.OrdinalIgnoreCase))
                    _isRunning = false;
            }

            ShadNetServer.Stop();

            Logger.Info("MHShadnetProxy finished running");
        }
    }
}
