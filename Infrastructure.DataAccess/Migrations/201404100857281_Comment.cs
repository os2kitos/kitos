namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Comment : DbMigration
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
                "dbo.User",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        Email = c.String(nullable: false, unicode: false),
                        Password = c.String(nullable: false, unicode: false),
                        Salt = c.String(nullable: false, unicode: false),
                        IsGlobalAdmin = c.Boolean(nullable: false),
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
                        HandoverTrial_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ContractTemplate", t => t.ContractTemplateId, cascadeDelete: true)
                .ForeignKey("dbo.ContractType", t => t.ContractTypeId, cascadeDelete: true)
                .ForeignKey("dbo.Organization", t => t.MunicipalityId)
                .ForeignKey("dbo.PaymentModel", t => t.PaymentModelId, cascadeDelete: true)
                .ForeignKey("dbo.PurchaseForm", t => t.PurchaseFormId, cascadeDelete: true)
                .ForeignKey("dbo.Supplier", t => t.SupplierId, cascadeDelete: true)
                .ForeignKey("dbo.HandoverTrial", t => t.HandoverTrial_Id)
                .Index(t => t.ContractTypeId)
                .Index(t => t.ContractTemplateId)
                .Index(t => t.PurchaseFormId)
                .Index(t => t.PaymentModelId)
                .Index(t => t.SupplierId)
                .Index(t => t.MunicipalityId)
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
                "dbo.Supplier",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Infrastructure",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        HostId = c.Int(nullable: false),
                        SupplierId = c.Int(nullable: false),
                        DepartmentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Host", t => t.HostId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.Id)
                .ForeignKey("dbo.OrganizationUnit", t => t.DepartmentId, cascadeDelete: true)
                .ForeignKey("dbo.Supplier", t => t.SupplierId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.HostId)
                .Index(t => t.SupplierId)
                .Index(t => t.DepartmentId);
            
            CreateTable(
                "dbo.Host",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItSystem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentItSystemId = c.Int(nullable: false),
                        MunicipalityId = c.Int(nullable: false),
                        SystemTypeId = c.Int(nullable: false),
                        InterfaceTypeId = c.Int(nullable: false),
                        ProtocolTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.InterfaceType", t => t.InterfaceTypeId)
                .ForeignKey("dbo.Organization", t => t.MunicipalityId)
                .ForeignKey("dbo.ItSystem", t => t.ParentItSystemId)
                .ForeignKey("dbo.ProtocolType", t => t.ProtocolTypeId)
                .ForeignKey("dbo.SystemType", t => t.SystemTypeId)
                .Index(t => t.ParentItSystemId)
                .Index(t => t.MunicipalityId)
                .Index(t => t.SystemTypeId)
                .Index(t => t.InterfaceTypeId)
                .Index(t => t.ProtocolTypeId);
            
            CreateTable(
                "dbo.BasicData",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystemId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .Index(t => t.ItSystemId);
            
            CreateTable(
                "dbo.Component",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystemId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .Index(t => t.ItSystemId);
            
            CreateTable(
                "dbo.Functionality",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Wish",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FunctionalityId = c.Int(),
                        InterfaceId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Functionality", t => t.FunctionalityId)
                .ForeignKey("dbo.Interface", t => t.InterfaceId)
                .Index(t => t.FunctionalityId)
                .Index(t => t.InterfaceId);
            
            CreateTable(
                "dbo.Interface",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystemId = c.Int(nullable: false),
                        MethodId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .ForeignKey("dbo.Method", t => t.MethodId, cascadeDelete: true)
                .Index(t => t.ItSystemId)
                .Index(t => t.MethodId);
            
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
                "dbo.TaskRef",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Uuid = c.Guid(nullable: false),
                        Type = c.String(unicode: false),
                        TaskKey = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        ActiveFrom = c.DateTime(precision: 0),
                        ActiveTo = c.DateTime(precision: 0),
                        ItProjectId = c.Int(),
                        ItSystemId = c.Int(),
                        ParentId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId)
                .ForeignKey("dbo.ItSystem", t => t.ItSystemId)
                .ForeignKey("dbo.TaskRef", t => t.ParentId)
                .Index(t => t.ItProjectId)
                .Index(t => t.ItSystemId)
                .Index(t => t.ParentId);
            
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
                "dbo.ProtocolType",
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
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SuperUser",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystemId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .Index(t => t.ItSystemId);
            
            CreateTable(
                "dbo.SystemType",
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
                "dbo.TaskSupport",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystemId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .Index(t => t.ItSystemId);
            
            CreateTable(
                "dbo.Technology",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        DatabaseTypeId = c.Int(nullable: false),
                        EnvironmentId = c.Int(nullable: false),
                        ProgLanguageId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DatabaseType", t => t.DatabaseTypeId, cascadeDelete: true)
                .ForeignKey("dbo.Environment", t => t.EnvironmentId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.Id)
                .ForeignKey("dbo.ProgLanguage", t => t.ProgLanguageId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.DatabaseTypeId)
                .Index(t => t.EnvironmentId)
                .Index(t => t.ProgLanguageId);
            
            CreateTable(
                "dbo.DatabaseType",
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
                "dbo.Environment",
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
                "dbo.ProgLanguage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserAdministration",
                c => new
                    {
                        Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.Id)
                .Index(t => t.Id);
            
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
                "dbo.AdminRole",
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItContract", "HandoverTrial_Id", "dbo.HandoverTrial");
            DropForeignKey("dbo.Agreement", "AgreementElement_Id", "dbo.AgreementElement");
            DropForeignKey("dbo.AdminRight", "UserId", "dbo.User");
            DropForeignKey("dbo.AdminRight", "RoleId", "dbo.AdminRole");
            DropForeignKey("dbo.AdminRight", "ObjectId", "dbo.Organization");
            DropForeignKey("dbo.ExtRefTypeLocale", "OriginalId", "dbo.ExtReferenceType");
            DropForeignKey("dbo.ExtReference", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.ExtReference", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.Stakeholder", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.Risk", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItProjectRight", "UserId", "dbo.User");
            DropForeignKey("dbo.PasswordResetRequest", "UserId", "dbo.User");
            DropForeignKey("dbo.ItContractRight", "UserId", "dbo.User");
            DropForeignKey("dbo.ItContractRight", "RoleId", "dbo.ItContractRole");
            DropForeignKey("dbo.ItContractRight", "ObjectId", "dbo.ItContract");
            DropForeignKey("dbo.ItContract", "SupplierId", "dbo.Supplier");
            DropForeignKey("dbo.Infrastructure", "SupplierId", "dbo.Supplier");
            DropForeignKey("dbo.Infrastructure", "DepartmentId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.Infrastructure", "Id", "dbo.ItSystem");
            DropForeignKey("dbo.UserAdministration", "Id", "dbo.ItSystem");
            DropForeignKey("dbo.Technology", "ProgLanguageId", "dbo.ProgLanguage");
            DropForeignKey("dbo.Technology", "Id", "dbo.ItSystem");
            DropForeignKey("dbo.Technology", "EnvironmentId", "dbo.Environment");
            DropForeignKey("dbo.Technology", "DatabaseTypeId", "dbo.DatabaseType");
            DropForeignKey("dbo.TaskSupport", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.ItSystem", "SystemTypeId", "dbo.SystemType");
            DropForeignKey("dbo.SuperUser", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.ItSystemRight", "UserId", "dbo.User");
            DropForeignKey("dbo.ItSystemRight", "RoleId", "dbo.ItSystemRole");
            DropForeignKey("dbo.ItSystemRight", "ObjectId", "dbo.ItSystem");
            DropForeignKey("dbo.ItSystem", "ProtocolTypeId", "dbo.ProtocolType");
            DropForeignKey("dbo.ItSystem", "ParentItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.ItSystem", "MunicipalityId", "dbo.Organization");
            DropForeignKey("dbo.TaskUsage", "TaskRefId", "dbo.TaskRef");
            DropForeignKey("dbo.TaskUsage", "OrgUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.OrganizationRight", "UserId", "dbo.User");
            DropForeignKey("dbo.OrganizationRight", "RoleId", "dbo.OrganizationRole");
            DropForeignKey("dbo.OrganizationRight", "ObjectId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.OrganizationUnit", "ParentId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.OrganizationUnit", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.TaskRef", "ParentId", "dbo.TaskRef");
            DropForeignKey("dbo.TaskRef", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.TaskRef", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItSystem", "InterfaceTypeId", "dbo.InterfaceType");
            DropForeignKey("dbo.Wish", "InterfaceId", "dbo.Interface");
            DropForeignKey("dbo.Interface", "MethodId", "dbo.Method");
            DropForeignKey("dbo.Interface", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.Wish", "FunctionalityId", "dbo.Functionality");
            DropForeignKey("dbo.Functionality", "Id", "dbo.ItSystem");
            DropForeignKey("dbo.Component", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.BasicData", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.Infrastructure", "HostId", "dbo.Host");
            DropForeignKey("dbo.ShipNotice", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.ItContract", "PurchaseFormId", "dbo.PurchaseForm");
            DropForeignKey("dbo.ItContract", "PaymentModelId", "dbo.PaymentModel");
            DropForeignKey("dbo.Payment", "Id", "dbo.ItContract");
            DropForeignKey("dbo.ItContract", "MunicipalityId", "dbo.Organization");
            DropForeignKey("dbo.ItContract", "ContractTypeId", "dbo.ContractType");
            DropForeignKey("dbo.ItContract", "ContractTemplateId", "dbo.ContractTemplate");
            DropForeignKey("dbo.Agreement", "Id", "dbo.ItContract");
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
            DropForeignKey("dbo.Economy", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.Communication", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ExtReference", "ExtReferenceTypeId", "dbo.ExtReferenceType");
            DropForeignKey("dbo.ExtRefTypeLocale", "MunicipalityId", "dbo.Organization");
            DropForeignKey("dbo.Config", "Id", "dbo.Organization");
            DropForeignKey("dbo.Config", "ItSystemModuleNameId", "dbo.ItSystemName");
            DropForeignKey("dbo.Config", "ItSupportModuleNameId", "dbo.ItSupportName");
            DropForeignKey("dbo.Config", "ItProjectModuleNameId", "dbo.ItProjectName");
            DropForeignKey("dbo.Config", "ItContractModuleNameId", "dbo.ItContractName");
            DropIndex("dbo.Stakeholder", new[] { "ItProjectId" });
            DropIndex("dbo.Risk", new[] { "ItProjectId" });
            DropIndex("dbo.PasswordResetRequest", new[] { "UserId" });
            DropIndex("dbo.UserAdministration", new[] { "Id" });
            DropIndex("dbo.Technology", new[] { "ProgLanguageId" });
            DropIndex("dbo.Technology", new[] { "EnvironmentId" });
            DropIndex("dbo.Technology", new[] { "DatabaseTypeId" });
            DropIndex("dbo.Technology", new[] { "Id" });
            DropIndex("dbo.TaskSupport", new[] { "ItSystemId" });
            DropIndex("dbo.SuperUser", new[] { "ItSystemId" });
            DropIndex("dbo.ItSystemRight", new[] { "UserId" });
            DropIndex("dbo.ItSystemRight", new[] { "RoleId" });
            DropIndex("dbo.ItSystemRight", new[] { "ObjectId" });
            DropIndex("dbo.OrganizationRight", new[] { "UserId" });
            DropIndex("dbo.OrganizationRight", new[] { "RoleId" });
            DropIndex("dbo.OrganizationRight", new[] { "ObjectId" });
            DropIndex("dbo.OrganizationUnit", new[] { "OrganizationId" });
            DropIndex("dbo.OrganizationUnit", new[] { "ParentId" });
            DropIndex("dbo.TaskUsage", new[] { "OrgUnitId" });
            DropIndex("dbo.TaskUsage", new[] { "TaskRefId" });
            DropIndex("dbo.TaskRef", new[] { "ParentId" });
            DropIndex("dbo.TaskRef", new[] { "ItSystemId" });
            DropIndex("dbo.TaskRef", new[] { "ItProjectId" });
            DropIndex("dbo.Interface", new[] { "MethodId" });
            DropIndex("dbo.Interface", new[] { "ItSystemId" });
            DropIndex("dbo.Wish", new[] { "InterfaceId" });
            DropIndex("dbo.Wish", new[] { "FunctionalityId" });
            DropIndex("dbo.Functionality", new[] { "Id" });
            DropIndex("dbo.Component", new[] { "ItSystemId" });
            DropIndex("dbo.BasicData", new[] { "ItSystemId" });
            DropIndex("dbo.ItSystem", new[] { "ProtocolTypeId" });
            DropIndex("dbo.ItSystem", new[] { "InterfaceTypeId" });
            DropIndex("dbo.ItSystem", new[] { "SystemTypeId" });
            DropIndex("dbo.ItSystem", new[] { "MunicipalityId" });
            DropIndex("dbo.ItSystem", new[] { "ParentItSystemId" });
            DropIndex("dbo.Infrastructure", new[] { "DepartmentId" });
            DropIndex("dbo.Infrastructure", new[] { "SupplierId" });
            DropIndex("dbo.Infrastructure", new[] { "HostId" });
            DropIndex("dbo.Infrastructure", new[] { "Id" });
            DropIndex("dbo.ShipNotice", new[] { "ItContractId" });
            DropIndex("dbo.Payment", new[] { "Id" });
            DropIndex("dbo.Agreement", new[] { "AgreementElement_Id" });
            DropIndex("dbo.Agreement", new[] { "Id" });
            DropIndex("dbo.ItContract", new[] { "HandoverTrial_Id" });
            DropIndex("dbo.ItContract", new[] { "MunicipalityId" });
            DropIndex("dbo.ItContract", new[] { "SupplierId" });
            DropIndex("dbo.ItContract", new[] { "PaymentModelId" });
            DropIndex("dbo.ItContract", new[] { "PurchaseFormId" });
            DropIndex("dbo.ItContract", new[] { "ContractTemplateId" });
            DropIndex("dbo.ItContract", new[] { "ContractTypeId" });
            DropIndex("dbo.ItContractRight", new[] { "UserId" });
            DropIndex("dbo.ItContractRight", new[] { "RoleId" });
            DropIndex("dbo.ItContractRight", new[] { "ObjectId" });
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
            DropIndex("dbo.Economy", new[] { "ItProjectId" });
            DropIndex("dbo.Communication", new[] { "ItProjectId" });
            DropIndex("dbo.ItProject", new[] { "MunicipalityId" });
            DropIndex("dbo.ItProject", new[] { "ProjectCategoryId" });
            DropIndex("dbo.ItProject", new[] { "ProjectTypeId" });
            DropIndex("dbo.ExtReference", new[] { "ItSystemId" });
            DropIndex("dbo.ExtReference", new[] { "ExtReferenceTypeId" });
            DropIndex("dbo.ExtReference", new[] { "ItProjectId" });
            DropIndex("dbo.ExtRefTypeLocale", new[] { "OriginalId" });
            DropIndex("dbo.ExtRefTypeLocale", new[] { "MunicipalityId" });
            DropIndex("dbo.Config", new[] { "ItContractModuleNameId" });
            DropIndex("dbo.Config", new[] { "ItSystemModuleNameId" });
            DropIndex("dbo.Config", new[] { "ItProjectModuleNameId" });
            DropIndex("dbo.Config", new[] { "ItSupportModuleNameId" });
            DropIndex("dbo.Config", new[] { "Id" });
            DropIndex("dbo.AdminRight", new[] { "UserId" });
            DropIndex("dbo.AdminRight", new[] { "RoleId" });
            DropIndex("dbo.AdminRight", new[] { "ObjectId" });
            DropTable("dbo.Text");
            DropTable("dbo.Localization");
            DropTable("dbo.HandoverTrial");
            DropTable("dbo.AgreementElement");
            DropTable("dbo.AdminRole");
            DropTable("dbo.Stakeholder");
            DropTable("dbo.Risk");
            DropTable("dbo.PasswordResetRequest");
            DropTable("dbo.ItContractRole");
            DropTable("dbo.UserAdministration");
            DropTable("dbo.ProgLanguage");
            DropTable("dbo.Environment");
            DropTable("dbo.DatabaseType");
            DropTable("dbo.Technology");
            DropTable("dbo.TaskSupport");
            DropTable("dbo.SystemType");
            DropTable("dbo.SuperUser");
            DropTable("dbo.ItSystemRole");
            DropTable("dbo.ItSystemRight");
            DropTable("dbo.ProtocolType");
            DropTable("dbo.OrganizationRole");
            DropTable("dbo.OrganizationRight");
            DropTable("dbo.OrganizationUnit");
            DropTable("dbo.TaskUsage");
            DropTable("dbo.TaskRef");
            DropTable("dbo.InterfaceType");
            DropTable("dbo.Method");
            DropTable("dbo.Interface");
            DropTable("dbo.Wish");
            DropTable("dbo.Functionality");
            DropTable("dbo.Component");
            DropTable("dbo.BasicData");
            DropTable("dbo.ItSystem");
            DropTable("dbo.Host");
            DropTable("dbo.Infrastructure");
            DropTable("dbo.Supplier");
            DropTable("dbo.ShipNotice");
            DropTable("dbo.PurchaseForm");
            DropTable("dbo.PaymentModel");
            DropTable("dbo.Payment");
            DropTable("dbo.ContractType");
            DropTable("dbo.ContractTemplate");
            DropTable("dbo.Agreement");
            DropTable("dbo.ItContract");
            DropTable("dbo.ItContractRight");
            DropTable("dbo.User");
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
            DropTable("dbo.Economy");
            DropTable("dbo.Communication");
            DropTable("dbo.ItProject");
            DropTable("dbo.ExtReference");
            DropTable("dbo.ExtReferenceType");
            DropTable("dbo.ExtRefTypeLocale");
            DropTable("dbo.ItSystemName");
            DropTable("dbo.ItSupportName");
            DropTable("dbo.ItProjectName");
            DropTable("dbo.ItContractName");
            DropTable("dbo.Config");
            DropTable("dbo.Organization");
            DropTable("dbo.AdminRight");
        }
    }
}
