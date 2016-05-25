namespace IdeaScroll_Backend.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class date : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Ideas", "Created", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Ideas", "LastEdited", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Ideas", "LastEdited");
            DropColumn("dbo.Ideas", "Created");
        }
    }
}
