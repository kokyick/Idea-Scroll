namespace IdeaScroll_Backend.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Galleries", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Galleries", "Name");
        }
    }
}
