module Kitos.Services {
    "use strict";

    export class KendoFilterService {

        public static $inject: string[] = ["$http"];

        constructor(private $http: IHttpServiceWithCustomConfig) {
        }

        getConfigurationFromOrg = (orgId: number, overviewType: Models.Generic.OverviewType) => {
            return this.$http
                .get<API.Models.IApiWrapper<Models.Generic.IKendoOrganizationalConfigurationDTO>>(
                    `api/v1/kendo-organizational-configuration?organizationId=${orgId}&overviewType=${overviewType}`);
        }

        getConfigurationVersion = (orgId: number, overviewType: Models.Generic.OverviewType) => {
            return this.$http
                .get<API.Models.IApiWrapper<string>>(
                    `api/v1/kendo-organizational-configuration/version?organizationId=${orgId}&overviewType=${overviewType}`);
        }

        postConfigurationFromOrg = (orgId: number, overviewType: Models.Generic.OverviewType, columns: Models.Generic.IKendoColumnConfigurationDTO[]) => {

            var payload = {
                OverviewType: overviewType,
                VisibleColumns: columns,
                OrganizationId: orgId
            }
            return this.$http.post(`api/v1/kendo-organizational-configuration`, payload);
        }

        deleteConfigurationFromOrg = (orgId: number, overviewType: Models.Generic.OverviewType) => {
            return this.$http.delete(`api/v1/kendo-organizational-configuration?organizationId=${orgId}&overviewType=${overviewType}`);
        }
    }

    app.service("KendoFilterService", KendoFilterService);
}
