module Kitos.Organization.GDPR {
    "use strict";

    class OrganizationGDPRController {

        public static $inject: string[] = ["$http", "$timeout", "_", "$", "$state", "$scope", "notify", "user", "hasWriteAccess",
            "organization", "dataResponsible", "dataProtectionAdvisor", "contactPerson", "emailExists"];
        public updateOrgUrl: string;
        public updatedataProtectionAdvisorUrl: string;
        public updatedataResponsibleUrl: string;
        public updateContactPersonUrl: string;

        public _$scope: any;
        public _contactPerson: any;
        public _user: any;

        constructor(
            private $http: ng.IHttpService,
            private $timeout: ng.ITimeoutService,
            private _: ILoDashWithMixins,
            private $: JQueryStatic,
            private $state: ng.ui.IStateService,
            private $scope,
            private notify,
            private user,
            private hasWriteAccess,
            private organization,
            private dataResponsible,
            private dataProtectionAdvisor,
            private contactPerson,
            private emailExists: boolean) {

            this.hasWriteAccess = hasWriteAccess;
            this.organization = organization;
            this._$scope = $scope;
            //init update urls
            this.updateOrgUrl = 'api/Organization/' + this.organization.id;
            this.updatedataProtectionAdvisorUrl = 'api/dataProtectionAdvisor/' + this.dataProtectionAdvisor.id;
            this.updatedataResponsibleUrl = 'api/dataResponsible/' + this.dataResponsible.id;
            this.updateContactPersonUrl = 'api/contactPerson/' + this.contactPerson.id;

            this._$scope._emailExists = emailExists;

            this._contactPerson = contactPerson;
            this._user = user;

            if (this.hasWriteAccess) {
                this._$scope.$watch("_emailExists", ((newValue, oldValue) => {
                    if (newValue) {
                        this.$http.get<any>('odata/GetUserByEmail(email=\'' + this.contactPerson.email + '\')')
                            .then((result) => {
                                this._contactPerson.name = result.data.Name;
                                this._contactPerson.lastName = result.data.LastName;
                                this._contactPerson.phoneNumber = result.data.PhoneNumber;
                                //patch
                                this.$http.patch<any>('api/contactPerson/' + this._contactPerson.id + "?organizationId= " + this._user.currentOrganizationId, this._contactPerson)
                                    .catch((err) => {
                                        console.log(err);
                                    });
                            });
                    }
                }));
            }
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider) => {
            $stateProvider.state("organization.gdpr", {
                url: "/basicInfo",
                templateUrl: "app/components/org/basicInformation/org-GDPR.view.html",
                controller: OrganizationGDPRController,
                controllerAs: "ctrl",
                resolve: {
                    user: [
                        "userService", userService => userService.getUser()
                    ],
                    userAccessRights: ["$http", "user", ($http, user) => $http.get("api/Organization?id=" + user.currentOrganizationId + "&getEntityAccessRights=true")
                        .then(result => result.data.response)
                    ],
                    hasWriteAccess: ["userAccessRights", userAccessRights => userAccessRights.canEdit
                    ],
                    organization: ['$http', 'user', function ($http, user) {
                        return $http.get('api/Organization/' + user.currentOrganizationId)
                            .then(function (result) {
                                return result.data.response;
                            });
                }],
                    dataResponsible: ['$http', 'organization', function ($http, organization) {
                        return $http.get('api/dataResponsible/' + organization.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                    }],
                    dataProtectionAdvisor: ['$http', 'organization', function ($http, organization) {
                        //get by org id
                        return $http.get('api/dataProtectionAdvisor/' + organization.id)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }],
                    contactPerson: ['$http', 'organization', function ($http, organization) {
                        //get by org id
                        return $http.get('api/contactPerson/' + organization.id)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }],
                    emailExists: ['$http', 'contactPerson', function ($http, contactPerson) {
                        //get by org id
                        if (contactPerson != null) {
                            return $http.get('/odata/Users/Users.IsEmailAvailable(email=\'' + contactPerson.email + '\')')
                                .then(function (response) {
                                    if (response.data.value) {return false;} else {return true;};
                                });
                       }
                      return false;
                    }]
                }
            });
        }
        ]);
}
