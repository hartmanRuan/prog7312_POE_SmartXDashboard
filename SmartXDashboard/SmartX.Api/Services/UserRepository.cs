using Microsoft.AspNetCore.Identity;
using SmartX.Api.Models;
using System.Collections.Generic;
using System.Linq;

namespace SmartX.Api.Services
{
    public static class UserRepository
    {
        private static readonly PasswordHasher<User> _hasher = new();

        public static List<User> Users { get; set; } = new()
        {
            CreateUser("admin", "password123", "Administrator"),
            CreateUser("ruan", "securepassword", "Operator")
        };

        private static User CreateUser(string username, string plainPassword, string role)
        {
            var user = new User { Username = username };
            user.PasswordHash = _hasher.HashPassword(user, plainPassword);
            return user;
        }

        public static bool VerifyUser(string username, string plainPassword, out User matchedUser)
        {
            matchedUser = Users.FirstOrDefault(u => u.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase));
            if (matchedUser == null) return false;

            var result = _hasher.VerifyHashedPassword(matchedUser, matchedUser.PasswordHash, plainPassword);
            return result == PasswordVerificationResult.Success;
        }

        public static void AddUser(string username, string plainPassword)
        {
            var user = new User { Username = username };
            user.PasswordHash = _hasher.HashPassword(user, plainPassword);
            Users.Add(user);
        }
    }
}