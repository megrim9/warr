using Newtonsoft.Json;
using System;
using Utils;

namespace WarriorsClient
{
    public class MessageHandler
    {
        public static void HandleMessage(string message)
        {
            try
            {
                var messageObj = JsonConvert.DeserializeObject<Message>(message);

                if (messageObj != null)
                {
                    switch (messageObj.Type)
                    {
                        case MessageType.Welcome:
                        case MessageType.Success:
                            Console.WriteLine(messageObj.Text);
                            break;
                        case MessageType.LoginAttempt:
                            var loginMessage = JsonConvert.DeserializeObject<LoginMessage>(message);
                            if (loginMessage.Status) 
                                Client.SetLoggedInAs(loginMessage.Login);
                            else
                                Client.SetLoggedInAs(null); 
                            break;
                        case MessageType.GetGridAroundPlayer:
                            var gridMessage = JsonConvert.DeserializeObject<GridMessage>(message);
                            GridManager.PrintGrid(gridMessage.Grid);
                            break;
                        case MessageType.GetResources:
                            var resourcesMessage = JsonConvert.DeserializeObject<ResourcesMessage>(message);
                            PrintResources(resourcesMessage);
                            break;
                        case MessageType.GetEmails:
                            var emailMessage = JsonConvert.DeserializeObject<EmailMessage>(message);
                            PrintEmails(emailMessage);
                            break;
                        case MessageType.GetTroops:
                            var TroopsMessage = JsonConvert.DeserializeObject<TroopsMessage>(message);
                            PrintTroops(TroopsMessage);
                            break;
                        case MessageType.Error:
                            var errorMessage = JsonConvert.DeserializeObject<Message>(message);
                            if (errorMessage != null)
                            {
                                Console.WriteLine($"Error: {errorMessage.Text}");
                            }
                            break;
                        default:
                            Console.WriteLine("Unknown message type." + message);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while handling received message: {ex.Message}");
            }
        }

        private static void PrintResources(ResourcesMessage resourcesMessage)
        {
            if (resourcesMessage != null)
            {
                Console.WriteLine($"Wood: {resourcesMessage.Wood}");
                Console.WriteLine($"Clay: {resourcesMessage.Clay}");
                Console.WriteLine($"Iron: {resourcesMessage.Iron}");
                Console.WriteLine($"Crop: {resourcesMessage.Crop}");
            }
        }

        private static void PrintEmails(EmailMessage resourcesMessage)
        {
            Console.WriteLine("--- Email list start ---");
            foreach (var email in resourcesMessage.Emails)
            {
                Console.WriteLine($"Sender: {email.Sender}     Subject: {email.Subject}     Body: {email.Body}     Date: {email.Date}.");
            }
            Console.WriteLine("--- Email list end   ---");
        }
        private static void PrintTroops(TroopsMessage resourcesMessage)
        {
            if (resourcesMessage != null)
            {
                Console.WriteLine($"Archers: {resourcesMessage.ArcherCount}");
                Console.WriteLine($"Knights: {resourcesMessage.KnightCount}");
                Console.WriteLine($"Pikeman: {resourcesMessage.PikemanCount}");
            }
        }
    }
}
