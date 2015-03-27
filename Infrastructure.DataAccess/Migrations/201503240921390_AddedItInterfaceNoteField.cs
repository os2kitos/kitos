namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedItInterfaceNoteField : DbMigration
    {
        public override void Up()
        {
            AddColumn("ItInterface", "Note", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("ItInterface", "Note");
        }
    }
}
