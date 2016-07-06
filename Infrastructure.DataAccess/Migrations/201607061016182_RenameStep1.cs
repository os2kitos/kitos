namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep1 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.InfUsage", new[] { "ItInterfaceId" });
            DropIndex("dbo.OptionExtends", new[] { "ObjectOwnerId" });
            DropIndex("dbo.OptionExtends", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PaymentFreqencies", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PaymentFreqencies", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PaymentModels", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PaymentModels", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PriceRegulations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PriceRegulations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.TerminationDeadlines", new[] { "ObjectOwnerId" });
            DropIndex("dbo.TerminationDeadlines", new[] { "LastChangedByUserId" });
            AlterColumn("dbo.OptionExtends", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.OptionExtends", "ObjectOwnerId", c => c.Int(nullable: false));
            AlterColumn("dbo.OptionExtends", "LastChangedByUserId", c => c.Int(nullable: false));
            AlterColumn("dbo.PaymentFreqencies", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.PaymentFreqencies", "ObjectOwnerId", c => c.Int(nullable: false));
            AlterColumn("dbo.PaymentFreqencies", "LastChangedByUserId", c => c.Int(nullable: false));
            AlterColumn("dbo.PaymentModels", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.PaymentModels", "ObjectOwnerId", c => c.Int(nullable: false));
            AlterColumn("dbo.PaymentModels", "LastChangedByUserId", c => c.Int(nullable: false));
            AlterColumn("dbo.PriceRegulations", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.PriceRegulations", "ObjectOwnerId", c => c.Int(nullable: false));
            AlterColumn("dbo.PriceRegulations", "LastChangedByUserId", c => c.Int(nullable: false));
            AlterColumn("dbo.TerminationDeadlines", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.TerminationDeadlines", "ObjectOwnerId", c => c.Int(nullable: false));
            AlterColumn("dbo.TerminationDeadlines", "LastChangedByUserId", c => c.Int(nullable: false));
            CreateIndex("dbo.OptionExtends", "ObjectOwnerId");
            CreateIndex("dbo.OptionExtends", "LastChangedByUserId");
            CreateIndex("dbo.PaymentFreqencies", "ObjectOwnerId");
            CreateIndex("dbo.PaymentFreqencies", "LastChangedByUserId");
            CreateIndex("dbo.PaymentModels", "ObjectOwnerId");
            CreateIndex("dbo.PaymentModels", "LastChangedByUserId");
            CreateIndex("dbo.PriceRegulations", "ObjectOwnerId");
            CreateIndex("dbo.PriceRegulations", "LastChangedByUserId");
            CreateIndex("dbo.TerminationDeadlines", "ObjectOwnerId");
            CreateIndex("dbo.TerminationDeadlines", "LastChangedByUserId");
        }

        public override void Down()
        {
            DropIndex("dbo.TerminationDeadlines", new[] { "LastChangedByUserId" });
            DropIndex("dbo.TerminationDeadlines", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PriceRegulations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PriceRegulations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PaymentModels", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PaymentModels", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PaymentFreqencies", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PaymentFreqencies", new[] { "ObjectOwnerId" });
            DropIndex("dbo.OptionExtends", new[] { "LastChangedByUserId" });
            DropIndex("dbo.OptionExtends", new[] { "ObjectOwnerId" });
            AlterColumn("dbo.TerminationDeadlines", "LastChangedByUserId", c => c.Int());
            AlterColumn("dbo.TerminationDeadlines", "ObjectOwnerId", c => c.Int());
            AlterColumn("dbo.TerminationDeadlines", "Name", c => c.String());
            AlterColumn("dbo.PriceRegulations", "LastChangedByUserId", c => c.Int());
            AlterColumn("dbo.PriceRegulations", "ObjectOwnerId", c => c.Int());
            AlterColumn("dbo.PriceRegulations", "Name", c => c.String());
            AlterColumn("dbo.PaymentModels", "LastChangedByUserId", c => c.Int());
            AlterColumn("dbo.PaymentModels", "ObjectOwnerId", c => c.Int());
            AlterColumn("dbo.PaymentModels", "Name", c => c.String());
            AlterColumn("dbo.PaymentFreqencies", "LastChangedByUserId", c => c.Int());
            AlterColumn("dbo.PaymentFreqencies", "ObjectOwnerId", c => c.Int());
            AlterColumn("dbo.PaymentFreqencies", "Name", c => c.String());
            AlterColumn("dbo.OptionExtends", "LastChangedByUserId", c => c.Int());
            AlterColumn("dbo.OptionExtends", "ObjectOwnerId", c => c.Int());
            AlterColumn("dbo.OptionExtends", "Name", c => c.String());
            CreateIndex("dbo.TerminationDeadlines", "LastChangedByUserId");
            CreateIndex("dbo.TerminationDeadlines", "ObjectOwnerId");
            CreateIndex("dbo.PriceRegulations", "LastChangedByUserId");
            CreateIndex("dbo.PriceRegulations", "ObjectOwnerId");
            CreateIndex("dbo.PaymentModels", "LastChangedByUserId");
            CreateIndex("dbo.PaymentModels", "ObjectOwnerId");
            CreateIndex("dbo.PaymentFreqencies", "LastChangedByUserId");
            CreateIndex("dbo.PaymentFreqencies", "ObjectOwnerId");
            CreateIndex("dbo.OptionExtends", "LastChangedByUserId");
            CreateIndex("dbo.OptionExtends", "ObjectOwnerId");
            CreateIndex("dbo.InfUsage", "ItInterfaceId");
        }
    }
}
