using System;
using System.Linq;
using Infra;
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
        db.CreateTable<Employee>(tableOptions: TableOptions.CreateIfNotExists);
        db.CreateTable<HolidayRequest>(tableOptions: TableOptions.CreateIfNotExists);
        db.CreateTable<DayOff>(tableOptions: TableOptions.CreateIfNotExists);
        db.CreateTable<LedgerEntry>(tableOptions: TableOptions.CreateIfNotExists);
        db.CreateTable<WorkSchedule>(tableOptions: TableOptions.CreateIfNotExists);
        db.CreateTable<RuleParameter>(tableOptions: TableOptions.CreateIfNotExists);
    }
 
    public void SeedUsers()
    {
        if (db.Employees.Any())
            return;
 
        db.Insert(new Employee
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