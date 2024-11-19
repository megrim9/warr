using System;
using System.Collections.Generic;

namespace Utils
{
    public static class Utils
    {

        // ---- random algorithms here ---------

        public static (int xDistance,int yDistance,int distanceInSquares) CalculateDistance(int city1X, int city1Y, int city2X, int city2Y)
        {
            // Calculate Manhattan distance 
            int xDistance = Math.Abs(city2X - city1X);
            int yDistance = Math.Abs(city2Y - city1Y);
            int distanceInSquares = xDistance + yDistance;

            return (xDistance, yDistance, distanceInSquares);
        }
        public static int GetIntInput()
        {
            int value;
            while (!int.TryParse(Console.ReadLine(), out value))
            {
                Console.WriteLine("Invalid input. Please enter an integer value.");
            }
            return value;
        }

    }
    public class Message
    {
        public MessageType Type { get; set; }
        public string Text { get; set; }
    }

    public class EmailMessage : Message
    {
        public List<Email> Emails { get; set; }
    }

    public class ResourcesMessage : Message
    {
        public int Wood { get; set; }
        public int Clay { get; set; }
        public int Iron { get; set; }
        public int Crop { get; set; }
    }


    public class TroopsMessage : Message
    {
        public int Amount { get; set; }
        public int CityIndex { get; set; }
        public TroopType TroopType;
        public int ArcherCount { get; set; }
        public int KnightCount { get; set; }
        public int PikemanCount { get; set; }
    }

    // to retrieve the grid around the player
        public class GridMessage : Message
    {
        public TileType[,] Grid { get; set; }
        public int CityIndex { get; set; }
}

    public class LoginMessage : Message
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public bool Status { get; set; }
    }
    public class RegisterMessage : Message
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
    public class UpgradeMessage : Message
    {
        public BuildingType BuildingType { get; set; }
    } 
    public class AttackMessage : Message
    {
        public string OwnCityName { get; set; }
        public string TargetCityName { get; set; }
    }

    public enum BuildingType
    {
        WoodFarm,
        ClayFarm,
        IronFarm,
        CropFarm
    }
    public enum MessageType
    {
        Success,
        Welcome,
        LoginAttempt,
        AddResources,
        AddTroops,
        Upgrade,
        Attack,
        Login,
        Register,
        RemoveResources,
        GetEmails,
        SendEmail,
        GetResources,
        GetTroops,
        GetGridAroundPlayer,
        Error,
        Disconnect
    }

    public enum TileType
    {
        Grass,
        Water,
        Mountain,
        Forest,
        City
    }
    public enum TroopType
    {
        Knight,
        Pikeman,
        Archer
    }

    public interface ITroop
    {
        int InstanceHealth { get; }
        int InstanceDamage { get; }
        int InstanceSpeed { get; }
        ArmorType InstanceArmorType { get; }
    }

    public abstract class Troop : ITroop
    {
        public abstract int InstanceHealth { get; }
        public abstract int InstanceDamage { get; }
        public abstract int InstanceSpeed { get; }
        public abstract ArmorType InstanceArmorType { get; }
    }

    public class Knight : Troop
    {
        private static readonly Knight instance = new Knight();
        public static int Health => instance.InstanceHealth;
        public static int Damage => instance.InstanceDamage;
        public static int Speed => instance.InstanceSpeed;
        public static ArmorType ArmorType => instance.InstanceArmorType;

        public override int InstanceHealth => 60;
        public override int InstanceDamage => 20;
        public override int InstanceSpeed => 8;
        public override ArmorType InstanceArmorType => ArmorType.Mounted;
    }

    public class Pikeman : Troop
    {
        private static readonly Pikeman instance = new Pikeman();
        public static int Health => instance.InstanceHealth;
        public static int Damage => instance.InstanceDamage;
        public static int Speed => instance.InstanceSpeed;
        public static ArmorType ArmorType => instance.InstanceArmorType;

        public override int InstanceHealth => 80;
        public override int InstanceDamage => 15;
        public override int InstanceSpeed => 3;
        public override ArmorType InstanceArmorType => ArmorType.Heavy;
    }

    public class Archer : Troop
    {
        private static readonly Archer instance = new Archer();
        public static int Health => instance.InstanceHealth;
        public static int Damage => instance.InstanceDamage;
        public static int Speed => instance.InstanceSpeed;
        public static ArmorType ArmorType => instance.InstanceArmorType;

        public override int InstanceHealth => 40;
        public override int InstanceDamage => 20;
        public override int InstanceSpeed => 5;
        public override ArmorType InstanceArmorType => ArmorType.Light;
    }

    public enum ArmorType
    {
        Heavy = 0,
        Light = 1,
        Mounted = 2,
    }
}
