using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Newtonsoft.Json;
using Utils;

namespace WarriorsServer
{
    public class MessageProcessor
    {
        private readonly Action<Socket, Message> _broadcastToAllExcept;
        private readonly Player _player;
        private readonly List<Player> _players;

        public MessageProcessor(Player player, List<Player> players, Action<Socket, Message> broadcastToAllExcept)
        {
            _broadcastToAllExcept = broadcastToAllExcept;
            _player = player;
            _players = players;
        }

        public void ProcessMessage(string message)
        {
            Message messageObj = JsonConvert.DeserializeObject<Message>(message);

            if (messageObj == null) return;
            switch (messageObj.Type)
            {
                case MessageType.AddResources:
                    ProcessAddResources(message);
                    break;
                case MessageType.AddTroops:
                    ProcessAddTroops(message);
                    break;
                case MessageType.Upgrade:
                    ProcessUpgradeBuilding(message);
                    break;
                case MessageType.Attack:
                    ProcessAttack(message);
                    break;
                case MessageType.RemoveResources:
                    ProcessRemoveResources(message);
                    break;
                case MessageType.GetResources:
                    ProcessGetResources(message);
                    break;
                case MessageType.GetEmails:
                    ProcessGetEmails(message);
                    break;
                case MessageType.GetTroops:
                    ProcessGetTroops(message);
                    break;
                case MessageType.SendEmail:
                    ProcessSendEmail(message);
                    break;
                case MessageType.GetGridAroundPlayer:
                    ProcessGetGridAroundPlayer(message);
                    break;
                case MessageType.Disconnect:
                    ProcessDisconnect();
                    break;
                default:
                    Console.WriteLine($"Unknown message type: {messageObj.Type}");
                    break;
            }
        }

        private void ProcessAddResources(string message)
        {
            ResourcesMessage addResourcesMessage = JsonConvert.DeserializeObject<ResourcesMessage>(message);
            if (addResourcesMessage == null) return;
            _player.AddResources(addResourcesMessage.Wood, addResourcesMessage.Clay, addResourcesMessage.Iron, addResourcesMessage.Crop);
        }

        private void ProcessAddTroops(string message)
        {
            TroopsMessage addTroopsMessage = JsonConvert.DeserializeObject<TroopsMessage>(message);
            if (addTroopsMessage == null) return;
            int amount = addTroopsMessage.Amount;
            int cityIndex = addTroopsMessage.CityIndex;
            TroopType tt = addTroopsMessage.TroopType;
            _player.GetCity(cityIndex).GetAssociatedArmy().AddTroops(amount, tt);
        }
        
        private void ProcessUpgradeBuilding(string message)
        {
            UpgradeMessage buildingTypeMessage = JsonConvert.DeserializeObject<UpgradeMessage>(message);
            if (buildingTypeMessage == null) return;
            _player.UpgradeBuilding(buildingTypeMessage);
        }

        private void ProcessAttack(string message)
        {
            AttackMessage AttackTypeMessage = JsonConvert.DeserializeObject<AttackMessage>(message);
            if (AttackTypeMessage == null) return;
            _player.ProcessAttack(AttackTypeMessage);
        }

        private void ProcessRemoveResources(string message)
        {
            ResourcesMessage removeResourcesMessage = JsonConvert.DeserializeObject<ResourcesMessage>(message);
            if (removeResourcesMessage == null) return;
            _player.RemoveResources(removeResourcesMessage.Wood, removeResourcesMessage.Clay, removeResourcesMessage.Iron, removeResourcesMessage.Crop);
        }

        private void ProcessGetResources(string message)
        {
            ResourcesMessage getResourcesMessage = JsonConvert.DeserializeObject<ResourcesMessage>(message);
            if (getResourcesMessage == null) return;
            _player.GetResources();
        }
        private void ProcessGetEmails(string message)
        {
            EmailMessage getEmailMessage = JsonConvert.DeserializeObject<EmailMessage>(message);
            if (getEmailMessage == null) return;
            _player.GetEmails();
        }
        private void ProcessSendEmail(string message)
        {
            EmailMessage sendEmailMessage = JsonConvert.DeserializeObject<EmailMessage>(message);
            if (sendEmailMessage == null) return;
            Email email = sendEmailMessage.Emails.FirstOrDefault();
            if (email == null) return;
            if (email.Target == _player.Login)
            {
                Server.SendToClient(_player.ClientSocket, new Message { Type = MessageType.Error, Text = $"Can't send an email to yourself." });
                return;
            }
            Player emailReciever = _players.FirstOrDefault(p => p.Login == email.Target);
            if (emailReciever == null)
            {
                Server.SendToClient(_player.ClientSocket, new Message { Type = MessageType.Error, Text = $"Player does not exist! email failed." });
                return;
            }
            emailReciever.SaveEmail(email); // saving email to itself for now
            Server.SendToClient(_player.ClientSocket, new Message { Type = MessageType.Success, Text = $"Email sent!" });

        }
        private void ProcessGetTroops(string message)
        {
            TroopsMessage addTroopsMessage = JsonConvert.DeserializeObject<TroopsMessage>(message);
            if (addTroopsMessage == null) return;
            int cityIndex = addTroopsMessage.CityIndex;
            Army army = _player.GetCity(cityIndex).GetAssociatedArmy();
            _player.GetTroops(army);
        }

        private void ProcessGetGridAroundPlayer(string message)
        {
            GridMessage getGridAroundPlayer = JsonConvert.DeserializeObject<GridMessage>(message);
            if (getGridAroundPlayer == null) return;
            _player.GetGridAroundPlayer(getGridAroundPlayer);
        }

        private void ProcessDisconnect()
        {
            _player.ClientSocket.Close();
            _broadcastToAllExcept(_player.ClientSocket, new Message { Type = MessageType.Welcome, Text = $"Player {_player.Login} left the game." });
            _player.ClientSocket = null;
        }
    }
}