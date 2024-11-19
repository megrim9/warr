using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Utils;

namespace WarriorsServer
{
    public class Server
    {
        private const int Port = 8888;
        private const int MaxPlayers = 100;
        private const int PlayerIdCounter = 100000; // player id start nr
        private Socket _serverSocket;
        private List<Player> _players;
        private const int ResourceUpdateInterval = 10000; // 10 seconds

        public void Start()
        {
            Map.InitializeMap();

            // test code start///
            _players = new List<Player>
            {
                new Player("l1", "p1", PlayerIdCounter + 11111, 7,9),
                new Player("l2", "p2", PlayerIdCounter + 22222, 9,9),
            };
            City c1 = _players[0].GetCity(0);
            City c2 = _players[1].GetCity(0);

            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ipA = IPAddress.Any;
            _serverSocket.Bind(new IPEndPoint(ipA, Port));
            _serverSocket.Listen(MaxPlayers);
            Console.WriteLine($"Server started on {ipA}:{Port}. Waiting for connections...");

            Task.Run(GameLoop);
            Task.Run(NotificationsLoop);
            AcceptClientConnections();

            while (true)
            {
            }
        }

        private async Task NotificationsLoop()
        {
            try
            {
                //string message = GetPromoMessage(); // will read a file and output its text as news in the future
                BroadcastToAllExcept(null, new Message { Type = MessageType.Welcome, Text = "this is the news!!" });
                await Task.Delay(TimeSpan.FromHours(1));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR while broadcasting news!!! {ex}");
            }
        }

        private async Task GameLoop()
        {
            while (true)
            {
                DateTime startTime = DateTime.UtcNow;

                // Update resources for each player
                Parallel.ForEach(_players, (player) =>
                {
                    player.AddResources(player.WoodFarmLevel, player.ClayFarmLevel, player.IronFarmLevel, player.CropFarmLevel);
                    Console.WriteLine($"Resources added to Player {player.Login}: Wood={player.Wood}, Clay={player.Clay}, Iron={player.Iron}, Crop={player.Crop}");
                });

                // Calculate the time elapsed for the resource update
                TimeSpan elapsedTime = DateTime.UtcNow - startTime;

                // Delay the remaining time until the next resource update interval
                int delayTime = ResourceUpdateInterval - (int)elapsedTime.TotalMilliseconds;
                if (delayTime > 0)
                    await Task.Delay(delayTime);
            }
        }

        private async void AcceptClientConnections()
        {
            while (true)
            {
                Socket clientSocket = await _serverSocket.AcceptAsync(); // waits for any new user to connect, set a socket to it.

                // Use Task.Run to handle each client concurrently
                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        Player player = await AuthenticateUser(clientSocket);
                        if (player != null)
                        {
                            if (player.ClientSocket != null)
                            {
                                SendToClient(player.ClientSocket, new Message { Type = MessageType.Error, Text = $"Login attempt on your account was made!" });
                                SendToClient(clientSocket, new Message { Type = MessageType.Error, Text = $"Account already logged in!" });
                                continue;
                            }
                            player.ClientSocket = clientSocket;
                            // Send a welcome message to the player
                            SendToClient(player.ClientSocket, new Message { Type = MessageType.Welcome, Text = $"Welcome to the game {player.Login}!" });

                            // Start handling the player's actions in a separate task
                            MessageProcessor messageProcessor = new MessageProcessor(player, _players, BroadcastToAllExcept);
                            _ = Task.Run(() => HandlePlayerActions(player, messageProcessor));

                            // Notify other players about the new player
                            BroadcastToAllExcept(player.ClientSocket, new Message { Type = MessageType.Welcome, Text = $"Player {player.Login} joined the game." });
                            break;
                        }
                    }
                });
            }
        }

        private async Task<Player> AuthenticateUser(Socket clientSocket)
        {
            // Receive login and password from the client
            byte[] buffer = new byte[1024];
            int bytesRead = await clientSocket.ReceiveAsync(buffer, SocketFlags.None);
            if (bytesRead <= 0) return null;
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            LoginMessage loginObj = JsonConvert.DeserializeObject<LoginMessage>(message);
            if(loginObj == null) return null;
            if (loginObj.Type == MessageType.Register)
            {
                if (_players.Any(v => v.Login == loginObj.Login))
                {
                    SendToClient(clientSocket, new Message { Type = MessageType.Error, Text = $"Login Already Exist!" });
                    return null;
                }
                Player pp = new Player(loginObj.Login, loginObj.Password, PlayerIdCounter + 10000, 7, 9);
                if (pp.GetCity(0) == null)
                {
                    SendToClient(clientSocket, new Message { Type = MessageType.Error, Text = $"Error registering (wrong city placement)" });
                    return null;
                }
                _players.Add(pp);
                SendToClient(clientSocket, new Message { Type = MessageType.Success, Text = $"User Created!" });
            }
            else if (loginObj.Type == MessageType.Login)
            {
                Player p = _players.FirstOrDefault(u => u.Login == loginObj.Login && u.Password == loginObj.Password);
                if (p == null)
                {
                    SendToClient(clientSocket, new LoginMessage { Type = MessageType.LoginAttempt, Text = $"Login failed!", Login = $"{loginObj.Login}", Status = false });
                }
                else //logging in
                {
                    SendToClient(clientSocket, new LoginMessage { Type = MessageType.LoginAttempt, Text = $"Logged in as {loginObj.Login}", Login = $"{loginObj.Login}", Status = true });
                }
                return p;
            }
            return null;
        }

        private async void HandlePlayerActions(Player player, MessageProcessor messageProcessor)
        {
            byte[] buffer = new byte[1024];

            while (player.ClientSocket.Connected)
            {
                try
                {
                    int bytesRead = await player.ClientSocket.ReceiveAsync(buffer, SocketFlags.None);

                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        messageProcessor.ProcessMessage(message);
                    }
                }
                catch (SocketException)
                {
                    // Handle disconnection or socket errors
                    player.ClientSocket.Close();
                    // Notify other players about the disconnected player
                    BroadcastToAllExcept(player.ClientSocket, new Message { Type = MessageType.Welcome, Text = $"Player {player.Login} left the game." });
                    player.ClientSocket = null;
                    break;
                }
            }
        }

        internal static void SendToClient(Socket clientSocket, Message message)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                clientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        public void BroadcastToAllExcept(Socket excludeClientSocket, Message message)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                foreach (Player player in _players)
                {
                    if (player.ClientSocket != null && player.ClientSocket != excludeClientSocket) // if player is online && is not the same
                    {
                        player.ClientSocket.Send(buffer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}