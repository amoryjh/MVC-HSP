namespace HPSMVC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VIEWER : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.File", "Viewer", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.File", "Viewer");
        }
    }
}
