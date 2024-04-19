using System;

namespace Database_Projekt
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IRepository repository = new PostgresRepository();
            new UserRegistrationWithPattern(repository).RunLoop();

            Console.ReadLine();
        }
    }
}