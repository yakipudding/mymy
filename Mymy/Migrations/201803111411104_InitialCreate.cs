namespace Mymy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Projects",
                c => new
                    {
                        ProjectId = c.Int(nullable: false, identity: true),
                        ProjectName = c.String(),
                        TicketUrl = c.String(),
                        RssUrl = c.String(),
                        RssTicketUrl = c.String(),
                        Memo = c.String(),
                        DetailMemo = c.String(),
                    })
                .PrimaryKey(t => t.ProjectId);
            
            CreateTable(
                "dbo.Tickets",
                c => new
                    {
                        TicketId = c.Int(nullable: false, identity: true),
                        Link = c.String(),
                        Title = c.String(),
                        Date = c.DateTime(nullable: false),
                        Description = c.String(),
                        Creator = c.String(),
                        TracId = c.Int(nullable: false),
                        Category = c.String(),
                        Status = c.String(),
                        Status2 = c.String(),
                        Link2 = c.String(),
                        Memo = c.String(),
                        DetailMemo = c.String(),
                        Visible = c.Boolean(nullable: false),
                        Project_ProjectId = c.Int(),
                    })
                .PrimaryKey(t => t.TicketId)
                .ForeignKey("dbo.Projects", t => t.Project_ProjectId)
                .Index(t => t.Project_ProjectId);
            
            CreateTable(
                "dbo.Settings",
                c => new
                    {
                        SettingId = c.Int(nullable: false, identity: true),
                        Label = c.String(),
                        Memo = c.String(),
                    })
                .PrimaryKey(t => t.SettingId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tickets", "Project_ProjectId", "dbo.Projects");
            DropIndex("dbo.Tickets", new[] { "Project_ProjectId" });
            DropTable("dbo.Settings");
            DropTable("dbo.Tickets");
            DropTable("dbo.Projects");
        }
    }
}
