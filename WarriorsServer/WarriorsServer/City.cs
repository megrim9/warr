using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Utils;

namespace WarriorsServer
{
    public class City
    {
        private static readonly object attackLock = new object();
        public string Name { get; set; }
        public int X { get; set; } // Latitude
        public int Y { get; set; } // Longitude
        public Player Owner { get; }
        private Army AssociatedArmy { get; set; }
        private List<AttackingArmy> AttackingArmy = new();

        public City(string name, int x, int y, Player p)
        {
            Name = name;
            X = x;
            Y = y;
            Owner = p;
            AssociatedArmy = CreateAssociatedArmy(p);
            AssociatedArmy.AddTroops(5, TroopType.Pikeman); // each new city starts with 5 Pikeman by default
        }

        public Army GetAssociatedArmy()
        {
            return AssociatedArmy;
        }

        public List<AttackingArmy> GetAttackingArmies()
        {
            return AttackingArmy;
        }

        public Army CreateAssociatedArmy(Player owner)
        {
            return new Army(owner);
        }


        public bool PerformAttack(Army army, City cityAttacked)
        {
            lock (attackLock)
            {
                if (!CanPerformAttack(army))
                {
                    Server.SendToClient(Owner.ClientSocket, new Message
                    {
                        Type = MessageType.Error,
                        Text = $"Cannot perform attack. Insufficient army specified."
                    });
                    return false;
                }
                AttackingArmy attackingArmy = new AttackingArmy();
                attackingArmy.SetArcherCount(army.ArcherCount);
                attackingArmy.SetPikemanCount(army.PikemanCount);
                attackingArmy.SetKnightCount(army.KnightCount);
                attackingArmy.AttackStartTime = DateTime.UtcNow;
                attackingArmy.AttackTarget = cityAttacked;

                // send the army on the attack, subtract it from the city total army
                AssociatedArmy.SetArcherCount(AssociatedArmy.ArcherCount - army.ArcherCount);
                AssociatedArmy.SetPikemanCount(AssociatedArmy.PikemanCount - army.PikemanCount);
                AssociatedArmy.SetKnightCount(AssociatedArmy.KnightCount - army.KnightCount);
                AttackingArmy.Add(attackingArmy);

                return true;
            }
        }

        public bool CanPerformAttack(Army attackArmy)
        {
            return AssociatedArmy.ArcherCount >= attackArmy.ArcherCount
                && AssociatedArmy.PikemanCount >= attackArmy.PikemanCount
                && AssociatedArmy.KnightCount >= attackArmy.KnightCount;
        }

    }
}