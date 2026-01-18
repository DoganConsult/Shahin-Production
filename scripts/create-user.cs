// Script to create a new user in GrcAuthDb
// Usage: dotnet script scripts/create-user.cs "FirstName" "LastName" "email@example.com" "Password123!"

using System;
using Microsoft.AspNetCore.Identity;
using GrcMvc.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using GrcMvc.Data;

// Get connection string from environment variable (REQUIRED)
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

if (string.IsNullOrEmpty(connectionString))
{
    Console.Error.WriteLine("ERROR: CONNECTION_STRING environment variable is required.");
    Console.Error.WriteLine("Please set the CONNECTION_STRING environment variable before running this script.");
    Console.Error.WriteLine("Example: export CONNECTION_STRING=\"Host=localhost;Database=GrcAuthDb;Username=postgres;Password=YourSecurePassword;Port=5432\"");
    Environment.Exit(1);
}

var args = Environment.GetCommandLineArgs();
if (args.Length < 5)
{
    Console.WriteLine("Usage: dotnet script create-user.cs \"FirstName\" \"LastName\" \"email@example.com\" \"Password123!\"");
    Environment.Exit(1);
}

var firstName = args[1];
var lastName = args[2];
var email = args[3];
var password = args[4];

// This would need to be run within the application context
// Better approach: Create a console command or use the application's seeding mechanism

Console.WriteLine($"To create user: {firstName} {lastName} ({email})");
Console.WriteLine("Please provide the names and I'll create a proper seed script or API endpoint.");
