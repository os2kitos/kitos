namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Start : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdminRights",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        ObjectId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.Organization", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.AdminRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.ObjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
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
                        CreatedInId = c.Int(),
                        DefaultOrganizationUnitId = c.Int(),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organization", t => t.CreatedInId)
                .ForeignKey("dbo.OrganizationUnit", t => t.DefaultOrganizationUnitId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.CreatedInId)
                .Index(t => t.DefaultOrganizationUnitId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.Organization",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        Type = c.Int(),
                        Cvr = c.String(unicode: false),
                        AccessModifier = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItSystem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystemId = c.String(unicode: false),
                        AppTypeOptionId = c.Int(),
                        ParentId = c.Int(),
                        BusinessTypeId = c.Int(),
                        Name = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        Url = c.String(unicode: false),
                        AccessModifier = c.Int(nullable: false),
                        OrganizationId = c.Int(nullable: false),
                        BelongsToId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemTypeOptions", t => t.AppTypeOptionId)
                .ForeignKey("dbo.Organization", t => t.BelongsToId)
                .ForeignKey("dbo.BusinessTypes", t => t.BusinessTypeId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.ParentId)
                .Index(t => t.AppTypeOptionId)
                .Index(t => t.ParentId)
                .Index(t => t.BusinessTypeId)
                .Index(t => t.OrganizationId)
                .Index(t => t.BelongsToId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItSystemTypeOptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.BusinessTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItInterfaceUses",
                c => new
                    {
                        ItSystemId = c.Int(nullable: false),
                        ItInterfaceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItSystemId, t.ItInterfaceId })
                .ForeignKey("dbo.ItInterface", t => t.ItInterfaceId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .Index(t => t.ItSystemId)
                .Index(t => t.ItInterfaceId);
            
            CreateTable(
                "dbo.InfUsage",
                c => new
                    {
                        ItSystemUsageId = c.Int(nullable: false),
                        ItSystemId = c.Int(nullable: false),
                        ItInterfaceId = c.Int(nullable: false),
                        ItContractId = c.Int(),
                        InfrastructureId = c.Int(),
                        IsWishedFor = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItSystemUsageId, t.ItSystemId, t.ItInterfaceId })
                .ForeignKey("dbo.ItInterface", t => t.ItInterfaceId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.InfrastructureId)
                .ForeignKey("dbo.ItContract", t => t.ItContractId, cascadeDelete: true)
                .ForeignKey("dbo.ItInterfaceUses", t => new { t.ItSystemId, t.ItInterfaceId }, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsageId, cascadeDelete: true)
                .Index(t => t.ItSystemUsageId)
                .Index(t => new { t.ItSystemId, t.ItInterfaceId })
                .Index(t => t.ItContractId)
                .Index(t => t.InfrastructureId);
            
            CreateTable(
                "dbo.DataRowUsage",
                c => new
                    {
                        DataRowId = c.Int(nullable: false),
                        SysUsageId = c.Int(nullable: false),
                        SysId = c.Int(nullable: false),
                        IntfId = c.Int(nullable: false),
                        FrequencyId = c.Int(),
                        Amount = c.Int(nullable: false),
                        Economy = c.Int(nullable: false),
                        Price = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.DataRowId, t.SysUsageId, t.SysId, t.IntfId })
                .ForeignKey("dbo.DataRow", t => t.DataRowId, cascadeDelete: true)
                .ForeignKey("dbo.Frequencies", t => t.FrequencyId)
                .ForeignKey("dbo.InfUsage", t => new { t.SysUsageId, t.SysId, t.IntfId }, cascadeDelete: true)
                .Index(t => t.DataRowId)
                .Index(t => new { t.SysUsageId, t.SysId, t.IntfId })
                .Index(t => t.FrequencyId);
            
            CreateTable(
                "dbo.DataRow",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItInterfaceId = c.Int(nullable: false),
                        DataTypeId = c.Int(),
                        Data = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DataTypes", t => t.DataTypeId)
                .ForeignKey("dbo.ItInterface", t => t.ItInterfaceId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ItInterfaceId)
                .Index(t => t.DataTypeId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.DataTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItInterface",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Version = c.String(unicode: false),
                        ItInterfaceId = c.String(unicode: false),
                        InterfaceId = c.Int(),
                        InterfaceTypeId = c.Int(),
                        TsaId = c.Int(),
                        MethodId = c.Int(),
                        Name = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        Url = c.String(unicode: false),
                        AccessModifier = c.Int(nullable: false),
                        OrganizationId = c.Int(nullable: false),
                        BelongsToId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organization", t => t.BelongsToId)
                .ForeignKey("dbo.Interfaces", t => t.InterfaceId)
                .ForeignKey("dbo.InterfaceTypes", t => t.InterfaceTypeId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.Methods", t => t.MethodId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("dbo.Tsas", t => t.TsaId)
                .Index(t => t.InterfaceId)
                .Index(t => t.InterfaceTypeId)
                .Index(t => t.TsaId)
                .Index(t => t.MethodId)
                .Index(t => t.OrganizationId)
                .Index(t => t.BelongsToId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.Exhibit",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ItSystemId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItInterface", t => t.Id, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.ItSystemId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItInterfaceExhibitUsage",
                c => new
                    {
                        ItSystemUsageId = c.Int(nullable: false),
                        ItInterfaceExhibitId = c.Int(nullable: false),
                        ItContractId = c.Int(),
                        IsWishedFor = c.Boolean(nullable: false),
                        ItInterface_Id = c.Int(),
                    })
                .PrimaryKey(t => new { t.ItSystemUsageId, t.ItInterfaceExhibitId })
                .ForeignKey("dbo.ItContract", t => t.ItContractId)
                .ForeignKey("dbo.Exhibit", t => t.ItInterfaceExhibitId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsageId, cascadeDelete: true)
                .ForeignKey("dbo.ItInterface", t => t.ItInterface_Id)
                .Index(t => t.ItSystemUsageId)
                .Index(t => t.ItInterfaceExhibitId)
                .Index(t => t.ItContractId)
                .Index(t => t.ItInterface_Id);
            
            CreateTable(
                "dbo.ItContract",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        Note = c.String(unicode: false),
                        ItContractId = c.String(unicode: false),
                        Esdh = c.String(unicode: false),
                        Folder = c.String(unicode: false),
                        SupplierContractSigner = c.String(unicode: false),
                        HasSupplierSigned = c.Boolean(nullable: false),
                        SupplierSignedDate = c.DateTime(precision: 0),
                        ContractSignerId = c.Int(),
                        IsSigned = c.Boolean(nullable: false),
                        SignedDate = c.DateTime(precision: 0),
                        ResponsibleOrganizationUnitId = c.Int(),
                        OrganizationId = c.Int(nullable: false),
                        SupplierId = c.Int(),
                        ProcurementStrategyId = c.Int(),
                        ProcurementPlanHalf = c.Int(),
                        ProcurementPlanYear = c.Int(),
                        ContractTemplateId = c.Int(),
                        ContractTypeId = c.Int(),
                        PurchaseFormId = c.Int(),
                        ParentId = c.Int(),
                        OperationTestExpected = c.DateTime(precision: 0),
                        OperationTestApproved = c.DateTime(precision: 0),
                        OperationalAcceptanceTestExpected = c.DateTime(precision: 0),
                        OperationalAcceptanceTestApproved = c.DateTime(precision: 0),
                        Concluded = c.DateTime(precision: 0),
                        Duration = c.Int(nullable: false),
                        IrrevocableTo = c.DateTime(precision: 0),
                        ExpirationDate = c.DateTime(precision: 0),
                        Terminated = c.DateTime(precision: 0),
                        TerminationDeadlineId = c.Int(),
                        OptionExtendId = c.Int(),
                        ExtendMultiplier = c.Int(nullable: false),
                        OperationRemunerationBegun = c.DateTime(precision: 0),
                        PaymentFreqencyId = c.Int(),
                        PaymentModelId = c.Int(),
                        PriceRegulationId = c.Int(),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.ContractSignerId)
                .ForeignKey("dbo.ContractTemplates", t => t.ContractTemplateId)
                .ForeignKey("dbo.ContractTypes", t => t.ContractTypeId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.OptionExtends", t => t.OptionExtendId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("dbo.ItContract", t => t.ParentId)
                .ForeignKey("dbo.PaymentFreqencies", t => t.PaymentFreqencyId)
                .ForeignKey("dbo.PaymentModels", t => t.PaymentModelId)
                .ForeignKey("dbo.PriceRegulations", t => t.PriceRegulationId)
                .ForeignKey("dbo.ProcurementStrategies", t => t.ProcurementStrategyId)
                .ForeignKey("dbo.PurchaseForms", t => t.PurchaseFormId)
                .ForeignKey("dbo.OrganizationUnit", t => t.ResponsibleOrganizationUnitId)
                .ForeignKey("dbo.Organization", t => t.SupplierId)
                .ForeignKey("dbo.TerminationDeadlines", t => t.TerminationDeadlineId)
                .Index(t => t.ContractSignerId)
                .Index(t => t.ResponsibleOrganizationUnitId)
                .Index(t => t.OrganizationId)
                .Index(t => t.SupplierId)
                .Index(t => t.ProcurementStrategyId)
                .Index(t => t.ContractTemplateId)
                .Index(t => t.ContractTypeId)
                .Index(t => t.PurchaseFormId)
                .Index(t => t.ParentId)
                .Index(t => t.TerminationDeadlineId)
                .Index(t => t.OptionExtendId)
                .Index(t => t.PaymentFreqencyId)
                .Index(t => t.PaymentModelId)
                .Index(t => t.PriceRegulationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.Advice",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsActive = c.Boolean(nullable: false),
                        Name = c.String(unicode: false),
                        AlarmDate = c.DateTime(precision: 0),
                        SentDate = c.DateTime(precision: 0),
                        ReceiverId = c.Int(),
                        CarbonCopyReceiverId = c.Int(),
                        Subject = c.String(unicode: false),
                        ItContractId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContractRoles", t => t.CarbonCopyReceiverId)
                .ForeignKey("dbo.ItContract", t => t.ItContractId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.ItContractRoles", t => t.ReceiverId)
                .Index(t => t.ReceiverId)
                .Index(t => t.CarbonCopyReceiverId)
                .Index(t => t.ItContractId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItContractRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItContractRights",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        ObjectId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.ItContract", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.ItContractRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.ObjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.AgreementElements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItContractItSystemUsages",
                c => new
                    {
                        ItContractId = c.Int(nullable: false),
                        ItSystemUsageId = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(),
                    })
                .PrimaryKey(t => new { t.ItContractId, t.ItSystemUsageId })
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsageId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id)
                .ForeignKey("dbo.ItContract", t => t.ItContractId, cascadeDelete: true)
                .Index(t => t.ItContractId)
                .Index(t => t.ItSystemUsageId)
                .Index(t => t.ItSystemUsage_Id);
            
            CreateTable(
                "dbo.ItSystemUsage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsStatusActive = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        LocalSystemId = c.String(unicode: false),
                        Version = c.String(unicode: false),
                        EsdhRef = c.String(unicode: false),
                        CmdbRef = c.String(unicode: false),
                        DirectoryOrUrlRef = c.String(unicode: false),
                        LocalCallName = c.String(unicode: false),
                        OrganizationId = c.Int(nullable: false),
                        ItSystemId = c.Int(nullable: false),
                        ArchiveTypeId = c.Int(),
                        SensitiveDataTypeId = c.Int(),
                        OverviewId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                        OrganizationUnit_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ArchiveTypes", t => t.ArchiveTypeId)
                .ForeignKey("dbo.OrganizationUnit", t => t.OrganizationUnit_Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemUsage", t => t.OverviewId)
                .ForeignKey("dbo.SensitiveDataTypes", t => t.SensitiveDataTypeId)
                .Index(t => t.OrganizationId)
                .Index(t => t.ItSystemId)
                .Index(t => t.ArchiveTypeId)
                .Index(t => t.SensitiveDataTypeId)
                .Index(t => t.OverviewId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.OrganizationUnit_Id);
            
            CreateTable(
                "dbo.ArchiveTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItProject",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProjectId = c.String(unicode: false),
                        Background = c.String(unicode: false),
                        Note = c.String(unicode: false),
                        Name = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        AccessModifier = c.Int(nullable: false),
                        IsArchived = c.Boolean(nullable: false),
                        Esdh = c.String(unicode: false),
                        Cmdb = c.String(unicode: false),
                        Folder = c.String(unicode: false),
                        ParentId = c.Int(),
                        ItProjectTypeId = c.Int(nullable: false),
                        OrganizationId = c.Int(nullable: false),
                        Priority = c.Int(nullable: false),
                        IsPriorityLocked = c.Boolean(nullable: false),
                        PriorityPf = c.Int(nullable: false),
                        IsTransversal = c.Boolean(nullable: false),
                        IsStatusGoalVisible = c.Boolean(nullable: false),
                        IsStrategyVisible = c.Boolean(nullable: false),
                        IsRiskVisible = c.Boolean(nullable: false),
                        IsHierarchyVisible = c.Boolean(nullable: false),
                        IsEconomyVisible = c.Boolean(nullable: false),
                        IsStakeholderVisible = c.Boolean(nullable: false),
                        IsCommunicationVisible = c.Boolean(nullable: false),
                        IsHandoverVisible = c.Boolean(nullable: false),
                        IsStrategy = c.Boolean(nullable: false),
                        JointMunicipalProjectId = c.Int(),
                        CommonPublicProjectId = c.Int(),
                        StatusProject = c.Int(nullable: false),
                        StatusDate = c.DateTime(precision: 0),
                        StatusNote = c.String(unicode: false),
                        CurrentPhaseId = c.Int(),
                        OriginalId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                        Phase1_Id = c.Int(),
                        Phase2_Id = c.Int(),
                        Phase3_Id = c.Int(),
                        Phase4_Id = c.Int(),
                        Phase5_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.CommonPublicProjectId)
                .ForeignKey("dbo.ItProjectTypes", t => t.ItProjectTypeId, cascadeDelete: true)
                .ForeignKey("dbo.ItProject", t => t.JointMunicipalProjectId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .ForeignKey("dbo.ItProject", t => t.OriginalId)
                .ForeignKey("dbo.ItProject", t => t.ParentId)
                .ForeignKey("dbo.ItProjectPhases", t => t.Phase1_Id)
                .ForeignKey("dbo.ItProjectPhases", t => t.Phase2_Id)
                .ForeignKey("dbo.ItProjectPhases", t => t.Phase3_Id)
                .ForeignKey("dbo.ItProjectPhases", t => t.Phase4_Id)
                .ForeignKey("dbo.ItProjectPhases", t => t.Phase5_Id)
                .Index(t => t.ParentId)
                .Index(t => t.ItProjectTypeId)
                .Index(t => t.OrganizationId)
                .Index(t => t.JointMunicipalProjectId)
                .Index(t => t.CommonPublicProjectId)
                .Index(t => t.OriginalId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.Phase1_Id)
                .Index(t => t.Phase2_Id)
                .Index(t => t.Phase3_Id)
                .Index(t => t.Phase4_Id)
                .Index(t => t.Phase5_Id);
            
            CreateTable(
                "dbo.Communication",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TargetAudiance = c.String(unicode: false),
                        Purpose = c.String(unicode: false),
                        Message = c.String(unicode: false),
                        Media = c.String(unicode: false),
                        DueDate = c.DateTime(precision: 0),
                        ResponsibleUserId = c.Int(),
                        ItProjectId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ResponsibleUserId)
                .Index(t => t.ResponsibleUserId)
                .Index(t => t.ItProjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.EconomyYears",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        YearNumber = c.Int(nullable: false),
                        ItProjectId = c.Int(nullable: false),
                        ConsultantBudget = c.Int(nullable: false),
                        ConsultantRea = c.Int(nullable: false),
                        EducationBudget = c.Int(nullable: false),
                        EducationRea = c.Int(nullable: false),
                        OtherBusinessExpensesBudget = c.Int(nullable: false),
                        OtherBusinessExpensesRea = c.Int(nullable: false),
                        IncreasedBusinessExpensesBudget = c.Int(nullable: false),
                        IncreasedBusinessExpensesRea = c.Int(nullable: false),
                        HardwareBudget = c.Int(nullable: false),
                        HardwareRea = c.Int(nullable: false),
                        SoftwareBudget = c.Int(nullable: false),
                        SoftwareRea = c.Int(nullable: false),
                        OtherItExpensesBudget = c.Int(nullable: false),
                        OtherItExpensesRea = c.Int(nullable: false),
                        IncreasedItExpensesBudget = c.Int(nullable: false),
                        IncreasedItExpensesRea = c.Int(nullable: false),
                        SalaryBudget = c.Int(nullable: false),
                        SalaryRea = c.Int(nullable: false),
                        OtherBusinessSavingsBudget = c.Int(nullable: false),
                        OtherBusinessSavingsRea = c.Int(nullable: false),
                        LicenseSavingsBudget = c.Int(nullable: false),
                        LicenseSavingsRea = c.Int(nullable: false),
                        SystemMaintenanceSavingsBudget = c.Int(nullable: false),
                        SystemMaintenanceSavingsRea = c.Int(nullable: false),
                        OtherItSavingsBudget = c.Int(nullable: false),
                        OtherItSavingsRea = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId, cascadeDelete: true)
                .Index(t => t.ItProjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.GoalStatus",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        StatusDate = c.DateTime(precision: 0),
                        StatusNote = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.Id, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.Goal",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HumanReadableId = c.String(unicode: false),
                        Name = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        Note = c.String(unicode: false),
                        Measurable = c.Boolean(nullable: false),
                        Status = c.Int(nullable: false),
                        GoalTypeId = c.Int(nullable: false),
                        GoalStatusId = c.Int(nullable: false),
                        SubGoalDate1 = c.DateTime(precision: 0),
                        SubGoalDate2 = c.DateTime(precision: 0),
                        SubGoalDate3 = c.DateTime(precision: 0),
                        SubGoal1 = c.String(unicode: false),
                        SubGoal2 = c.String(unicode: false),
                        SubGoal3 = c.String(unicode: false),
                        SubGoalRea1 = c.String(unicode: false),
                        SubGoalRea2 = c.String(unicode: false),
                        SubGoalRea3 = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GoalStatus", t => t.GoalStatusId, cascadeDelete: true)
                .ForeignKey("dbo.GoalTypes", t => t.GoalTypeId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.GoalTypeId)
                .Index(t => t.GoalStatusId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.GoalTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.Handover",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Description = c.String(unicode: false),
                        MeetingDate = c.DateTime(precision: 0),
                        Summary = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.Id, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItProjectStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HumanReadableId = c.String(unicode: false),
                        Name = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        Note = c.String(unicode: false),
                        TimeEstimate = c.Int(nullable: false),
                        AssociatedUserId = c.Int(),
                        AssociatedItProjectId = c.Int(nullable: false),
                        AssociatedPhaseId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                        StartDate = c.DateTime(precision: 0),
                        EndDate = c.DateTime(precision: 0),
                        StatusProcentage = c.Int(),
                        Date = c.DateTime(precision: 0),
                        Status = c.Int(),
                        Discriminator = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.AssociatedItProjectId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.AssociatedUserId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.AssociatedUserId)
                .Index(t => t.AssociatedItProjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItProjectTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItProjectPhases",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        StartDate = c.DateTime(precision: 0),
                        EndDate = c.DateTime(precision: 0),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItProjectOrgUnitUsages",
                c => new
                    {
                        ItProjectId = c.Int(nullable: false),
                        OrganizationUnitId = c.Int(nullable: false),
                        ItProject_Id = c.Int(),
                    })
                .PrimaryKey(t => new { t.ItProjectId, t.OrganizationUnitId })
                .ForeignKey("dbo.OrganizationUnit", t => t.OrganizationUnitId, cascadeDelete: true)
                .ForeignKey("dbo.ItProject", t => t.ItProject_Id)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId, cascadeDelete: true)
                .Index(t => t.ItProjectId)
                .Index(t => t.OrganizationUnitId)
                .Index(t => t.ItProject_Id);
            
            CreateTable(
                "dbo.OrganizationUnit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        Ean = c.Long(),
                        ParentId = c.Int(),
                        OrganizationId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("dbo.OrganizationUnit", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.ParentId)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.EconomyStream",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExternPaymentForId = c.Int(),
                        InternPaymentForId = c.Int(),
                        OrganizationUnitId = c.Int(),
                        Acquisition = c.Int(nullable: false),
                        Operation = c.Int(nullable: false),
                        Other = c.Int(nullable: false),
                        AccountingEntry = c.String(unicode: false),
                        AuditStatus = c.Int(nullable: false),
                        AuditDate = c.DateTime(precision: 0),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContract", t => t.ExternPaymentForId, cascadeDelete: true)
                .ForeignKey("dbo.ItContract", t => t.InternPaymentForId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.OrganizationUnit", t => t.OrganizationUnitId)
                .Index(t => t.ExternPaymentForId)
                .Index(t => t.InternPaymentForId)
                .Index(t => t.OrganizationUnitId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.TaskRef",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccessModifier = c.Int(nullable: false),
                        Uuid = c.Guid(nullable: false),
                        Type = c.String(unicode: false),
                        TaskKey = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        ActiveFrom = c.DateTime(precision: 0),
                        ActiveTo = c.DateTime(precision: 0),
                        ParentId = c.Int(),
                        OwnedByOrganizationUnitId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                        OrganizationUnit_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.OrganizationUnit", t => t.OwnedByOrganizationUnitId, cascadeDelete: true)
                .ForeignKey("dbo.TaskRef", t => t.ParentId)
                .ForeignKey("dbo.OrganizationUnit", t => t.OrganizationUnit_Id)
                .Index(t => t.ParentId)
                .Index(t => t.OwnedByOrganizationUnitId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.OrganizationUnit_Id);
            
            CreateTable(
                "dbo.TaskUsage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TaskRefId = c.Int(nullable: false),
                        OrgUnitId = c.Int(nullable: false),
                        ParentId = c.Int(),
                        Starred = c.Boolean(nullable: false),
                        TechnologyStatus = c.Int(nullable: false),
                        UsageStatus = c.Int(nullable: false),
                        Comment = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.OrganizationUnit", t => t.OrgUnitId, cascadeDelete: true)
                .ForeignKey("dbo.TaskUsage", t => t.ParentId, cascadeDelete: true)
                .ForeignKey("dbo.TaskRef", t => t.TaskRefId, cascadeDelete: true)
                .Index(t => t.TaskRefId)
                .Index(t => t.OrgUnitId)
                .Index(t => t.ParentId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.OrganizationRights",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        ObjectId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.OrganizationUnit", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.OrganizationRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.ObjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.OrganizationRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.itusageorgusage",
                c => new
                    {
                        ItSystemUsageId = c.Int(nullable: false),
                        OrganizationUnitId = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(),
                    })
                .PrimaryKey(t => new { t.ItSystemUsageId, t.OrganizationUnitId })
                .ForeignKey("dbo.OrganizationUnit", t => t.OrganizationUnitId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsageId, cascadeDelete: true)
                .Index(t => t.ItSystemUsageId)
                .Index(t => t.OrganizationUnitId)
                .Index(t => t.ItSystemUsage_Id);
            
            CreateTable(
                "dbo.ItProjectRights",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        ObjectId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.ItProject", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.ItProjectRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.ObjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItProjectRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.Risk",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProjectId = c.Int(nullable: false),
                        Name = c.String(unicode: false),
                        Action = c.String(unicode: false),
                        Probability = c.Int(nullable: false),
                        Consequence = c.Int(nullable: false),
                        ResponsibleUserId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ResponsibleUserId)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId, cascadeDelete: true)
                .Index(t => t.ItProjectId)
                .Index(t => t.ResponsibleUserId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.Stakeholder",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProjectId = c.Int(nullable: false),
                        Name = c.String(unicode: false),
                        Role = c.String(unicode: false),
                        Downsides = c.String(unicode: false),
                        Benefits = c.String(unicode: false),
                        Significance = c.Int(nullable: false),
                        HowToHandle = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.ItProject", t => t.ItProjectId, cascadeDelete: true)
                .Index(t => t.ItProjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItSystemRights",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        ObjectId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemUsage", t => t.ObjectId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.ObjectId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItSystemRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.SensitiveDataTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.Wish",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsPublic = c.Boolean(nullable: false),
                        Text = c.String(unicode: false),
                        UserId = c.Int(nullable: false),
                        ItSystemUsageId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                        ItSystem_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsageId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id)
                .Index(t => t.UserId)
                .Index(t => t.ItSystemUsageId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.ItSystem_Id);
            
            CreateTable(
                "dbo.ContractTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ContractTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.HandoverTrial",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Expected = c.DateTime(precision: 0),
                        Approved = c.DateTime(precision: 0),
                        ItContractId = c.Int(nullable: false),
                        HandoverTrialTypeId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.HandoverTrialTypes", t => t.HandoverTrialTypeId)
                .ForeignKey("dbo.ItContract", t => t.ItContractId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ItContractId)
                .Index(t => t.HandoverTrialTypeId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.HandoverTrialTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.OptionExtends",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.PaymentFreqencies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.PaymentMilestones",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(unicode: false),
                        Expected = c.DateTime(precision: 0),
                        Approved = c.DateTime(precision: 0),
                        ItContractId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContract", t => t.ItContractId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ItContractId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.PaymentModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.PriceRegulations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ProcurementStrategies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.PurchaseForms",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.TerminationDeadlines",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.Interfaces",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.InterfaceTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.Methods",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.Tsas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.Frequencies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
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
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.Organization", t => t.Id, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.PasswordResetRequest",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Hash = c.String(unicode: false),
                        Time = c.DateTime(nullable: false, precision: 0),
                        UserId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.AdminRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        HasReadAccess = c.Boolean(nullable: false),
                        HasWriteAccess = c.Boolean(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.Text",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(unicode: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId, cascadeDelete: true)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItContractAgreementElements",
                c => new
                    {
                        ItContractId = c.Int(nullable: false),
                        ElemId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItContractId, t.ElemId })
                .ForeignKey("dbo.ItContract", t => t.ItContractId, cascadeDelete: true)
                .ForeignKey("dbo.AgreementElements", t => t.ElemId, cascadeDelete: true)
                .Index(t => t.ItContractId)
                .Index(t => t.ElemId);
            
            CreateTable(
                "dbo.HandoverUsers",
                c => new
                    {
                        Handover_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Handover_Id, t.User_Id })
                .ForeignKey("dbo.Handover", t => t.Handover_Id, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Handover_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.ItProjectItSystemUsages",
                c => new
                    {
                        ItProject_Id = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItProject_Id, t.ItSystemUsage_Id })
                .ForeignKey("dbo.ItProject", t => t.ItProject_Id, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id, cascadeDelete: true)
                .Index(t => t.ItProject_Id)
                .Index(t => t.ItSystemUsage_Id);
            
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
                "dbo.TaskRefItSystemUsages",
                c => new
                    {
                        TaskRef_Id = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TaskRef_Id, t.ItSystemUsage_Id })
                .ForeignKey("dbo.TaskRef", t => t.TaskRef_Id, cascadeDelete: true)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id, cascadeDelete: true)
                .Index(t => t.TaskRef_Id)
                .Index(t => t.ItSystemUsage_Id);
            
            CreateTable(
                "dbo.ItProjectTaskRefs",
                c => new
                    {
                        ItProject_Id = c.Int(nullable: false),
                        TaskRef_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItProject_Id, t.TaskRef_Id })
                .ForeignKey("dbo.ItProject", t => t.ItProject_Id, cascadeDelete: true)
                .ForeignKey("dbo.TaskRef", t => t.TaskRef_Id, cascadeDelete: true)
                .Index(t => t.ItProject_Id)
                .Index(t => t.TaskRef_Id);
            
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
            DropForeignKey("dbo.Text", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Text", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.AdminRights", "UserId", "dbo.User");
            DropForeignKey("dbo.AdminRights", "RoleId", "dbo.AdminRoles");
            DropForeignKey("dbo.AdminRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.AdminRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.AdminRights", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.AdminRights", "ObjectId", "dbo.Organization");
            DropForeignKey("dbo.AdminRights", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.PasswordResetRequest", "UserId", "dbo.User");
            DropForeignKey("dbo.PasswordResetRequest", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.PasswordResetRequest", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.User", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.User", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.User", "DefaultOrganizationUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.User", "CreatedInId", "dbo.Organization");
            DropForeignKey("dbo.Organization", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Organization", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Config", "Id", "dbo.Organization");
            DropForeignKey("dbo.Config", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Config", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Wish", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.ItSystem", "ParentId", "dbo.ItSystem");
            DropForeignKey("dbo.ItSystem", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.ItSystem", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItSystem", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Exhibit", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.ItInterfaceUses", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.InfUsage", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.InfUsage", new[] { "ItSystemId", "ItInterfaceId" }, "dbo.ItInterfaceUses");
            DropForeignKey("dbo.InfUsage", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.InfUsage", "InfrastructureId", "dbo.ItSystem");
            DropForeignKey("dbo.DataRowUsage", new[] { "SysUsageId", "SysId", "IntfId" }, "dbo.InfUsage");
            DropForeignKey("dbo.DataRowUsage", "FrequencyId", "dbo.Frequencies");
            DropForeignKey("dbo.Frequencies", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Frequencies", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataRowUsage", "DataRowId", "dbo.DataRow");
            DropForeignKey("dbo.DataRow", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataRow", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataRow", "ItInterfaceId", "dbo.ItInterface");
            DropForeignKey("dbo.ItInterface", "TsaId", "dbo.Tsas");
            DropForeignKey("dbo.Tsas", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Tsas", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItInterface", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.ItInterface", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItInterface", "MethodId", "dbo.Methods");
            DropForeignKey("dbo.Methods", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Methods", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItInterface", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItInterface", "InterfaceTypeId", "dbo.InterfaceTypes");
            DropForeignKey("dbo.InterfaceTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.InterfaceTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.InfUsage", "ItInterfaceId", "dbo.ItInterface");
            DropForeignKey("dbo.ItInterfaceExhibitUsage", "ItInterface_Id", "dbo.ItInterface");
            DropForeignKey("dbo.ItInterface", "InterfaceId", "dbo.Interfaces");
            DropForeignKey("dbo.Interfaces", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Interfaces", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Exhibit", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Exhibit", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItInterfaceExhibitUsage", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItInterfaceExhibitUsage", "ItInterfaceExhibitId", "dbo.Exhibit");
            DropForeignKey("dbo.ItInterfaceExhibitUsage", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.ItContract", "TerminationDeadlineId", "dbo.TerminationDeadlines");
            DropForeignKey("dbo.TerminationDeadlines", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.TerminationDeadlines", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "SupplierId", "dbo.Organization");
            DropForeignKey("dbo.ItContract", "ResponsibleOrganizationUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.ItContract", "PurchaseFormId", "dbo.PurchaseForms");
            DropForeignKey("dbo.PurchaseForms", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.PurchaseForms", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "ProcurementStrategyId", "dbo.ProcurementStrategies");
            DropForeignKey("dbo.ProcurementStrategies", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ProcurementStrategies", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "PriceRegulationId", "dbo.PriceRegulations");
            DropForeignKey("dbo.PriceRegulations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.PriceRegulations", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "PaymentModelId", "dbo.PaymentModels");
            DropForeignKey("dbo.PaymentModels", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.PaymentModels", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.PaymentMilestones", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.PaymentMilestones", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.PaymentMilestones", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.ItContract", "PaymentFreqencyId", "dbo.PaymentFreqencies");
            DropForeignKey("dbo.PaymentFreqencies", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.PaymentFreqencies", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "ParentId", "dbo.ItContract");
            DropForeignKey("dbo.ItContract", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.ItContract", "OptionExtendId", "dbo.OptionExtends");
            DropForeignKey("dbo.OptionExtends", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.OptionExtends", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItContract", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.HandoverTrial", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.HandoverTrial", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.HandoverTrial", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.HandoverTrial", "HandoverTrialTypeId", "dbo.HandoverTrialTypes");
            DropForeignKey("dbo.HandoverTrialTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.HandoverTrialTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "ContractTypeId", "dbo.ContractTypes");
            DropForeignKey("dbo.ContractTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ContractTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "ContractTemplateId", "dbo.ContractTemplates");
            DropForeignKey("dbo.ContractTemplates", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ContractTemplates", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContract", "ContractSignerId", "dbo.User");
            DropForeignKey("dbo.ItContractItSystemUsages", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.Wish", "UserId", "dbo.User");
            DropForeignKey("dbo.Wish", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Wish", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Wish", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.itusageorgusage", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsage", "SensitiveDataTypeId", "dbo.SensitiveDataTypes");
            DropForeignKey("dbo.SensitiveDataTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.SensitiveDataTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystemRights", "UserId", "dbo.User");
            DropForeignKey("dbo.ItSystemRights", "RoleId", "dbo.ItSystemRoles");
            DropForeignKey("dbo.ItSystemRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItSystemRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystemRights", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItSystemRights", "ObjectId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemRights", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.itusageorgusage", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsage", "OverviewId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.OrgUnitSystemUsage", "OrganizationUnit_Id", "dbo.OrganizationUnit");
            DropForeignKey("dbo.OrgUnitSystemUsage", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsage", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.ItSystemUsage", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItContractItSystemUsages", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsage", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystemUsage", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.ItProjectOrgUnitUsages", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItProjectTaskRefs", "TaskRef_Id", "dbo.TaskRef");
            DropForeignKey("dbo.ItProjectTaskRefs", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.Stakeholder", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.Stakeholder", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Stakeholder", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Risk", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.Risk", "ResponsibleUserId", "dbo.User");
            DropForeignKey("dbo.Risk", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Risk", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectRights", "UserId", "dbo.User");
            DropForeignKey("dbo.ItProjectRights", "RoleId", "dbo.ItProjectRoles");
            DropForeignKey("dbo.ItProjectRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItProjectRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectRights", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItProjectRights", "ObjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItProjectRights", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectOrgUnitUsages", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.ItProjectOrgUnitUsages", "OrganizationUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.itusageorgusage", "OrganizationUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.TaskRef", "OrganizationUnit_Id", "dbo.OrganizationUnit");
            DropForeignKey("dbo.OrganizationRights", "UserId", "dbo.User");
            DropForeignKey("dbo.OrganizationRights", "RoleId", "dbo.OrganizationRoles");
            DropForeignKey("dbo.OrganizationRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.OrganizationRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.OrganizationRights", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.OrganizationRights", "ObjectId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.OrganizationRights", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.OrganizationUnit", "ParentId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.TaskUsage", "TaskRefId", "dbo.TaskRef");
            DropForeignKey("dbo.TaskUsage", "ParentId", "dbo.TaskUsage");
            DropForeignKey("dbo.TaskUsage", "OrgUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.TaskUsage", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.TaskUsage", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.TaskRef", "ParentId", "dbo.TaskRef");
            DropForeignKey("dbo.TaskRef", "OwnedByOrganizationUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.TaskRef", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.TaskRef", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.TaskRefItSystemUsages", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.TaskRefItSystemUsages", "TaskRef_Id", "dbo.TaskRef");
            DropForeignKey("dbo.TaskRefItSystems", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.TaskRefItSystems", "TaskRef_Id", "dbo.TaskRef");
            DropForeignKey("dbo.OrganizationUnit", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.OrganizationUnit", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.OrganizationUnit", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.EconomyStream", "OrganizationUnitId", "dbo.OrganizationUnit");
            DropForeignKey("dbo.EconomyStream", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.EconomyStream", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.EconomyStream", "InternPaymentForId", "dbo.ItContract");
            DropForeignKey("dbo.EconomyStream", "ExternPaymentForId", "dbo.ItContract");
            DropForeignKey("dbo.ItSystemUsage", "OrganizationUnit_Id", "dbo.OrganizationUnit");
            DropForeignKey("dbo.ItProject", "Phase5_Id", "dbo.ItProjectPhases");
            DropForeignKey("dbo.ItProject", "Phase4_Id", "dbo.ItProjectPhases");
            DropForeignKey("dbo.ItProject", "Phase3_Id", "dbo.ItProjectPhases");
            DropForeignKey("dbo.ItProject", "Phase2_Id", "dbo.ItProjectPhases");
            DropForeignKey("dbo.ItProject", "Phase1_Id", "dbo.ItProjectPhases");
            DropForeignKey("dbo.ItProjectPhases", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItProjectPhases", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProject", "ParentId", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "OriginalId", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.ItProject", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItProject", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProject", "JointMunicipalProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItProjectItSystemUsages", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItProjectItSystemUsages", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "ItProjectTypeId", "dbo.ItProjectTypes");
            DropForeignKey("dbo.ItProjectTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItProjectTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectStatus", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItProjectStatus", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectStatus", "AssociatedUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectStatus", "AssociatedItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.HandoverUsers", "User_Id", "dbo.User");
            DropForeignKey("dbo.HandoverUsers", "Handover_Id", "dbo.Handover");
            DropForeignKey("dbo.Handover", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Handover", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Handover", "Id", "dbo.ItProject");
            DropForeignKey("dbo.GoalStatus", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.GoalStatus", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.GoalStatus", "Id", "dbo.ItProject");
            DropForeignKey("dbo.Goal", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Goal", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Goal", "GoalTypeId", "dbo.GoalTypes");
            DropForeignKey("dbo.GoalTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.GoalTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Goal", "GoalStatusId", "dbo.GoalStatus");
            DropForeignKey("dbo.EconomyYears", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.EconomyYears", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.EconomyYears", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Communication", "ResponsibleUserId", "dbo.User");
            DropForeignKey("dbo.Communication", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Communication", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Communication", "ItProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItProject", "CommonPublicProjectId", "dbo.ItProject");
            DropForeignKey("dbo.ItContractItSystemUsages", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsage", "ArchiveTypeId", "dbo.ArchiveTypes");
            DropForeignKey("dbo.ArchiveTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ArchiveTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContractAgreementElements", "ElemId", "dbo.AgreementElements");
            DropForeignKey("dbo.ItContractAgreementElements", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.AgreementElements", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.AgreementElements", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Advice", "ReceiverId", "dbo.ItContractRoles");
            DropForeignKey("dbo.Advice", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Advice", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Advice", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.Advice", "CarbonCopyReceiverId", "dbo.ItContractRoles");
            DropForeignKey("dbo.ItContractRights", "UserId", "dbo.User");
            DropForeignKey("dbo.ItContractRights", "RoleId", "dbo.ItContractRoles");
            DropForeignKey("dbo.ItContractRights", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItContractRights", "ObjectId", "dbo.ItContract");
            DropForeignKey("dbo.ItContractRights", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItContractRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItContractRoles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Exhibit", "Id", "dbo.ItInterface");
            DropForeignKey("dbo.ItInterfaceUses", "ItInterfaceId", "dbo.ItInterface");
            DropForeignKey("dbo.ItInterface", "BelongsToId", "dbo.Organization");
            DropForeignKey("dbo.DataRow", "DataTypeId", "dbo.DataTypes");
            DropForeignKey("dbo.DataTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystem", "BusinessTypeId", "dbo.BusinessTypes");
            DropForeignKey("dbo.BusinessTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.BusinessTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystem", "BelongsToId", "dbo.Organization");
            DropForeignKey("dbo.ItSystem", "AppTypeOptionId", "dbo.ItSystemTypeOptions");
            DropForeignKey("dbo.ItSystemTypeOptions", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItSystemTypeOptions", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.OrgUnitSystemUsage", new[] { "OrganizationUnit_Id" });
            DropIndex("dbo.OrgUnitSystemUsage", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.ItProjectTaskRefs", new[] { "TaskRef_Id" });
            DropIndex("dbo.ItProjectTaskRefs", new[] { "ItProject_Id" });
            DropIndex("dbo.TaskRefItSystemUsages", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.TaskRefItSystemUsages", new[] { "TaskRef_Id" });
            DropIndex("dbo.TaskRefItSystems", new[] { "ItSystem_Id" });
            DropIndex("dbo.TaskRefItSystems", new[] { "TaskRef_Id" });
            DropIndex("dbo.ItProjectItSystemUsages", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.ItProjectItSystemUsages", new[] { "ItProject_Id" });
            DropIndex("dbo.HandoverUsers", new[] { "User_Id" });
            DropIndex("dbo.HandoverUsers", new[] { "Handover_Id" });
            DropIndex("dbo.ItContractAgreementElements", new[] { "ElemId" });
            DropIndex("dbo.ItContractAgreementElements", new[] { "ItContractId" });
            DropIndex("dbo.Text", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Text", new[] { "ObjectOwnerId" });
            DropIndex("dbo.AdminRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.AdminRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PasswordResetRequest", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PasswordResetRequest", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PasswordResetRequest", new[] { "UserId" });
            DropIndex("dbo.Config", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Config", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Config", new[] { "Id" });
            DropIndex("dbo.Frequencies", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Frequencies", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Tsas", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Tsas", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Methods", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Methods", new[] { "ObjectOwnerId" });
            DropIndex("dbo.InterfaceTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.InterfaceTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Interfaces", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Interfaces", new[] { "ObjectOwnerId" });
            DropIndex("dbo.TerminationDeadlines", new[] { "LastChangedByUserId" });
            DropIndex("dbo.TerminationDeadlines", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PurchaseForms", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PurchaseForms", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ProcurementStrategies", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ProcurementStrategies", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PriceRegulations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PriceRegulations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PaymentModels", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PaymentModels", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PaymentMilestones", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PaymentMilestones", new[] { "ObjectOwnerId" });
            DropIndex("dbo.PaymentMilestones", new[] { "ItContractId" });
            DropIndex("dbo.PaymentFreqencies", new[] { "LastChangedByUserId" });
            DropIndex("dbo.PaymentFreqencies", new[] { "ObjectOwnerId" });
            DropIndex("dbo.OptionExtends", new[] { "LastChangedByUserId" });
            DropIndex("dbo.OptionExtends", new[] { "ObjectOwnerId" });
            DropIndex("dbo.HandoverTrialTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.HandoverTrialTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.HandoverTrial", new[] { "LastChangedByUserId" });
            DropIndex("dbo.HandoverTrial", new[] { "ObjectOwnerId" });
            DropIndex("dbo.HandoverTrial", new[] { "HandoverTrialTypeId" });
            DropIndex("dbo.HandoverTrial", new[] { "ItContractId" });
            DropIndex("dbo.ContractTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ContractTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ContractTemplates", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ContractTemplates", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Wish", new[] { "ItSystem_Id" });
            DropIndex("dbo.Wish", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Wish", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Wish", new[] { "ItSystemUsageId" });
            DropIndex("dbo.Wish", new[] { "UserId" });
            DropIndex("dbo.SensitiveDataTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.SensitiveDataTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItSystemRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemRights", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItSystemRights", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemRights", new[] { "ObjectId" });
            DropIndex("dbo.ItSystemRights", new[] { "RoleId" });
            DropIndex("dbo.ItSystemRights", new[] { "UserId" });
            DropIndex("dbo.Stakeholder", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Stakeholder", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Stakeholder", new[] { "ItProjectId" });
            DropIndex("dbo.Risk", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Risk", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Risk", new[] { "ResponsibleUserId" });
            DropIndex("dbo.Risk", new[] { "ItProjectId" });
            DropIndex("dbo.ItProjectRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItProjectRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProjectRights", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItProjectRights", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProjectRights", new[] { "ObjectId" });
            DropIndex("dbo.ItProjectRights", new[] { "RoleId" });
            DropIndex("dbo.ItProjectRights", new[] { "UserId" });
            DropIndex("dbo.itusageorgusage", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.itusageorgusage", new[] { "OrganizationUnitId" });
            DropIndex("dbo.itusageorgusage", new[] { "ItSystemUsageId" });
            DropIndex("dbo.OrganizationRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.OrganizationRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.OrganizationRights", new[] { "LastChangedByUserId" });
            DropIndex("dbo.OrganizationRights", new[] { "ObjectOwnerId" });
            DropIndex("dbo.OrganizationRights", new[] { "ObjectId" });
            DropIndex("dbo.OrganizationRights", new[] { "RoleId" });
            DropIndex("dbo.OrganizationRights", new[] { "UserId" });
            DropIndex("dbo.TaskUsage", new[] { "LastChangedByUserId" });
            DropIndex("dbo.TaskUsage", new[] { "ObjectOwnerId" });
            DropIndex("dbo.TaskUsage", new[] { "ParentId" });
            DropIndex("dbo.TaskUsage", new[] { "OrgUnitId" });
            DropIndex("dbo.TaskUsage", new[] { "TaskRefId" });
            DropIndex("dbo.TaskRef", new[] { "OrganizationUnit_Id" });
            DropIndex("dbo.TaskRef", new[] { "LastChangedByUserId" });
            DropIndex("dbo.TaskRef", new[] { "ObjectOwnerId" });
            DropIndex("dbo.TaskRef", new[] { "OwnedByOrganizationUnitId" });
            DropIndex("dbo.TaskRef", new[] { "ParentId" });
            DropIndex("dbo.EconomyStream", new[] { "LastChangedByUserId" });
            DropIndex("dbo.EconomyStream", new[] { "ObjectOwnerId" });
            DropIndex("dbo.EconomyStream", new[] { "OrganizationUnitId" });
            DropIndex("dbo.EconomyStream", new[] { "InternPaymentForId" });
            DropIndex("dbo.EconomyStream", new[] { "ExternPaymentForId" });
            DropIndex("dbo.OrganizationUnit", new[] { "LastChangedByUserId" });
            DropIndex("dbo.OrganizationUnit", new[] { "ObjectOwnerId" });
            DropIndex("dbo.OrganizationUnit", new[] { "OrganizationId" });
            DropIndex("dbo.OrganizationUnit", new[] { "ParentId" });
            DropIndex("dbo.ItProjectOrgUnitUsages", new[] { "ItProject_Id" });
            DropIndex("dbo.ItProjectOrgUnitUsages", new[] { "OrganizationUnitId" });
            DropIndex("dbo.ItProjectOrgUnitUsages", new[] { "ItProjectId" });
            DropIndex("dbo.ItProjectPhases", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItProjectPhases", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProjectTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItProjectTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProjectStatus", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItProjectStatus", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProjectStatus", new[] { "AssociatedItProjectId" });
            DropIndex("dbo.ItProjectStatus", new[] { "AssociatedUserId" });
            DropIndex("dbo.Handover", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Handover", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Handover", new[] { "Id" });
            DropIndex("dbo.GoalTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.GoalTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Goal", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Goal", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Goal", new[] { "GoalStatusId" });
            DropIndex("dbo.Goal", new[] { "GoalTypeId" });
            DropIndex("dbo.GoalStatus", new[] { "LastChangedByUserId" });
            DropIndex("dbo.GoalStatus", new[] { "ObjectOwnerId" });
            DropIndex("dbo.GoalStatus", new[] { "Id" });
            DropIndex("dbo.EconomyYears", new[] { "LastChangedByUserId" });
            DropIndex("dbo.EconomyYears", new[] { "ObjectOwnerId" });
            DropIndex("dbo.EconomyYears", new[] { "ItProjectId" });
            DropIndex("dbo.Communication", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Communication", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Communication", new[] { "ItProjectId" });
            DropIndex("dbo.Communication", new[] { "ResponsibleUserId" });
            DropIndex("dbo.ItProject", new[] { "Phase5_Id" });
            DropIndex("dbo.ItProject", new[] { "Phase4_Id" });
            DropIndex("dbo.ItProject", new[] { "Phase3_Id" });
            DropIndex("dbo.ItProject", new[] { "Phase2_Id" });
            DropIndex("dbo.ItProject", new[] { "Phase1_Id" });
            DropIndex("dbo.ItProject", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItProject", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProject", new[] { "OriginalId" });
            DropIndex("dbo.ItProject", new[] { "CommonPublicProjectId" });
            DropIndex("dbo.ItProject", new[] { "JointMunicipalProjectId" });
            DropIndex("dbo.ItProject", new[] { "OrganizationId" });
            DropIndex("dbo.ItProject", new[] { "ItProjectTypeId" });
            DropIndex("dbo.ItProject", new[] { "ParentId" });
            DropIndex("dbo.ArchiveTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ArchiveTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemUsage", new[] { "OrganizationUnit_Id" });
            DropIndex("dbo.ItSystemUsage", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItSystemUsage", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemUsage", new[] { "OverviewId" });
            DropIndex("dbo.ItSystemUsage", new[] { "SensitiveDataTypeId" });
            DropIndex("dbo.ItSystemUsage", new[] { "ArchiveTypeId" });
            DropIndex("dbo.ItSystemUsage", new[] { "ItSystemId" });
            DropIndex("dbo.ItSystemUsage", new[] { "OrganizationId" });
            DropIndex("dbo.ItContractItSystemUsages", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.ItContractItSystemUsages", new[] { "ItSystemUsageId" });
            DropIndex("dbo.ItContractItSystemUsages", new[] { "ItContractId" });
            DropIndex("dbo.AgreementElements", new[] { "LastChangedByUserId" });
            DropIndex("dbo.AgreementElements", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItContractRights", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItContractRights", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItContractRights", new[] { "ObjectId" });
            DropIndex("dbo.ItContractRights", new[] { "RoleId" });
            DropIndex("dbo.ItContractRights", new[] { "UserId" });
            DropIndex("dbo.ItContractRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItContractRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Advice", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Advice", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Advice", new[] { "ItContractId" });
            DropIndex("dbo.Advice", new[] { "CarbonCopyReceiverId" });
            DropIndex("dbo.Advice", new[] { "ReceiverId" });
            DropIndex("dbo.ItContract", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItContract", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItContract", new[] { "PriceRegulationId" });
            DropIndex("dbo.ItContract", new[] { "PaymentModelId" });
            DropIndex("dbo.ItContract", new[] { "PaymentFreqencyId" });
            DropIndex("dbo.ItContract", new[] { "OptionExtendId" });
            DropIndex("dbo.ItContract", new[] { "TerminationDeadlineId" });
            DropIndex("dbo.ItContract", new[] { "ParentId" });
            DropIndex("dbo.ItContract", new[] { "PurchaseFormId" });
            DropIndex("dbo.ItContract", new[] { "ContractTypeId" });
            DropIndex("dbo.ItContract", new[] { "ContractTemplateId" });
            DropIndex("dbo.ItContract", new[] { "ProcurementStrategyId" });
            DropIndex("dbo.ItContract", new[] { "SupplierId" });
            DropIndex("dbo.ItContract", new[] { "OrganizationId" });
            DropIndex("dbo.ItContract", new[] { "ResponsibleOrganizationUnitId" });
            DropIndex("dbo.ItContract", new[] { "ContractSignerId" });
            DropIndex("dbo.ItInterfaceExhibitUsage", new[] { "ItInterface_Id" });
            DropIndex("dbo.ItInterfaceExhibitUsage", new[] { "ItContractId" });
            DropIndex("dbo.ItInterfaceExhibitUsage", new[] { "ItInterfaceExhibitId" });
            DropIndex("dbo.ItInterfaceExhibitUsage", new[] { "ItSystemUsageId" });
            DropIndex("dbo.Exhibit", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Exhibit", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Exhibit", new[] { "ItSystemId" });
            DropIndex("dbo.Exhibit", new[] { "Id" });
            DropIndex("dbo.ItInterface", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItInterface", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItInterface", new[] { "BelongsToId" });
            DropIndex("dbo.ItInterface", new[] { "OrganizationId" });
            DropIndex("dbo.ItInterface", new[] { "MethodId" });
            DropIndex("dbo.ItInterface", new[] { "TsaId" });
            DropIndex("dbo.ItInterface", new[] { "InterfaceTypeId" });
            DropIndex("dbo.ItInterface", new[] { "InterfaceId" });
            DropIndex("dbo.DataTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataRow", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataRow", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataRow", new[] { "DataTypeId" });
            DropIndex("dbo.DataRow", new[] { "ItInterfaceId" });
            DropIndex("dbo.DataRowUsage", new[] { "FrequencyId" });
            DropIndex("dbo.DataRowUsage", new[] { "SysUsageId", "SysId", "IntfId" });
            DropIndex("dbo.DataRowUsage", new[] { "DataRowId" });
            DropIndex("dbo.InfUsage", new[] { "InfrastructureId" });
            DropIndex("dbo.InfUsage", new[] { "ItContractId" });
            DropIndex("dbo.InfUsage", new[] { "ItSystemId", "ItInterfaceId" });
            DropIndex("dbo.InfUsage", new[] { "ItSystemUsageId" });
            DropIndex("dbo.ItInterfaceUses", new[] { "ItInterfaceId" });
            DropIndex("dbo.ItInterfaceUses", new[] { "ItSystemId" });
            DropIndex("dbo.BusinessTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.BusinessTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemTypeOptions", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItSystemTypeOptions", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystem", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItSystem", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystem", new[] { "BelongsToId" });
            DropIndex("dbo.ItSystem", new[] { "OrganizationId" });
            DropIndex("dbo.ItSystem", new[] { "BusinessTypeId" });
            DropIndex("dbo.ItSystem", new[] { "ParentId" });
            DropIndex("dbo.ItSystem", new[] { "AppTypeOptionId" });
            DropIndex("dbo.Organization", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Organization", new[] { "ObjectOwnerId" });
            DropIndex("dbo.User", new[] { "LastChangedByUserId" });
            DropIndex("dbo.User", new[] { "ObjectOwnerId" });
            DropIndex("dbo.User", new[] { "DefaultOrganizationUnitId" });
            DropIndex("dbo.User", new[] { "CreatedInId" });
            DropIndex("dbo.AdminRights", new[] { "LastChangedByUserId" });
            DropIndex("dbo.AdminRights", new[] { "ObjectOwnerId" });
            DropIndex("dbo.AdminRights", new[] { "ObjectId" });
            DropIndex("dbo.AdminRights", new[] { "RoleId" });
            DropIndex("dbo.AdminRights", new[] { "UserId" });
            DropTable("dbo.OrgUnitSystemUsage");
            DropTable("dbo.ItProjectTaskRefs");
            DropTable("dbo.TaskRefItSystemUsages");
            DropTable("dbo.TaskRefItSystems");
            DropTable("dbo.ItProjectItSystemUsages");
            DropTable("dbo.HandoverUsers");
            DropTable("dbo.ItContractAgreementElements");
            DropTable("dbo.Text");
            DropTable("dbo.AdminRoles");
            DropTable("dbo.PasswordResetRequest");
            DropTable("dbo.Config");
            DropTable("dbo.Frequencies");
            DropTable("dbo.Tsas");
            DropTable("dbo.Methods");
            DropTable("dbo.InterfaceTypes");
            DropTable("dbo.Interfaces");
            DropTable("dbo.TerminationDeadlines");
            DropTable("dbo.PurchaseForms");
            DropTable("dbo.ProcurementStrategies");
            DropTable("dbo.PriceRegulations");
            DropTable("dbo.PaymentModels");
            DropTable("dbo.PaymentMilestones");
            DropTable("dbo.PaymentFreqencies");
            DropTable("dbo.OptionExtends");
            DropTable("dbo.HandoverTrialTypes");
            DropTable("dbo.HandoverTrial");
            DropTable("dbo.ContractTypes");
            DropTable("dbo.ContractTemplates");
            DropTable("dbo.Wish");
            DropTable("dbo.SensitiveDataTypes");
            DropTable("dbo.ItSystemRoles");
            DropTable("dbo.ItSystemRights");
            DropTable("dbo.Stakeholder");
            DropTable("dbo.Risk");
            DropTable("dbo.ItProjectRoles");
            DropTable("dbo.ItProjectRights");
            DropTable("dbo.itusageorgusage");
            DropTable("dbo.OrganizationRoles");
            DropTable("dbo.OrganizationRights");
            DropTable("dbo.TaskUsage");
            DropTable("dbo.TaskRef");
            DropTable("dbo.EconomyStream");
            DropTable("dbo.OrganizationUnit");
            DropTable("dbo.ItProjectOrgUnitUsages");
            DropTable("dbo.ItProjectPhases");
            DropTable("dbo.ItProjectTypes");
            DropTable("dbo.ItProjectStatus");
            DropTable("dbo.Handover");
            DropTable("dbo.GoalTypes");
            DropTable("dbo.Goal");
            DropTable("dbo.GoalStatus");
            DropTable("dbo.EconomyYears");
            DropTable("dbo.Communication");
            DropTable("dbo.ItProject");
            DropTable("dbo.ArchiveTypes");
            DropTable("dbo.ItSystemUsage");
            DropTable("dbo.ItContractItSystemUsages");
            DropTable("dbo.AgreementElements");
            DropTable("dbo.ItContractRights");
            DropTable("dbo.ItContractRoles");
            DropTable("dbo.Advice");
            DropTable("dbo.ItContract");
            DropTable("dbo.ItInterfaceExhibitUsage");
            DropTable("dbo.Exhibit");
            DropTable("dbo.ItInterface");
            DropTable("dbo.DataTypes");
            DropTable("dbo.DataRow");
            DropTable("dbo.DataRowUsage");
            DropTable("dbo.InfUsage");
            DropTable("dbo.ItInterfaceUses");
            DropTable("dbo.BusinessTypes");
            DropTable("dbo.ItSystemTypeOptions");
            DropTable("dbo.ItSystem");
            DropTable("dbo.Organization");
            DropTable("dbo.User");
            DropTable("dbo.AdminRights");
        }
    }
}
