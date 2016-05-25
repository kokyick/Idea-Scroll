namespace IdeaScroll_Backend.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Pictures",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FileName = c.String(),
                        FileUrl = c.String(),
                        FileSizeInBytes = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Galleries", "Visibility", c => c.Boolean(nullable: false));
            AddColumn("dbo.Ideas", "visible", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Ideas", "visible");
            DropColumn("dbo.Galleries", "Visibility");
            DropTable("dbo.Pictures");
        }
    }
}
