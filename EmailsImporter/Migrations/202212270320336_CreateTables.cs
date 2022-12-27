namespace EmailsImporter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GoogleAuthStores",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 100),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.MicrosoftAuthStores",
                c => new
                    {
                        UserId = c.Guid(nullable: false),
                        MsAccessToken = c.String(),
                        MsRefreshToken = c.String(),
                        MsAccessTokenExpiresAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.MicrosoftAuthStores");
            DropTable("dbo.GoogleAuthStores");
        }
    }
}
