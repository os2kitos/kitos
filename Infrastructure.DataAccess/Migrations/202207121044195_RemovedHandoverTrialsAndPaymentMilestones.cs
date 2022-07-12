namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedHandoverTrialsAndPaymentMilestones : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.HandoverTrialTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.HandoverTrialTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.HandoverTrial", "HandoverTrialTypeId", "dbo.HandoverTrialTypes");
            DropForeignKey("dbo.HandoverTrial", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.HandoverTrial", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.HandoverTrial", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.PaymentMilestones", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.PaymentMilestones", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.PaymentMilestones", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalHandoverTrialTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalHandoverTrialTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalHandoverTrialTypes", "OrganizationId", "dbo.Organization");
            DropIndex("dbo.HandoverTrial", new[] { "ItContractId" });
            DropIndex("dbo.HandoverTrial", new[] { "HandoverTrialTypeId" });
            DropIndex("dbo.HandoverTrial", new[] { "ObjectOwnerId" });
            DropIndex("dbo.HandoverTrial", new[] { "LastChangedByUserId" });
            DropIndex("dbo.HandoverTrialTypes", "UX_Option_Uuid");
            DropIndex("dbo.HandoverTrialTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.HandoverTrialTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PaymentMilestones", new[] { "ItContractId" });
            DropIndex("dbo.PaymentMilestones", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PaymentMilestones", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalHandoverTrialTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalHandoverTrialTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalHandoverTrialTypes", new[] { "LastChangedByUserId" });
            DropTable("dbo.HandoverTrial");
            DropTable("dbo.HandoverTrialTypes");
            DropTable("dbo.PaymentMilestones");
            DropTable("dbo.LocalHandoverTrialTypes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.LocalHandoverTrialTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PaymentMilestones",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Expected = c.DateTime(precision: 7, storeType: "datetime2"),
                        Approved = c.DateTime(precision: 7, storeType: "datetime2"),
                        ItContractId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.HandoverTrialTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 150),
                        IsLocallyAvailable = c.Boolean(nullable: false),
                        IsObligatory = c.Boolean(nullable: false),
                        Description = c.String(),
                        IsEnabled = c.Boolean(nullable: false),
                        Priority = c.Int(nullable: false),
                        Uuid = c.Guid(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.HandoverTrial",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Expected = c.DateTime(precision: 7, storeType: "datetime2"),
                        Approved = c.DateTime(precision: 7, storeType: "datetime2"),
                        ItContractId = c.Int(nullable: false),
                        HandoverTrialTypeId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.LocalHandoverTrialTypes", "LastChangedByUserId");
            CreateIndex("dbo.LocalHandoverTrialTypes", "ObjectOwnerId");
            CreateIndex("dbo.LocalHandoverTrialTypes", "OrganizationId");
            CreateIndex("dbo.PaymentMilestones", "LastChangedByUserId");
            CreateIndex("dbo.PaymentMilestones", "ObjectOwnerId");
            CreateIndex("dbo.PaymentMilestones", "ItContractId");
            CreateIndex("dbo.HandoverTrialTypes", "LastChangedByUserId");
            CreateIndex("dbo.HandoverTrialTypes", "ObjectOwnerId");
            CreateIndex("dbo.HandoverTrialTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.HandoverTrial", "LastChangedByUserId");
            CreateIndex("dbo.HandoverTrial", "ObjectOwnerId");
            CreateIndex("dbo.HandoverTrial", "HandoverTrialTypeId");
            CreateIndex("dbo.HandoverTrial", "ItContractId");
            AddForeignKey("dbo.LocalHandoverTrialTypes", "OrganizationId", "dbo.Organization", "Id", cascadeDelete: true);
            AddForeignKey("dbo.LocalHandoverTrialTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.LocalHandoverTrialTypes", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.PaymentMilestones", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.PaymentMilestones", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.PaymentMilestones", "ItContractId", "dbo.ItContract", "Id", cascadeDelete: true);
            AddForeignKey("dbo.HandoverTrial", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.HandoverTrial", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.HandoverTrial", "ItContractId", "dbo.ItContract", "Id", cascadeDelete: true);
            AddForeignKey("dbo.HandoverTrial", "HandoverTrialTypeId", "dbo.HandoverTrialTypes", "Id");
            AddForeignKey("dbo.HandoverTrialTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.HandoverTrialTypes", "LastChangedByUserId", "dbo.User", "Id");
        }
    }
}
