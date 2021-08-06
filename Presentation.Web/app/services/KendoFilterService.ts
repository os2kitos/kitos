module Kitos.Services {
    "use strict";

    export class KendoFilterService {

        public static $inject: string[] = ["$http"];

        constructor(private $http: IHttpServiceWithCustomConfig) {
        }

        //TODO: JMO casing
        GetConfigurationFromOrg = (orgId: number, overviewType: Models.Generic.OverviewType) => {
            return this.$http
                .get<API.Models.IApiWrapper<Models.Generic.IKendoOrganizationalConfigurationDTO>>(
                    `api/v1/kendo-organizational-configuration?organizationId=${orgId}&overviewType=${overviewType}`);
        }
        //TODO: JMO casing
        GetConfigurationVersion = (orgId: number, overviewType: Models.Generic.OverviewType) => {
            return this.$http
                .get<API.Models.IApiWrapper<string>>(
                    `api/v1/kendo-organizational-configuration/version?organizationId=${orgId}&overviewType=${overviewType}`);
        }
        //TODO: JMO casing
        PostConfigurationFromOrg = (orgId: number, overviewType: Models.Generic.OverviewType, columns: Models.Generic.IKendoColumnConfigurationDTO[]) => {

            var payload = {
                OverviewType: overviewType,
                Columns: columns,
                OrganizationId: orgId
            }
            return this.$http.post(`api/v1/kendo-organizational-configuration`, payload);
        }
        //TODO: JMO casing
        DeleteConfigurationFromOrg = (orgId: number, overviewType: Models.Generic.OverviewType) => {
            return this.$http.delete(`api/v1/kendo-organizational-configuration?organizationId=${orgId}&overviewType=${overviewType}`);
        }
    }

    app.service("KendoFilterService", KendoFilterService);
}
