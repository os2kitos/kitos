namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedMaxLengthToItInterfaceVersion : DbMigration
    {
        public override void Up()
        {
            AlterColumn("ItInterface", "Version", c => c.String(maxLength: 20, storeType: "nvarchar"));
        }
        
        public override void Down()
        {
            AlterColumn("ItInterface", "Version", c => c.String(unicode: false));
        }
    }
}
