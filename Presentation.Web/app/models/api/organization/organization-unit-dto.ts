module Kitos.Models.Api.Organization {
    export interface IOrganizationUnitDto {
        id: number;
        uuid:string;
        name: string;
        ean: string;
        localId: number;
        parentId: number;
        organizationId: number;
        origin: OrganizationUnitOrigin;
        externalOriginUuid: string | null;
        organization?: IOrganizationDto;
        children?: IOrganizationUnitDto[];
    }
}
