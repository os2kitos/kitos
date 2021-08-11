﻿module Kitos.Models.ItSystemUsage {
    import ItSystem = Models.ItSystem.IItSystem;

    export interface IItSystemUsage extends IEntity {
        /** Gets or sets the note. */
        Note: string;
        /** Gets or sets the user defined local system identifier. */
        LocalSystemId: string;
        /** Gets or sets the version. */
        Version: string;
        /** Gets or sets a reference to relevant documents in an extern CMDB system. */
        CmdbRef: string;
        /** Gets or sets the local call system. */
        LocalCallName: string;
        /** Organization Unit responsible for this system usage. */
        ResponsibleUsage: IItSystemUsageOrgUnitUsage;
        OrganizationId: number;
        /** Gets or sets the organization marked as responsible for this it system usage. */
        Organization: IOrganization;
        ItSystemId: number;
        /** Gets or sets the it system this instance is using. */
        ItSystem: ItSystem;
        ArchiveTypeId: number;
        ArchiveType: ItSystem.IArchiveType;

        ArchiveLocationId: number;
        ArchiveLocation: ItSystem.IArchiveLocation;

        ArchiveTestLocationId: number;
        ArchiveTestLocation: ItSystem.IArchiveTestLocation;

        SensitiveDataTypeId: number;
        SensitiveDataType: ItSystem.ISensitiveDataType;
        OverviewId: number;
        /** Gets or sets the it system usage that is set to be displayed on the it system overview page. */
        Overview: IItSystemUsage;
        /** Gets or sets the main it contract for this instance.The it contract is used to determine whether this instanceis marked as active/inactive. */
        MainContract: ItContract.IItContractItSystemUsage;
        /** Gets or sets it contracts associated with this instance. */
        Contracts: Array<ItContract.IItContractItSystemUsage>;
        /** Gets or sets the organization units that are using this instance. */
        UsedBy: Array<IItSystemUsageOrgUnitUsage>;
        /** Gets or sets the tasks this instance supports. */
        TaskRefs: Array<ITaskRef>;
        /** Gets or sets the associated it projects. */
        ItProjects: Array<ItProject.IItProject>;

        ArchivePeriods: ItSystem.IArchivePeriode[];

        ReferenceId: number;
        Reference: IReference;

        Concluded?: Date;
        ArchiveDuty: string;
        Registertype: boolean;

        IsActive: boolean;

        RiskSupervisionDocumentationUrl: string;
        RiskSupervisionDocumentationUrlName: string;
        LinkToDirectoryUrl: string;
        LinkToDirectoryUrlName: string;

        SensitiveDataLevels: Models.Odata.ItSystemUsage.IItSystemUsageSensitiveDataLevelDTO[];

        AssociatedDataProcessingRegistrations: Models.OData.DataProcessing.IDataProcessingRegistration[];
    }
}
