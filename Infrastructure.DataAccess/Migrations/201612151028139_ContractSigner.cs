namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ContractSigner : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItContract", "ContractSignerId", "dbo.User");
            DropIndex("dbo.ItContract", new[] { "ContractSignerId" });
            AddColumn("dbo.ItContract", "ContractSigner", c => c.String());
            //Script der overfører brugerens navn til kontrakter contractsigner der erstatter contractsignerid, da feltet contractsigner skal kunne indeholde navne på ikke brugere også
            Sql("DECLARE @fName varchar(max), @lName varchar(max), @uId int\r\n\r\nDECLARE cursorName CURSOR\r\n\r\nLOCAL SCROLL STATIC\r\n\r\nFOR\r\n\r\nSELECT Name, LastName, Id FROM [dbo].[User]\r\n\r\nOPEN cursorName\r\n\r\nFETCH NEXT FROM cursorName\r\n\r\nINTO @fName, @lName, @uId\r\n\r\nWHILE @@FETCH_STATUS = 0\r\n\r\nBEGIN\r\n\r\nUPDATE [dbo].[ItContract]\r\n\r\nSET ContractSigner = @fName + \' \' + @lName\r\n\r\nWHERE ContractSignerId = @uId\r\n\r\nFETCH NEXT FROM cursorName\r\nINTO @fName, @lName, @uId\r\n\r\nEND\r\n\r\nCLOSE cursorName\r\n\r\nDEALLOCATE cursorName");
            DropColumn("dbo.ItContract", "ContractSignerId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItContract", "ContractSignerId", c => c.Int());
            DropColumn("dbo.ItContract", "ContractSigner");
            CreateIndex("dbo.ItContract", "ContractSignerId");
            AddForeignKey("dbo.ItContract", "ContractSignerId", "dbo.User", "Id");
        }
    }
}
