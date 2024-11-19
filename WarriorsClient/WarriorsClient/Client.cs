using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WarriorsClient
{
    public class Client
    {
        private const string ServerIp = "127.0.0.1";
        private const int ServerPort = 8888;
        private TcpClient _client;
        private NetworkStream _stream;
        private byte[] _buffer;
        private static string loggedInAs;

        private CommandProcessor _commandProcessor;

        public void Connect()
        {
            try
            {
                _client = new TcpClient();
                TryConnect();
                _stream = _client.GetStream();
                _buffer = new byte[1024];
                _commandProcessor = new CommandProcessor(_stream, _client);
                Task.Run(ReceiveMessages);
                HandleUserCommands();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
        public static void SetLoggedInAs(string loggedAs)
        {
            loggedInAs = loggedAs;
        }
        public static string GetLoggedInAs()
        {
            return loggedInAs;
        }

        private void HandleUserCommands()
        {
            Console.WriteLine("Enter a command: (addResources,addTroops, remove, upgrade, get, gettroops, getemails, sendemail, grid, login, register, exit)");
            while (true)
            {
                string command = Console.ReadLine()?.ToLower();
                _commandProcessor.ProcessCommand(command);
            }
        }

        private async Task ReceiveMessages()
        {
            try
            {
                while (true)
                {
                    int bytesRead = await _stream.ReadAsync(_buffer, 0, _buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(_buffer, 0, bytesRead);
                        MessageHandler.HandleMessage(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        private void TryConnect()
        {
            bool didConnect = false;
            while (!didConnect)
                try
                {
                    _client.Connect(ServerIp, ServerPort);
                    didConnect = true;
                }
                catch (Exception)
                {
                    Console.WriteLine($"Unable to establish connection to server: {ServerIp},{ServerPort}. retrying in 5s...");
                    Thread.Sleep(5000);
                }
        }
    }
}
 