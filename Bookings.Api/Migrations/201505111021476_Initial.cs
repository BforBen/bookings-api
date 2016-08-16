namespace Bookings.Api.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Bookings",
                c => new
                    {
                        Id = c.Long(nullable: false),
                        InstanceId = c.Long(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Instances", t => t.InstanceId, cascadeDelete: true)
                .Index(t => t.InstanceId);
            
            CreateTable(
                "dbo.Instances",
                c => new
                    {
                        Id = c.Long(nullable: false),
                        ResourceId = c.Long(nullable: false),
                        Start = c.DateTime(nullable: false),
                        End = c.DateTime(nullable: false),
                        Capacity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Resources", t => t.ResourceId, cascadeDelete: true)
                .Index(t => t.ResourceId);
            
            CreateTable(
                "dbo.Resources",
                c => new
                    {
                        Id = c.Long(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Instances", "ResourceId", "dbo.Resources");
            DropForeignKey("dbo.Bookings", "InstanceId", "dbo.Instances");
            DropIndex("dbo.Instances", new[] { "ResourceId" });
            DropIndex("dbo.Bookings", new[] { "InstanceId" });
            DropTable("dbo.Resources");
            DropTable("dbo.Instances");
            DropTable("dbo.Bookings");
        }
    }
}
