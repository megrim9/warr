using System;
using System.Net.Sockets;
using Utils;

namespace WarriorsServer
{
    public class Army
    {
        public int ArcherCount { get; private set; }
        public int PikemanCount { get; private set; }
        public int KnightCount { get; private set; }
        public Player Player;

        public Army(Player p)
        {
            this.Player = p;
        }
        public void SetArcherCount(int count) => ArcherCount = count;
        public void SetPikemanCount(int count) => PikemanCount = count;
        public void SetKnightCount(int count) => KnightCount = count;

        public Player GetOwner()
        {
            return this.Player;
        }

        public void AddTroops(int amount, TroopType troop)
        {
            if (amount <= 0)
            {
                Server.SendToClient(Player.ClientSocket, new Message
                {
                    Type = MessageType.Error,
                    Text = $"Army amount must be positive."
                });
            }
            switch (troop)
            {
                case TroopType.Archer:
                    ArcherCount += amount;
                    break;

                case TroopType.Pikeman:
                    PikemanCount += amount;
                    break;

                case TroopType.Knight:
                    KnightCount += amount;
                    break;

                default:
                    Server.SendToClient(Player.ClientSocket, new Message
                    {
                        Type = MessageType.Error,
                        Text = $"Troop type not recognized."
                    });
                    break;
            }
        }
 

    }


}