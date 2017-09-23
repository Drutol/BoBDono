﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BobDono.Entities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Database
{
    public class BobDatabaseContext : DbContext
    {
        public BobDatabaseContext()
        {
            
        }

        public DbSet<Bracket> Brackets { get; set; }
        public DbSet<Election> Elections { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Waifu> Waifus { get; set; }
        public DbSet<WaifuContender> WaifuContenders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=bob.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var modelType in GetClassesFromNamespace())
            {
                modelType.GetMethod("OnModelCreating").Invoke(null, new object[] { modelBuilder });
            }

            base.OnModelCreating(modelBuilder);
        }

        private IEnumerable<Type> GetClassesFromNamespace()
        {
            var @interface = typeof(IModelWithRelation);
            string @namespace = "BobDono.Entities";

            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && t.Namespace == @namespace && @interface.IsAssignableFrom(t));
        }

    }
}
