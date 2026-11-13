using System;
using System.Linq;
using Infra;
using Infra.Entities;
using LinqToDB;
using Service;

namespace API;
 
public class DatabaseSeeder(AppDb db, PasswordHasher hasher)
{
    public void Seed()
    {
        CreateTables();
        SeedUsers();
    }
  
    public void CreateTables()
    {
        db.CreateTable<User>(tableOptions: TableOptions.CreateIfNotExists);
    }
 
    public void SeedUsers()
    {
        if (db.Users.Any())
            return;
 
        db.Insert(new User
        {
            Id = Guid.NewGuid(),
            Name = "admin",
            Email = "admin@example.dk",
            PasswordHash = hasher.Hash("admin123"),
            Role = Roles.Admin,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });
    }
}