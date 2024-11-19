using Utils;
using WarriorsServer;
using Xunit;

namespace Warriors.Test
{
    public class WarriorTests
    {
        public WarriorTests()
        {
            Map.InitializeMap();
        }

        [Fact]
        public void TestFightAlgorithm()
        {
            InitTestArmies(out Army army1, out Army army2);

            BattleService.ExecuteFight(army1, army2);

            Assert.True(army1.ArcherCount == 0);
            Assert.True(army1.KnightCount == 67);
            Assert.True(army1.PikemanCount == 0);
            Assert.True(army2.ArcherCount == 99);
        }

        private static List<Player> InitTestPlayers()
        {
            return new()
            {
                new Player("l1", "p1", 11222333, 7,9),
                new Player("l2", "p2", 22333444, 9,1)
            };
        }

        private static void InitTestArmies(out Army army1, out Army army2)
        {
            List<Player> _players = InitTestPlayers();
            City c1 = _players[0].GetCity(0);
            City c2 = _players[1].GetCity(0);
            army1 = c1.GetAssociatedArmy();
            army2 = c2.GetAssociatedArmy();
            army1.AddTroops(1, TroopType.Archer);
            army1.AddTroops(1, TroopType.Pikeman);
            army1.AddTroops(100, TroopType.Knight);
            army2.AddTroops(200, TroopType.Archer);
        }

        [Fact]
        public void CalcDistanceBetweenPlayerrs()
        {
            List<Player> _players = InitTestPlayers();
            City c1 = _players[0].GetCity(0);
            City c2 = _players[1].GetCity(0);

            (int xDistance, int yDistance, int distanceInSquares) = Utils.Utils.CalculateDistance(c1.X, c1.Y, c2.X, c2.Y);
            Assert.True(xDistance == 2);
            Assert.True(yDistance == 8);
            Assert.True(distanceInSquares == 10);
        }

        [Fact]
        public void TravelTroops()
        {
            InitTestArmies(out Army army1, out Army army2);
            Player p1 = army1.GetOwner();
            Player p2 = army2.GetOwner();

            p1.GetCity(0).PerformAttack(army1, p2.GetCity(0));
             
            Assert.True(p1.GetCity(0).GetAttackingArmies().Count == 1);
        }
    }
}