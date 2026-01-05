using Microsoft.AspNetCore.Identity;
using System;

namespace GrcMvc
{
    public class PasswordHashGenerator
    {
        public static string GenerateHash(string password)
        {
            var hasher = new PasswordHasher<object>();
            return hasher.HashPassword(new object(), password);
        }

        public static void Main()
        {
            string password = "DogCon@Admin";
            string hash = GenerateHash(password);

            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"Hash: {hash}");
            Console.WriteLine();
            Console.WriteLine("Use this hash in SQL:");
            Console.WriteLine($"'{hash}'");
        }
    }
}