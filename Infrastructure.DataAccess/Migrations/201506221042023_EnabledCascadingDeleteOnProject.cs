namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EnabledCascadingDeleteOnProject : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("ItProject", "ParentId", "ItProject");
            DropForeignKey("ItProject", "CommonPublicProjectId", "ItProject");
            DropForeignKey("ItProject", "JointMunicipalProjectId", "ItProject");
            AddForeignKey("ItProject", "ParentId", "ItProject", "Id", cascadeDelete: true);
            AddForeignKey("ItProject", "CommonPublicProjectId", "ItProject", "Id", cascadeDelete: true);
            AddForeignKey("ItProject", "JointMunicipalProjectId", "ItProject", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("ItProject", "JointMunicipalProjectId", "ItProject");
            DropForeignKey("ItProject", "CommonPublicProjectId", "ItProject");
            DropForeignKey("ItProject", "ParentId", "ItProject");
            AddForeignKey("ItProject", "JointMunicipalProjectId", "ItProject", "Id");
            AddForeignKey("ItProject", "CommonPublicProjectId", "ItProject", "Id");
            AddForeignKey("ItProject", "ParentId", "ItProject", "Id");
        }
    }
}
