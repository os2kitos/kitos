module Kitos.Services {
    "use strict";

    export class KendoFilterService {

        public static $inject: string[] = ["$http"];

        constructor(private $http: IHttpServiceWithCustomConfig) {
        }

        GetSystemFilterOptionFromOrg = (orgId: number, overviewType: Models.ItSystem.OverviewType) => {
            return this.$http
                .get<API.Models.IApiWrapper<Models.ItSystem.IKendoOrganizationalConfigurationDTO>>(
                    `api/v1/kendo-organizational-configuration?organizationId=${orgId}&overviewType=${overviewType}`);
        }

        PostSystemFilterOptionFromOrg = (orgId: number, overviewType: Models.ItSystem.OverviewType, configuration: string) => {

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
