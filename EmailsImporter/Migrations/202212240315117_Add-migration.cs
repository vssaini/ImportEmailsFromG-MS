namespace EmailsImporter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addmigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GoogleAuthItems",
                c => new
                    {
                        Key = c.String(nullable: false, maxLength: 100),
                        Value = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.Key);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.GoogleAuthItems");
        }
    }
}
