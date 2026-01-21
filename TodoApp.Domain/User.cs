using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoApp.Domain
{
    public class User(string name, string email, string passwordHash)
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; private set; } = name;
        public string Email { get; private set; } = email;
        public string PasswordHash { get; private set; } = passwordHash;
    }
}