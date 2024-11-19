using System;

namespace WarriorsServer
{
    public class AttackingArmy
    {
        public City AttackTarget { get; set; }
        public DateTime AttackStartTime { get; set; }

        public int ArcherCount { get; private set; }
        public int PikemanCount { get; private set; }
        public int KnightCount { get; private set; }
        public void SetArcherCount(int count) => ArcherCount = count;
        public void SetPikemanCount(int count) => PikemanCount = count;
        public void SetKnightCount(int count) => KnightCount = count;
    }
}