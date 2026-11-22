using System;
using System.Linq;
using Infra;
using LinqToDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service;

namespace API;
 
public class DatabaseSeeder(AppDb db, PasswordHasher hasher, IConfiguration config, IHostEnvironment env, ILogger<DatabaseSeeder> logger)
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
        db.CreateTable<ApprovalAudit>(tableOptions: TableOptions.CreateIfNotExists);
    }
 
    public void SeedUsers()
    {
        // Kun under udvikling: Mette fra init.sql får en kendt testadgangskode, så man kan logge ind som medarbejder
        if (env.IsDevelopment())
        {
            var mette = db.Employees.FirstOrDefault(e => e.Email == "mette@businessesbjerg.com" && e.PasswordHash == "");
            if (mette is not null)
                db.Employees.Where(e => e.Id == mette.Id).Set(e => e.PasswordHash, hasher.Hash("mette123")).Update();
        }

        if (db.Employees.Any(e => e.Role == Roles.Admin))
            return;

        // Den første admin kommer fra konfigurationen (Seed:AdminEmail og Seed:AdminPassword), aldrig fra koden
        var email = config["Seed:AdminEmail"];
        var password = config["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("No admin exists and Seed:AdminEmail / Seed:AdminPassword is not set, so none was created.");
            return;
        }
 
        db.Insert(new Employee
        {
            Id = Guid.NewGuid(),
            Name = "admin",
            Email = email,
            PasswordHash = hasher.Hash(password),
            Role = Roles.Admin,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });
    }
}