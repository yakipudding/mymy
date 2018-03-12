namespace Mymy.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Collections.Generic;
    using Mymy.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<Mymy.DAL.MymyContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "Mymy.DAL.MymyContext";
        }

        protected override void Seed(Mymy.DAL.MymyContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
            var projects = new List<Project>
            {
                new Project{ProjectId=1, ProjectName="プロジェクト", TicketUrl="http://******/"},
                new Project{ProjectId=2, ProjectName="プロジェクト2", TicketUrl="http://******/"}
            };
            projects.ForEach(s => context.Projects.Add(s));
            context.SaveChanges();

            var settings = new List<Setting>
            {
                new Setting{SettingId=1, Label="メモ",Memo=""}
            };
            settings.ForEach(s => context.Settings.Add(s));
            context.SaveChanges();
        }
    }
}
