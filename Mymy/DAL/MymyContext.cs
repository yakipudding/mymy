using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Mymy.Models;

namespace Mymy.DAL
{
    public class MymyContext : DbContext
    {
        public MymyContext() : base("MymyContext")
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectCustomField> ProjectCustomFields { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
    }
}