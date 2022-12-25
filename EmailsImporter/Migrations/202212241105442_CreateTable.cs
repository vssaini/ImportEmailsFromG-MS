namespace EmailsImporter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GoogleAuthStores",
                c => new
                    {
                        Key = c.String(nullable: false, maxLength: 100),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.Key);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.GoogleAuthStores");
        }
    }
}
