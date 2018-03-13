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
            var project = new Project { ProjectId = 1, ProjectName = "プロジェクト", TicketUrl = "http://******/" };
            context.Projects.AddOrUpdate(project);

            var projectCustomField = new ProjectCustomField();
            projectCustomField.ProjectCustomFieldId = 1;
            projectCustomField.Project = project;
            projectCustomField.Field = "status2";
            projectCustomField.FieldJapaneseName = "状態";
            projectCustomField.Visible = true;
            context.ProjectCustomFields.AddOrUpdate(projectCustomField);

            var settings = new List<Setting>
            {
                new Setting{SettingId=1, Label="メモ",Memo=""}
            };
            settings.ForEach(s => context.Settings.AddOrUpdate(s));
            context.SaveChanges();
        }
    }
}
