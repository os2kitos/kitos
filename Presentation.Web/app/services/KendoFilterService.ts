module Kitos.Services {
    import OverviewType = Models.ItSystem.OverviewType;
    import IKendoOrganizationalConfigurationDTO = Models.ItSystem.IKendoOrganizationalConfigurationDTO;
    "use strict";

    export class KendoFilterService {

        public static $inject: string[] = ["$http"];

        constructor(private $http: IHttpServiceWithCustomConfig) {
        }

        GetSystemFilterOptionFromOrg = (orgId: number, overviewType: OverviewType) => {
            return this.$http
                .get<API.Models.IApiWrapper<IKendoOrganizationalConfigurationDTO>>(
                    `api/v1/kendo-organizational-configuration?organizationId=${orgId}&overviewType=${overviewType}`);
        }

        PostSystemFilterOptionFromOrg = (orgId: number, overviewType: OverviewType, configuration: string) => {

            var payload = {
                OverviewType: overviewType,
                Configuration: configuration,
                OrganizationId: orgId
            }
            return this.$http.post(`api/v1/kendo-organizational-configuration`, payload);

        }
    }

    app.service("KendoFilterService", KendoFilterService);
}
