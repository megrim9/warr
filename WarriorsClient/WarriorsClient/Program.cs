using System;

namespace WarriorsClient
{
    public class Program
    {
        public static void Main()
        {
            Client client = new Client();
            client.Connect();

            while(true)
                Console.ReadLine();
        }
    }
}