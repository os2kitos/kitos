namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdminRight",
                c => new
                    {
                        ObjectId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ObjectId, t.RoleId, t.UserId })
                .ForeignKey("dbo.Organization", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("dbo.AdminRole", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ObjectId)
                .Index(t => t.RoleId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Organization",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        Type = c.Int(),
                        Cvr = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItSystem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Version = c.String(unicode: false),
                        Name = c.String(unicode: false),
                        SystemId = c.String(unicode: false),
                        ExposedById = c.Int(),
                        BelongsToId = c.Int(nullable: false),
                        OrganizationId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        AccessModifier = c.Int(nullable: false),
                        Description = c.String(unicode: false),
                        Url = c.String(unicode: false),
                        ParentId = c.Int(),
                        AppTypeId = c.Int(nullable: false),
                        BusinessTypeId = c.Int(nullable: false),
                        InterfaceId = c.Int(),
                        InterfaceTypeId = c.Int(),
                        TsaId = c.Int(),
                        MethodId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AppType", t => t.AppTypeId, cascadeDelete: true)
                .ForeignKey("dbo.Organization", t => t.BelongsToId, cascadeDelete: true)
                .ForeignKey("dbo.BusinessType", t => t.BusinessTypeId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.ExposedById)
                .ForeignKey("dbo.Interface", t => t.InterfaceId)
                .ForeignKey("dbo.InterfaceType", t => t.InterfaceTypeId)
                .ForeignKey("dbo.Method", t => t.MethodId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.ParentId)
                .ForeignKey("dbo.Tsa", t => t.TsaId)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ExposedById)
                .Index(t => t.BelongsToId)
                .Index(t => t.OrganizationId)
                .Index(t => t.UserId)
                .Index(t => t.ParentId)
                .Index(t => t.AppTypeId)
                .Index(t => t.BusinessTypeId)
                .Index(t => t.InterfaceId)
                .Index(t => t.InterfaceTypeId)
                .Index(t => t.TsaId)
                .Index(t => t.MethodId);
            
            CreateTable(
                "dbo.AppType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BusinessType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DataRows",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystemId = c.Int(nullable: false),
                        DataTypeId = c.Int(nullable: false),
                        Data = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DataType", t => t.DataTypeId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .Index(t => t.ItSystemId)
                .Index(t => t.DataTypeId);
            
            CreateTable(
                "dbo.DataType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Interface",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InterfaceType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Method",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItSystemUsage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsStatusActive = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        LocalSystemId = c.String(unicode: false),
                        EsdhRef = c.String(unicode: false),
                        CmdbRef = c.String(unicode: false),
                        DirectoryOrUrlRef = c.String(unicode: false),
                        AdOrIdmRef = c.String(unicode: false),
                        ResponsibleUnitId = c.Int(),
                        OrganizationId = c.Int(nullable: false),
                        ItSystemId = c.Int(nullable: false),
                        ArchiveTypeId = c.Int(),
                        SensitiveDataTypeId = c.Int(),
                        OverviewItSystemId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ArchiveType", t => t.ArchiveTypeId)
                .ForeignKey("dbo.ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.OverviewItSystemId)
                .ForeignKey("dbo.OrganizationUnit", t => t.ResponsibleUnitId)
                .ForeignKey("dbo.SensitiveDataType", t => t.SensitiveDataTypeId)
                .Index(t => t.ResponsibleUnitId)
                .Index(t => t.OrganizationId)
                .Index(t => t.ItSystemId)
                .Index(t => t.ArchiveTypeId)
                .Index(t => t.SensitiveDataTypeId)
                .Index(t => t.OverviewItSystemId);
            
            CreateTable(
                "dbo.ArchiveType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItContract",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ContractTypeId = c.Int(nullable: false),
                        ContractTemplateId = c.Int(nullable: false),
                        PurchaseFormId = c.Int(nullable: false),
                        PaymentModelId = c.Int(nullable: false),
                        SupplierId = c.Int(nullable: false),
                        MunicipalityId = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(),
                        HandoverTrial_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ContractTemplate", t => t.ContractTemplateId, cascadeDelete: true)
                .ForeignKey("dbo.ContractType", t => t.ContractTypeId, cascadeDelete: true)
                .ForeignKey("dbo.Organization", t => t.MunicipalityId)
                .ForeignKey("dbo.PaymentModel", t => t.PaymentModelId, cascadeDelete: true)
                .ForeignKey("dbo.PurchaseForm", t => t.PurchaseFormId, cascadeDelete: true)
                .ForeignKey("dbo.Supplier", t => t.SupplierId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id)
                .ForeignKey("dbo.HandoverTrial", t => t.HandoverTrial_Id)
                .Index(t => t.ContractTypeId)
                .Index(t => t.ContractTemplateId)
                .Index(t => t.PurchaseFormId)
                .Index(t => t.PaymentModelId)
                .Index(t => t.SupplierId)
                .Index(t => t.MunicipalityId)
                .Index(t => t.ItSystemUsage_Id)
                .Index(t => t.HandoverTrial_Id);
            
            CreateTable(
                "dbo.Agreement",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        AgreementElement_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContract", t => t.Id)
                .ForeignKey("dbo.AgreementElement", t => t.AgreementElement_Id)
                .Index(t => t.Id)
                .Index(t => t.AgreementElement_Id);
            
            CreateTable(
                "dbo.ContractTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ContractType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Payment",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContract", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.PaymentModel",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PurchaseForm",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItContractRight",
                c => new
                    {
                        ObjectId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ObjectId, t.RoleId, t.UserId })
                .ForeignKey("dbo.ItContract", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("dbo.ItContractRole", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ObjectId)
                .Index(t => t.RoleId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ItContractRole",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        Email = c.String(nullable: false, unicode: false),
                        Password = c.String(nullable: false, unicode: false),
                        Salt = c.String(nullable: false, unicode: false),
                        IsGlobalAdmin = c.Boolean(nullable: false),
                        DefaultOrganizationUnitId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.OrganizationUnit", t => t.DefaultOrganizationUnitId)
                .Index(t => t.DefaultOrganizationUnitId);
            
            CreateTable(
                "dbo.OrganizationUnit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        ParentId = c.Int(),
                        OrganizationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("dbo.OrganizationUnit", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.ParentId)
                .Index(t => t.OrganizationId);
            
            CreateTable(
                "dbo.Infrastructure",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HostId = c.Int(nullable: false),
                        SupplierId = c.Int(nullable: false),
                        DepartmentId = c.Int(nullable: false),
                        ItSystem_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id)
                .ForeignKey("dbo.OrganizationUnit", t => t.DepartmentId, cascadeDelete: true)
                .ForeignKey("dbo.Supplier", t => t.SupplierId, cascadeDelete: true)
                .Index(t => t.SupplierId)
                .Index(t => t.DepartmentId)
                .Index(t => t.ItSystem_Id);
            
            CreateTable(
                "dbo.Supplier",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TaskRef",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsPublic = c.Boolean(nullable: false),
                        Uuid = c.Guid(nullable: false),
                        Type = c.String(unicode: false),
                        TaskKey = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        ActiveFrom = c.DateTime(precision: 0),
                        ActiveTo = c.DateTime(precision: 0),
                        ItProjectId = c.Int(),
                        ItSystemId = c.Int(),
                        ParentId = c.Int(),
                        OwnedByOrganizationUnitId = c.Int(nullable: false),
                        OrganizationUnit_Id = c.Int(),
                        ItSystemUsage_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId)
                .ForeignKey("dbo.OrganizationUnit", t => t.OwnedByOrganizationUnitId, cascadeDelete: true)
                .ForeignKey("dbo.TaskRef", t => t.ParentId)
                .ForeignKey("dbo.OrganizationUnit", t => t.OrganizationUnit_Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id)
                .Index(t => t.ItProjectId)
                .Index(t => t.ParentId)
                .Index(t => t.OwnedByOrganizationUnitId)
                .Index(t => t.OrganizationUnit_Id)
                .Index(t => t.ItSystemUsage_Id);
            
            CreateTable(
                "dbo.ItProject",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Background = c.String(nullable: false, unicode: false),
                        IsTransversal = c.Boolean(nullable: false),
                        IsTermsOfReferenceApproved = c.Boolean(nullable: false),
                        Note = c.String(nullable: false, unicode: false),
                        Name = c.String(nullable: false, unicode: false),
                        ProjectTypeId = c.Int(nullable: false),
                        ProjectCategoryId = c.Int(nullable: false),
                        MunicipalityId = c.Int(nullable: false),
                        ItProjectOwnerId = c.Int(),
                        ItProjectLeaderId = c.Int(),
                        PartItProjectLeaderId = c.Int(),
                        ConsultantId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organization", t => t.MunicipalityId)
                .ForeignKey("dbo.ProjectCategory", t => t.ProjectCategoryId, cascadeDelete: true)
                .ForeignKey("dbo.ProjectType", t => t.ProjectTypeId, cascadeDelete: true)
                .Index(t => t.ProjectTypeId)
                .Index(t => t.ProjectCategoryId)
                .Index(t => t.MunicipalityId);
            
            CreateTable(
                "dbo.Communication",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProjectId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId, cascadeDelete: true)
                .Index(t => t.ItProjectId);
            
            CreateTable(
                "dbo.Economy",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProjectId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId, cascadeDelete: true)
                .Index(t => t.ItProjectId);
            
            CreateTable(
                "dbo.ExtReference",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(nullable: false, unicode: false),
                        ItProjectId = c.Int(nullable: false),
                        ExtReferenceTypeId = c.Int(nullable: false),
                        ItSystemId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ExtReferenceType", t => t.ExtReferenceTypeId, cascadeDelete: true)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .Index(t => t.ItProjectId)
                .Index(t => t.ExtReferenceTypeId)
                .Index(t => t.ItSystemId);
            
            CreateTable(
                "dbo.ExtReferenceType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ExtRefTypeLocale",
                c => new
                    {
                        MunicipalityId = c.Int(nullable: false),
                        OriginalId = c.Int(nullable: false),
                        Name = c.String(unicode: false),
                    })
                .PrimaryKey(t => new { t.MunicipalityId, t.OriginalId })
                .ForeignKey("dbo.Organization", t => t.MunicipalityId, cascadeDelete: true)
                .ForeignKey("dbo.ExtReferenceType", t => t.OriginalId, cascadeDelete: true)
                .Index(t => t.MunicipalityId)
                .Index(t => t.OriginalId);
            
            CreateTable(
                "dbo.GoalStatus",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Goal",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GoalStatusId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GoalStatus", t => t.GoalStatusId, cascadeDelete: true)
                .Index(t => t.GoalStatusId);
            
            CreateTable(
                "dbo.Handover",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Hierarchy",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ItProgramRef_Id = c.Int(),
                        ItProjectRef_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.ItProgramRef_Id)
                .ForeignKey("dbo.ItProject", t => t.Id, cascadeDelete: true)
                .ForeignKey("dbo.ItProject", t => t.ItProjectRef_Id)
                .Index(t => t.Id)
                .Index(t => t.ItProgramRef_Id)
                .Index(t => t.ItProjectRef_Id);
            
            CreateTable(
                "dbo.OrgTab",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.PreAnalysis",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.ProjectCategory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProjectStatus",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ProjectPhaseId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.Id)
                .ForeignKey("dbo.ProjectPhase", t => t.ProjectPhaseId)
                .Index(t => t.Id)
                .Index(t => t.ProjectPhaseId);
            
            CreateTable(
                "dbo.Milestone",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProjectStatusId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProjectStatus", t => t.ProjectStatusId, cascadeDelete: true)
                .Index(t => t.ProjectStatusId);
            
            CreateTable(
                "dbo.ProjectPhase",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProjectPhaseLocale",
                c => new
                    {
                        MunicipalityId = c.Int(nullable: false),
                        OriginalId = c.Int(nullable: false),
                        Name = c.String(unicode: false),
                    })
                .PrimaryKey(t => new { t.MunicipalityId, t.OriginalId })
                .ForeignKey("dbo.Organization", t => t.MunicipalityId, cascadeDelete: true)
                .ForeignKey("dbo.ProjectPhase", t => t.OriginalId, cascadeDelete: true)
                .Index(t => t.MunicipalityId)
                .Index(t => t.OriginalId);
            
            CreateTable(
                "dbo.ProjectType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Resource",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProjectId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId, cascadeDelete: true)
                .Index(t => t.ItProjectId);
            
            CreateTable(
                "dbo.ItProjectRight",
                c => new
                    {
                        ObjectId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ObjectId, t.RoleId, t.UserId })
                .ForeignKey("dbo.ItProject", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("dbo.ItProjectRole", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ObjectId)
                .Index(t => t.RoleId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ItProjectRole",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Risk",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProjectId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId, cascadeDelete: true)
                .Index(t => t.ItProjectId);
            
            CreateTable(
                "dbo.Stakeholder",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProjectId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId, cascadeDelete: true)
                .Index(t => t.ItProjectId);
            
            CreateTable(
                "dbo.TaskUsage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TaskRefId = c.Int(nullable: false),
                        OrgUnitId = c.Int(nullable: false),
                        Starred = c.Boolean(nullable: false),
                        TechnologyStatus = c.Int(nullable: false),
                        UsageStatus = c.Int(nullable: false),
                        Comment = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.OrganizationUnit", t => t.OrgUnitId, cascadeDelete: true)
                .ForeignKey("dbo.TaskRef", t => t.TaskRefId, cascadeDelete: true)
                .Index(t => t.TaskRefId)
                .Index(t => t.OrgUnitId);
            
            CreateTable(
                "dbo.OrganizationRight",
                c => new
                    {
                        ObjectId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ObjectId, t.RoleId, t.UserId })
                .ForeignKey("dbo.OrganizationUnit", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("dbo.OrganizationRole", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ObjectId)
                .Index(t => t.RoleId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.OrganizationRole",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PasswordResetRequest",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Time = c.DateTime(nullable: false, precision: 0),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ItSystemRight",
                c => new
                    {
                        ObjectId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ObjectId, t.RoleId, t.UserId })
                .ForeignKey("dbo.ItSystem", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemRole", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ObjectId)
                .Index(t => t.RoleId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ItSystemRole",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ItSystemUsage_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id)
                .Index(t => t.ItSystemUsage_Id);
            
            CreateTable(
                "dbo.ShipNotice",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AlarmDate = c.String(nullable: false, unicode: false),
                        To = c.String(nullable: false, unicode: false),
                        Cc = c.String(nullable: false, unicode: false),
                        Subject = c.String(nullable: false, unicode: false),
                        ItContractId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContract", t => t.ItContractId, cascadeDelete: true)
                .Index(t => t.ItContractId);
            
            CreateTable(
                "dbo.SensitiveDataType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Wish",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(unicode: false),
                        ItSystem_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id)
                .Index(t => t.ItSystem_Id)
                .Index(t => t.User_Id)
                .Index(t => t.ItSystemUsage_Id);
            
            CreateTable(
                "dbo.Tsa",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Config",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ShowItProjectModule = c.Boolean(nullable: false),
                        ShowItSystemModule = c.Boolean(nullable: false),
                        ShowItContractModule = c.Boolean(nullable: false),
                        ItSupportModuleNameId = c.Int(nullable: false),
                        ItSupportGuide = c.String(unicode: false),
                        ShowTabOverview = c.Boolean(nullable: false),
                        ShowColumnTechnology = c.Boolean(nullable: false),
                        ShowColumnUsage = c.Boolean(nullable: false),
                        ShowColumnMandatory = c.Boolean(nullable: false),
                        ItProjectModuleNameId = c.Int(nullable: false),
                        ItProjectGuide = c.String(unicode: false),
                        ShowPortfolio = c.Boolean(nullable: false),
                        ShowBC = c.Boolean(nullable: false),
                        ItSystemModuleNameId = c.Int(nullable: false),
                        ItSystemGuide = c.String(unicode: false),
                        ItContractModuleNameId = c.Int(nullable: false),
                        ItContractGuide = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContractName", t => t.ItContractModuleNameId, cascadeDelete: true)
                .ForeignKey("dbo.ItProjectName", t => t.ItProjectModuleNameId, cascadeDelete: true)
                .ForeignKey("dbo.ItSupportName", t => t.ItSupportModuleNameId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemName", t => t.ItSystemModuleNameId, cascadeDelete: true)
                .ForeignKey("dbo.Organization", t => t.Id, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.ItSupportModuleNameId)
                .Index(t => t.ItProjectModuleNameId)
                .Index(t => t.ItSystemModuleNameId)
                .Index(t => t.ItContractModuleNameId);
            
            CreateTable(
                "dbo.ItContractName",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItProjectName",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItSupportName",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItSystemName",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AdminRole",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AgreementElement",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.HandoverTrial",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Localization",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EsdhRef = c.String(unicode: false),
                        CmdbRef = c.String(unicode: false),
                        IdmRef = c.String(unicode: false),
                        FolderRef = c.String(unicode: false),
                        ItProject = c.String(unicode: false),
                        ItProgram = c.String(unicode: false),
                        FocusArea = c.String(unicode: false),
                        Fase1 = c.String(unicode: false),
                        Fase2 = c.String(unicode: false),
                        Fase3 = c.String(unicode: false),
                        Fase4 = c.String(unicode: false),
                        Fase5 = c.String(unicode: false),
                        RightOfUse = c.String(unicode: false),
                        License = c.String(unicode: false),
                        Operation = c.String(unicode: false),
                        Maintenance = c.String(unicode: false),
                        Support = c.String(unicode: false),
                        ServerLicense = c.String(unicode: false),
                        ServerOperation = c.String(unicode: false),
                        Backup = c.String(unicode: false),
                        Surveillance = c.String(unicode: false),
                        Other = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Text",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Value = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItSystemItSystems",
                c => new
                    {
                        ItSystem_Id = c.Int(nullable: false),
                        ItSystem_Id1 = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItSystem_Id, t.ItSystem_Id1 })
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id1)
                .Index(t => t.ItSystem_Id)
                .Index(t => t.ItSystem_Id1);
            
            CreateTable(
                "dbo.TaskRefItSystems",
                c => new
                    {
                        TaskRef_Id = c.Int(nullable: false),
                        ItSystem_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TaskRef_Id, t.ItSystem_Id })
                .ForeignKey("dbo.TaskRef", t => t.TaskRef_Id, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id, cascadeDelete: true)
                .Index(t => t.TaskRef_Id)
                .Index(t => t.ItSystem_Id);
            
            CreateTable(
                "dbo.OrgUnitSystemUsage",
                c => new
                    {
                        ItSystemUsage_Id = c.Int(nullable: false),
                        OrganizationUnit_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItSystemUsage_Id, t.OrganizationUnit_Id })
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id, cascadeDelete: true)
                .ForeignKey("dbo.OrganizationUnit", t => t.OrganizationUnit_Id, cascadeDelete: true)
                .Index(t => t.ItSystemUsage_Id)
                .Index(t => t.OrganizationUnit_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItContract", "HandoverTrial_Id", "dbo.HandoverTrial");
            DropForeignKey("dbo.Agreement", "AgreementElement_Id", "dbo.AgreementElement");
            DropForeignKey("dbo.AdminRight", "UserId", "dbo.User");
            DropForeignKey("dbo.AdminRight", "RoleId", "dbo.AdminRole");
            DropForeignKey("dbo.AdminRight", "ObjectId", "dbo.Organization");
            DropForeignKey("dbo.Config", "Id", "dbo.Organization");
            DropForeignKey("dbo.Config", "ItSystemModuleNameId", "dbo.ItSystemName");
            DropForeignKey("dbo.Config", "ItSupportModuleNameId", "dbo.ItSupportName");
            DropForeignKey("dbo.Config", "ItProjectModuleNameId", "dbo.ItProjectName");
            DropForeignKey("dbo.Config", "ItContractModuleNameId", "dbo.ItContractName");
            DropForeignKey("dbo.ItSystem", "UserId", "dbo.User");
            DropForeignKey("dbo.ItSystem", "TsaId", "dbo.Tsa");
            DropForeignKey("dbo.ItSystem", "ParentId", "dbo.ItSystem");
            DropForeignKey("dbo.Wish", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.Wish", "User_Id", "dbo.User");
            DropForeignKey("dbo.Wish", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.TaskRef", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemRole", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsage", "SensitiveDataTypeId", "dbo.SensitiveDataType");
            DropForeignKey("dbo.ItSystemUsage", "ResponsibleUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.ItSystemUsage", "OverviewItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.OrgUnitSystemUsage", "OrganizationUnit_Id", "dbo.OrganizationUnit");
            DropForeignKey("dbo.OrgUnitSystemUsage", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsage", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.ItSystemUsage", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.ItContract", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItContract", "SupplierId", "dbo.Supplier");
            DropForeignKey("dbo.ShipNotice", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.ItContractRight", "UserId", "dbo.User");
            DropForeignKey("dbo.ItSystemRight", "UserId", "dbo.User");
            DropForeignKey("dbo.ItSystemRight", "RoleId", "dbo.ItSystemRole");
            DropForeignKey("dbo.ItSystemRight", "ObjectId", "dbo.ItSystem");
            DropForeignKey("dbo.PasswordResetRequest", "UserId", "dbo.User");
            DropForeignKey("dbo.User", "DefaultOrganizationUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.TaskRef", "OrganizationUnit_Id", "dbo.OrganizationUnit");
            DropForeignKey("dbo.OrganizationRight", "UserId", "dbo.User");
            DropForeignKey("dbo.OrganizationRight", "RoleId", "dbo.OrganizationRole");
            DropForeignKey("dbo.OrganizationRight", "ObjectId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.OrganizationUnit", "ParentId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.TaskUsage", "TaskRefId", "dbo.TaskRef");
            DropForeignKey("dbo.TaskUsage", "OrgUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.TaskRef", "ParentId", "dbo.TaskRef");
            DropForeignKey("dbo.TaskRef", "OwnedByOrganizationUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.TaskRefItSystems", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.TaskRefItSystems", "TaskRef_Id", "dbo.TaskRef");
            DropForeignKey("dbo.TaskRef", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.Stakeholder", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.Risk", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItProjectRight", "UserId", "dbo.User");
            DropForeignKey("dbo.ItProjectRight", "RoleId", "dbo.ItProjectRole");
            DropForeignKey("dbo.ItProjectRight", "ObjectId", "dbo.ItProject");
            DropForeignKey("dbo.Resource", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "ProjectTypeId", "dbo.ProjectType");
            DropForeignKey("dbo.ProjectStatus", "ProjectPhaseId", "dbo.ProjectPhase");
            DropForeignKey("dbo.ProjectPhaseLocale", "OriginalId", "dbo.ProjectPhase");
            DropForeignKey("dbo.ProjectPhaseLocale", "MunicipalityId", "dbo.Organization");
            DropForeignKey("dbo.Milestone", "ProjectStatusId", "dbo.ProjectStatus");
            DropForeignKey("dbo.ProjectStatus", "Id", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "ProjectCategoryId", "dbo.ProjectCategory");
            DropForeignKey("dbo.PreAnalysis", "Id", "dbo.ItProject");
            DropForeignKey("dbo.OrgTab", "Id", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "MunicipalityId", "dbo.Organization");
            DropForeignKey("dbo.Hierarchy", "ItProjectRef_Id", "dbo.ItProject");
            DropForeignKey("dbo.Hierarchy", "Id", "dbo.ItProject");
            DropForeignKey("dbo.Hierarchy", "ItProgramRef_Id", "dbo.ItProject");
            DropForeignKey("dbo.Handover", "Id", "dbo.ItProject");
            DropForeignKey("dbo.GoalStatus", "Id", "dbo.ItProject");
            DropForeignKey("dbo.Goal", "GoalStatusId", "dbo.GoalStatus");
            DropForeignKey("dbo.ExtReference", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.ExtReference", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ExtReference", "ExtReferenceTypeId", "dbo.ExtReferenceType");
            DropForeignKey("dbo.ExtRefTypeLocale", "OriginalId", "dbo.ExtReferenceType");
            DropForeignKey("dbo.ExtRefTypeLocale", "MunicipalityId", "dbo.Organization");
            DropForeignKey("dbo.Economy", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.Communication", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.OrganizationUnit", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.Infrastructure", "SupplierId", "dbo.Supplier");
            DropForeignKey("dbo.Infrastructure", "DepartmentId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.Infrastructure", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.ItContractRight", "RoleId", "dbo.ItContractRole");
            DropForeignKey("dbo.ItContractRight", "ObjectId", "dbo.ItContract");
            DropForeignKey("dbo.ItContract", "PurchaseFormId", "dbo.PurchaseForm");
            DropForeignKey("dbo.ItContract", "PaymentModelId", "dbo.PaymentModel");
            DropForeignKey("dbo.Payment", "Id", "dbo.ItContract");
            DropForeignKey("dbo.ItContract", "MunicipalityId", "dbo.Organization");
            DropForeignKey("dbo.ItContract", "ContractTypeId", "dbo.ContractType");
            DropForeignKey("dbo.ItContract", "ContractTemplateId", "dbo.ContractTemplate");
            DropForeignKey("dbo.Agreement", "Id", "dbo.ItContract");
            DropForeignKey("dbo.ItSystemUsage", "ArchiveTypeId", "dbo.ArchiveType");
            DropForeignKey("dbo.ItSystem", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.ItSystem", "MethodId", "dbo.Method");
            DropForeignKey("dbo.ItSystem", "InterfaceTypeId", "dbo.InterfaceType");
            DropForeignKey("dbo.ItSystem", "InterfaceId", "dbo.Interface");
            DropForeignKey("dbo.ItSystem", "ExposedById", "dbo.ItSystem");
            DropForeignKey("dbo.DataRows", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.DataRows", "DataTypeId", "dbo.DataType");
            DropForeignKey("dbo.ItSystemItSystems", "ItSystem_Id1", "dbo.ItSystem");
            DropForeignKey("dbo.ItSystemItSystems", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.ItSystem", "BusinessTypeId", "dbo.BusinessType");
            DropForeignKey("dbo.ItSystem", "BelongsToId", "dbo.Organization");
            DropForeignKey("dbo.ItSystem", "AppTypeId", "dbo.AppType");
            DropIndex("dbo.OrgUnitSystemUsage", new[] { "OrganizationUnit_Id" });
            DropIndex("dbo.OrgUnitSystemUsage", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.TaskRefItSystems", new[] { "ItSystem_Id" });
            DropIndex("dbo.TaskRefItSystems", new[] { "TaskRef_Id" });
            DropIndex("dbo.ItSystemItSystems", new[] { "ItSystem_Id1" });
            DropIndex("dbo.ItSystemItSystems", new[] { "ItSystem_Id" });
            DropIndex("dbo.Config", new[] { "ItContractModuleNameId" });
            DropIndex("dbo.Config", new[] { "ItSystemModuleNameId" });
            DropIndex("dbo.Config", new[] { "ItProjectModuleNameId" });
            DropIndex("dbo.Config", new[] { "ItSupportModuleNameId" });
            DropIndex("dbo.Config", new[] { "Id" });
            DropIndex("dbo.Wish", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.Wish", new[] { "User_Id" });
            DropIndex("dbo.Wish", new[] { "ItSystem_Id" });
            DropIndex("dbo.ShipNotice", new[] { "ItContractId" });
            DropIndex("dbo.ItSystemRole", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.ItSystemRight", new[] { "UserId" });
            DropIndex("dbo.ItSystemRight", new[] { "RoleId" });
            DropIndex("dbo.ItSystemRight", new[] { "ObjectId" });
            DropIndex("dbo.PasswordResetRequest", new[] { "UserId" });
            DropIndex("dbo.OrganizationRight", new[] { "UserId" });
            DropIndex("dbo.OrganizationRight", new[] { "RoleId" });
            DropIndex("dbo.OrganizationRight", new[] { "ObjectId" });
            DropIndex("dbo.TaskUsage", new[] { "OrgUnitId" });
            DropIndex("dbo.TaskUsage", new[] { "TaskRefId" });
            DropIndex("dbo.Stakeholder", new[] { "ItProjectId" });
            DropIndex("dbo.Risk", new[] { "ItProjectId" });
            DropIndex("dbo.ItProjectRight", new[] { "UserId" });
            DropIndex("dbo.ItProjectRight", new[] { "RoleId" });
            DropIndex("dbo.ItProjectRight", new[] { "ObjectId" });
            DropIndex("dbo.Resource", new[] { "ItProjectId" });
            DropIndex("dbo.ProjectPhaseLocale", new[] { "OriginalId" });
            DropIndex("dbo.ProjectPhaseLocale", new[] { "MunicipalityId" });
            DropIndex("dbo.Milestone", new[] { "ProjectStatusId" });
            DropIndex("dbo.ProjectStatus", new[] { "ProjectPhaseId" });
            DropIndex("dbo.ProjectStatus", new[] { "Id" });
            DropIndex("dbo.PreAnalysis", new[] { "Id" });
            DropIndex("dbo.OrgTab", new[] { "Id" });
            DropIndex("dbo.Hierarchy", new[] { "ItProjectRef_Id" });
            DropIndex("dbo.Hierarchy", new[] { "ItProgramRef_Id" });
            DropIndex("dbo.Hierarchy", new[] { "Id" });
            DropIndex("dbo.Handover", new[] { "Id" });
            DropIndex("dbo.Goal", new[] { "GoalStatusId" });
            DropIndex("dbo.GoalStatus", new[] { "Id" });
            DropIndex("dbo.ExtRefTypeLocale", new[] { "OriginalId" });
            DropIndex("dbo.ExtRefTypeLocale", new[] { "MunicipalityId" });
            DropIndex("dbo.ExtReference", new[] { "ItSystemId" });
            DropIndex("dbo.ExtReference", new[] { "ExtReferenceTypeId" });
            DropIndex("dbo.ExtReference", new[] { "ItProjectId" });
            DropIndex("dbo.Economy", new[] { "ItProjectId" });
            DropIndex("dbo.Communication", new[] { "ItProjectId" });
            DropIndex("dbo.ItProject", new[] { "MunicipalityId" });
            DropIndex("dbo.ItProject", new[] { "ProjectCategoryId" });
            DropIndex("dbo.ItProject", new[] { "ProjectTypeId" });
            DropIndex("dbo.TaskRef", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.TaskRef", new[] { "OrganizationUnit_Id" });
            DropIndex("dbo.TaskRef", new[] { "OwnedByOrganizationUnitId" });
            DropIndex("dbo.TaskRef", new[] { "ParentId" });
            DropIndex("dbo.TaskRef", new[] { "ItProjectId" });
            DropIndex("dbo.Infrastructure", new[] { "ItSystem_Id" });
            DropIndex("dbo.Infrastructure", new[] { "DepartmentId" });
            DropIndex("dbo.Infrastructure", new[] { "SupplierId" });
            DropIndex("dbo.OrganizationUnit", new[] { "OrganizationId" });
            DropIndex("dbo.OrganizationUnit", new[] { "ParentId" });
            DropIndex("dbo.User", new[] { "DefaultOrganizationUnitId" });
            DropIndex("dbo.ItContractRight", new[] { "UserId" });
            DropIndex("dbo.ItContractRight", new[] { "RoleId" });
            DropIndex("dbo.ItContractRight", new[] { "ObjectId" });
            DropIndex("dbo.Payment", new[] { "Id" });
            DropIndex("dbo.Agreement", new[] { "AgreementElement_Id" });
            DropIndex("dbo.Agreement", new[] { "Id" });
            DropIndex("dbo.ItContract", new[] { "HandoverTrial_Id" });
            DropIndex("dbo.ItContract", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.ItContract", new[] { "MunicipalityId" });
            DropIndex("dbo.ItContract", new[] { "SupplierId" });
            DropIndex("dbo.ItContract", new[] { "PaymentModelId" });
            DropIndex("dbo.ItContract", new[] { "PurchaseFormId" });
            DropIndex("dbo.ItContract", new[] { "ContractTemplateId" });
            DropIndex("dbo.ItContract", new[] { "ContractTypeId" });
            DropIndex("dbo.ItSystemUsage", new[] { "OverviewItSystemId" });
            DropIndex("dbo.ItSystemUsage", new[] { "SensitiveDataTypeId" });
            DropIndex("dbo.ItSystemUsage", new[] { "ArchiveTypeId" });
            DropIndex("dbo.ItSystemUsage", new[] { "ItSystemId" });
            DropIndex("dbo.ItSystemUsage", new[] { "OrganizationId" });
            DropIndex("dbo.ItSystemUsage", new[] { "ResponsibleUnitId" });
            DropIndex("dbo.DataRows", new[] { "DataTypeId" });
            DropIndex("dbo.DataRows", new[] { "ItSystemId" });
            DropIndex("dbo.ItSystem", new[] { "MethodId" });
            DropIndex("dbo.ItSystem", new[] { "TsaId" });
            DropIndex("dbo.ItSystem", new[] { "InterfaceTypeId" });
            DropIndex("dbo.ItSystem", new[] { "InterfaceId" });
            DropIndex("dbo.ItSystem", new[] { "BusinessTypeId" });
            DropIndex("dbo.ItSystem", new[] { "AppTypeId" });
            DropIndex("dbo.ItSystem", new[] { "ParentId" });
            DropIndex("dbo.ItSystem", new[] { "UserId" });
            DropIndex("dbo.ItSystem", new[] { "OrganizationId" });
            DropIndex("dbo.ItSystem", new[] { "BelongsToId" });
            DropIndex("dbo.ItSystem", new[] { "ExposedById" });
            DropIndex("dbo.AdminRight", new[] { "UserId" });
            DropIndex("dbo.AdminRight", new[] { "RoleId" });
            DropIndex("dbo.AdminRight", new[] { "ObjectId" });
            DropTable("dbo.OrgUnitSystemUsage");
            DropTable("dbo.TaskRefItSystems");
            DropTable("dbo.ItSystemItSystems");
            DropTable("dbo.Text");
            DropTable("dbo.Localization");
            DropTable("dbo.HandoverTrial");
            DropTable("dbo.AgreementElement");
            DropTable("dbo.AdminRole");
            DropTable("dbo.ItSystemName");
            DropTable("dbo.ItSupportName");
            DropTable("dbo.ItProjectName");
            DropTable("dbo.ItContractName");
            DropTable("dbo.Config");
            DropTable("dbo.Tsa");
            DropTable("dbo.Wish");
            DropTable("dbo.SensitiveDataType");
            DropTable("dbo.ShipNotice");
            DropTable("dbo.ItSystemRole");
            DropTable("dbo.ItSystemRight");
            DropTable("dbo.PasswordResetRequest");
            DropTable("dbo.OrganizationRole");
            DropTable("dbo.OrganizationRight");
            DropTable("dbo.TaskUsage");
            DropTable("dbo.Stakeholder");
            DropTable("dbo.Risk");
            DropTable("dbo.ItProjectRole");
            DropTable("dbo.ItProjectRight");
            DropTable("dbo.Resource");
            DropTable("dbo.ProjectType");
            DropTable("dbo.ProjectPhaseLocale");
            DropTable("dbo.ProjectPhase");
            DropTable("dbo.Milestone");
            DropTable("dbo.ProjectStatus");
            DropTable("dbo.ProjectCategory");
            DropTable("dbo.PreAnalysis");
            DropTable("dbo.OrgTab");
            DropTable("dbo.Hierarchy");
            DropTable("dbo.Handover");
            DropTable("dbo.Goal");
            DropTable("dbo.GoalStatus");
            DropTable("dbo.ExtRefTypeLocale");
            DropTable("dbo.ExtReferenceType");
            DropTable("dbo.ExtReference");
            DropTable("dbo.Economy");
            DropTable("dbo.Communication");
            DropTable("dbo.ItProject");
            DropTable("dbo.TaskRef");
            DropTable("dbo.Supplier");
            DropTable("dbo.Infrastructure");
            DropTable("dbo.OrganizationUnit");
            DropTable("dbo.User");
            DropTable("dbo.ItContractRole");
            DropTable("dbo.ItContractRight");
            DropTable("dbo.PurchaseForm");
            DropTable("dbo.PaymentModel");
            DropTable("dbo.Payment");
            DropTable("dbo.ContractType");
            DropTable("dbo.ContractTemplate");
            DropTable("dbo.Agreement");
            DropTable("dbo.ItContract");
            DropTable("dbo.ArchiveType");
            DropTable("dbo.ItSystemUsage");
            DropTable("dbo.Method");
            DropTable("dbo.InterfaceType");
            DropTable("dbo.Interface");
            DropTable("dbo.DataType");
            DropTable("dbo.DataRows");
            DropTable("dbo.BusinessType");
            DropTable("dbo.AppType");
            DropTable("dbo.ItSystem");
            DropTable("dbo.Organization");
            DropTable("dbo.AdminRight");
        }
    }
}
