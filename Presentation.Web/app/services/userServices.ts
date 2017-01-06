module Kitos.Services {
    export interface IUser {
        isAuth: boolean;
        id: number;
        name: string;
        lastName: string;
        fullName: string;
        email: string;
        phoneNumber: number;
        uuid: string;
        defaultUserStartPreference: string;
        isGlobalAdmin: boolean;
        isLocalAdmin: boolean;
        isOrgAdmin: boolean;
        isProjectAdmin: boolean;
        isSystemAdmin: boolean;
        isContractAdmin: boolean;
        isReportAdmin: boolean;

        orgAndDefaultUnit: any;

        currentOrganizationUnitId: number;
        currentOrganizationUnitName: string;
        isUsingDefaultOrgUnit: boolean;

        defaultOrganizationUnitId: number;

        currentOrganization: string;
        currentOrganizationId: number;
        currentOrganizationName: string;
        currentConfig: any;
    }

    export interface IUserService {
        reAuthorize(): ng.IPromise<IUser>;
        getUser(): ng.IPromise<IUser>;
        login(email: string, password: string, rememberMe: boolean): ng.IPromise<any>;
        logout(): ng.IHttpPromise<any>;
        auth(adminRoles): ng.IPromise<any>;
        patchUser(payload): ng.IPromise<any>;
        updateDefaultOrgUnit(newDefaultOrgUnitId: number): ng.IPromise<any>;
    }

    export class UserService implements IUserService {
        _user: Kitos.Services.IUser = null;
        _loadUserDeferred = null;

        static $inject = ['$http', '$window', '$q', '$rootScope', '$uibModal', '_'];
        constructor(private $http: ng.IHttpService, private $window: ng.IWindowService, private $q: ng.IQService, private $rootScope, private $uibModal: ng.ui.bootstrap.IModalService, private _: _.LoDashStatic) {
        }

        saveUser = (user, orgAndDefaultUnit) => {
            var currOrg = orgAndDefaultUnit.organization;
            var defaultOrgUnit = orgAndDefaultUnit.defaultOrgUnit;
            var defaultOrgUnitId = defaultOrgUnit == null ? null : defaultOrgUnit.id;

            var isLocalAdmin = this._.some(user.organizationRights, function (userRight: { role; organizationId; }) {
                return userRight.role == Kitos.API.Models.OrganizationRole.LocalAdmin && userRight.organizationId == currOrg.id;
            });

            var isOrgAdmin = this._.some(user.organizationRights, function (userRight: { role; organizationId; }) {
                return userRight.role == Kitos.API.Models.OrganizationRole.OrganizationModuleAdmin && userRight.organizationId == currOrg.id;
            });

            var isProjectAdmin = this._.some(user.organizationRights, function (userRight: { role; organizationId; }) {
                return userRight.role == Kitos.API.Models.OrganizationRole.ProjectModuleAdmin && userRight.organizationId == currOrg.id;
            });

            var isSystemAdmin = this._.some(user.organizationRights, function (userRight: { role; organizationId; }) {
                return userRight.role == Kitos.API.Models.OrganizationRole.SystemModuleAdmin && userRight.organizationId == currOrg.id;
            });

            var isContractAdmin = this._.some(user.organizationRights, function (userRight: { role; organizationId; }) {
                return userRight.role == Kitos.API.Models.OrganizationRole.ContractModuleAdmin && userRight.organizationId == currOrg.id;
            });

            var isReportAdmin = this._.some(user.organizationRights, function (userRight: { role; organizationId; }) {
                return userRight.role == Kitos.API.Models.OrganizationRole.ReportModuleAdmin && userRight.organizationId == currOrg.id;
            });

            // the current org unit is the default org unit for this organization if the user has selected one
            // otherwise it's the root of this organization
            var currentOrgUnitId;
            var currentOrgUnitName;
            var isUsingDefaultOrgUnit;

            if (defaultOrgUnitId == null) {
                currentOrgUnitId = currOrg.root.id;
                currentOrgUnitName = currOrg.root.name;

                isUsingDefaultOrgUnit = false;
            } else {
                currentOrgUnitId = defaultOrgUnitId;
                currentOrgUnitName = defaultOrgUnit.name;

                isUsingDefaultOrgUnit = true;
            }

            this._user = {
                isAuth: true,
                id: user.id,
                name: user.name,
                lastName: user.lastName,
                fullName: user.fullName,
                email: user.email,
                phoneNumber: user.phoneNumber,
                uuid: user.uuid,
                defaultUserStartPreference: user.defaultUserStartPreference || "index",

                isGlobalAdmin: user.isGlobalAdmin,
                isLocalAdmin: isLocalAdmin,
                isOrgAdmin: isOrgAdmin,
                isProjectAdmin: isProjectAdmin,
                isSystemAdmin: isSystemAdmin,
                isContractAdmin: isContractAdmin,
                isReportAdmin: isReportAdmin,

                orgAndDefaultUnit: orgAndDefaultUnit,

                currentOrganizationUnitId: currentOrgUnitId,
                currentOrganizationUnitName: currentOrgUnitName,
                isUsingDefaultOrgUnit: isUsingDefaultOrgUnit,

                defaultOrganizationUnitId: defaultOrgUnitId,

                currentOrganization: currOrg,
                currentOrganizationId: currOrg.id,
                currentOrganizationName: currOrg.name,
                currentConfig: currOrg.config
            };

            this.$rootScope.user = this._user;

            console.log("saveUser");
            console.log("--------------");

            return this._user;
        }

        patchUser = (payload) => {
            var deferred = this.$q.defer();

            if (this._user == null) {
                deferred.reject("Not authenticated.");
            } else {
                this.$http<Kitos.API.Models.IApiWrapper<any>>({
                    method: "PATCH",
                    url: `api/user/${this._user.id}?organizationId=${this._user.currentOrganizationId}`,
                    data: payload
                }).success(result => {
                    var newUser = result.response;

                    // updating the user. the organization and default org unit is unchanged
                    this.saveUser(newUser, this._user.orgAndDefaultUnit);
                    this._loadUserDeferred = null;
                    deferred.resolve(this._user);
                }).error(() => {
                    deferred.reject("Couldn't patch the user!");
                });
            }
            return deferred.promise;
        }

        // the object returned contains the user, user rights and user organizations
        getUser = () => {
            return this.reAuthorize();
        }

        reAuthorize = () => {
            console.log("reAuthorize --> loadUser(null)");
            console.log("--------------");

            var deferred = this.$q.defer();
            // loadUser(null) triggers a re-auth
            deferred.resolve(this.loadUser(null));
            return deferred.promise;
        }

        getOrganizationsIfAuthorized = () => {
            console.log("Authorize/GET (Re-Auth)");
            console.log("--------------");

            return this.$http.get<Kitos.API.Models.IApiWrapper<any>>("api/authorize");
        }

        authorizeUser = (userLoginInfo) => {
            console.log("Authorize/POST (Logging in)");
            console.log("--------------");

            return this.$http.post<Kitos.API.Models.IApiWrapper<any>>("api/authorize", userLoginInfo);
        }

        saveUserInfo = (user, orgAndDefaultUnit) => {
            this.saveUser(user, orgAndDefaultUnit);
            this.setDefaultOrganizationInBackend(orgAndDefaultUnit.organization.id);
        }

        loadUser = (userLoginInfo) => {
            console.log("loadUser payload: ", userLoginInfo);
            console.log("_loadUserDeferred before if: ", this._loadUserDeferred);
            console.log("--------------");

            if (!this._loadUserDeferred) {
                this._loadUserDeferred = this.$q.defer();
                console.log("_loadUserDeferred after if: ", this._loadUserDeferred);
                console.log("--------------");

                // login or re-auth? If userLoginInfo is null then re-auth otherwise login
                var httpDeferred = userLoginInfo ? this.authorizeUser(userLoginInfo) : this.getOrganizationsIfAuthorized();

                httpDeferred.then(result => {
                    var user = result.data.response.user;
                    var orgsAndDefaultUnits = result.data.response.organizations;

                    //this.resolveOrganization2(orgsAndDefaultUnits).then((orgAndDefaultUnit: any) => {
                    this.determineOrganizationChoice(orgsAndDefaultUnits).then((orgAndDefaultUnit: any) => {

                        this.saveUserInfo(user, orgAndDefaultUnit);

                        this._loadUserDeferred.resolve(this._user);
                        this._loadUserDeferred = null;

                    }, () => {

                        this._loadUserDeferred.reject("No organization selected");
                        this._loadUserDeferred = null;

                    });

                }, result => {

                    this._loadUserDeferred.reject(result.data);
                    this._loadUserDeferred = null;
                    this.clearSavedOrgId();

                });
            }

            return this._loadUserDeferred.promise;
        }

        //TODO soon obsolete
        //loadUser = (payload) => {
        //    if (!this._loadUserDeferred) {
        //        this._loadUserDeferred = this.$q.defer();

        //        // login or re-auth?
        //        var httpDeferred = payload ? this.$http.post<Kitos.API.Models.IApiWrapper<any>>("api/authorize", payload) : this.$http.get<Kitos.API.Models.IApiWrapper<any>>("api/authorize");

        //        httpDeferred.then(result => {
        //            var user = result.data.response.user;
        //            var orgsAndDefaultUnits = result.data.response.organizations;

        //            this.resolveOrganization2(orgsAndDefaultUnits).then((orgAndDefaultUnit: any) => {
        //                this.saveUser(user, orgAndDefaultUnit);
        //                this.setDefaultOrganizationInBackend(orgAndDefaultUnit.organization.id);
        //                this._loadUserDeferred.resolve(this._user);
        //                this._loadUserDeferred = null;
        //            }, () => {
        //                this._loadUserDeferred.reject("No organization selected");
        //                this._loadUserDeferred = null;
        //            });
        //        }, result => {
        //            this._loadUserDeferred.reject(result.data);
        //            this._loadUserDeferred = null;
        //            this.clearSavedOrgId();
        //        });
        //    }

        //    return this._loadUserDeferred.promise;
        //}

        setDefaultOrganizationInBackend = (organizationId) => {
            this.$http.post(`api/user?updateDefaultOrganization=true&organizationId=${organizationId}`, undefined);
        }

        login = (email: string, password: string, rememberMe: boolean) => {
            console.log("login called");
            console.log("--------------");

            var deferred = this.$q.defer();

            if (!email || !password) {
                deferred.reject("Email or password cannot be empty");
            } else {
                var data = {
                    "email": email,
                    "password": password,
                    "rememberMe": rememberMe
                };
                deferred.resolve(this.loadUser(data));
            }
            return deferred.promise;
        }

        logout = () => {
            this.clearSavedOrgId();
            this._loadUserDeferred = null;
            this._user = null;
            this.$rootScope.user = null;
            return this.$http.post("api/authorize?logout", undefined);
        }

        auth = (adminRoles) => {
            console.log("auth called");
            console.log("--------------");

            return this.getUser().then((user: any) => {
                if (adminRoles) {
                    var hasRequiredRole = this._.some(adminRoles, role => ((role === "GlobalAdmin" && user.isGlobalAdmin) || (role === Models.OrganizationRole.LocalAdmin && user.isLocalAdmin)));

                    if (!hasRequiredRole) return this.$q.reject("User doesn't have the required permissions");
                }

                return true;
            });
        }

        getSavedOrgId = () => {
            var orgId = localStorage.getItem("currentOrgId");
            return orgId != null && JSON.parse(orgId);
        }

        setSavedOrgId = (orgId) => {
            localStorage.setItem("currentOrgId", JSON.stringify(orgId));
        }

        clearSavedOrgId = () => {
            localStorage.setItem("currentOrgId", null);
        }

        // determines
        determineOrganizationChoice = (orgsAndDefaultUnits) => {
            console.log("--------------");
            console.log("determineOrganizationChoice called");

            var deferred = this.$q.defer();
            const onlyOneOrganization = orgsAndDefaultUnits.$values.length == 1;

            console.log("num of orgs: " + orgsAndDefaultUnits.$values.length);
            console.log("--------------");

            // the users has previously been logged in so the last chosen organization will be selected
            if (this.currentlyLoggedIn()) {

                var storedOrgId = this.getSavedOrgId();

                if (storedOrgId) {
                    deferred.resolve(this.lastChosenOrganization(orgsAndDefaultUnits, storedOrgId));
                }

                if (!onlyOneOrganization) {
                    this.$rootScope.userHasOrgChoices = true;
                }

            } else if (onlyOneOrganization) {

                deferred.resolve(this.chooseOrganization(orgsAndDefaultUnits));

            } else { // the user can choose from multiple organizations

                this.$rootScope.userHasOrgChoices = true;
                deferred.resolve(this.selectAmongOrganizations(orgsAndDefaultUnits));

            }

            return deferred.promise;
        }

        currentlyLoggedIn() {
            console.log("getSavedOrgId: " + this.getSavedOrgId());
            console.log("--------------");

            return this.getSavedOrgId();
        }

        chooseOrganization = (orgsAndDefaultUnits) => {
            console.log("chooseOrganization called");
            console.log("--------------");

            // disable the the option to change the current organization
            this.$rootScope.userHasOrgChoices = false;

            var firstOrgAndDefaultUnit = orgsAndDefaultUnits.$values[0];
            this.setSavedOrgId(firstOrgAndDefaultUnit.organization.id);

            return firstOrgAndDefaultUnit;
        }

        lastChosenOrganization = (orgsAndDefaultUnits, storedOrgId) => {
            console.log("lastChosenOrganization called");
            console.log("--------------");

            // given the saved org id, find the organization in the list of organization and default org units
            var foundOrgAndDefaultUnit = this._.find(orgsAndDefaultUnits.$values, function (orgAndUnit: { organization }) {
                return orgAndUnit.organization.id == storedOrgId;
            });

            if (foundOrgAndDefaultUnit != null) {
                return foundOrgAndDefaultUnit;
            }

            // if we get to this point, the stored org id was useless - i.e. it referred to an organization, that the user no longer is a member of.
            // so clear it
            this.clearSavedOrgId();
        }

        selectAmongOrganizations = (orgsAndDefaultUnits) => {
            console.log("selectAmongOrganizations called");
            console.log("--------------");

            var deferred = this.$q.defer();
            var modal = this.showOrganizationsModal(orgsAndDefaultUnits);

            modal.result.then((selectedOrgAndUnit) => {
                this.setSavedOrgId(selectedOrgAndUnit.organization.id);
                console.log("setSavedOrgId was changed to: " + selectedOrgAndUnit.organization.id);
                console.log("--------------");

                deferred.resolve(selectedOrgAndUnit);
            }, () => {
                deferred.reject("Modal dismissed");
            });

            return deferred.promise;
        }

        showOrganizationsModal = (orgsAndDefaultUnits) => {
            console.log("showOrganizationsModal called");
            console.log("--------------");

            var modal = this.$uibModal.open({
                backdrop: "static",
                templateUrl: "app/components/home/choose-organization.html",
                controller: ["$scope", "$uibModalInstance", "autofocus", ($modalScope, $modalInstance, autofocus) => {
                    autofocus();

                    $modalScope.organizations = this._.map(orgsAndDefaultUnits.$values, function (orgAndUnit: { organization }) {
                        return orgAndUnit.organization;
                    });

                    $modalScope.orgChooser = {
                        selectedId: null
                    };

                    $modalScope.ok = () => {
                        console.log("org chosen");

                        var selectedOrgAndUnit = this._.find(orgsAndDefaultUnits.$values, function (orgAndUnit: { organization }) {
                            return orgAndUnit.organization.id == $modalScope.orgChooser.selectedId;
                        });
                        $modalInstance.close(selectedOrgAndUnit);
                    };
                }]
            });

            return modal;
        }

        // resolve which organization context, the user will be working in.
        // when a user logs in, the user is prompted with a select-organization modal.
        // the organization that is selected here, will be saved in local storage, for the next
        // time the user is visiting.
        // TODO soon obsolete
        resolveOrganization2 = (orgsAndDefaultUnits) => {
            var deferred = this.$q.defer();

            // first, if the user is only member of one organization, just use that
            if (orgsAndDefaultUnits.$values.length == 1) {
                // toggles the option to change current organization if the user has multiple to choose from
                this.$rootScope.userHasOrgChoices = false;
                var firstOrgAndDefaultUnit = orgsAndDefaultUnits.$values[0];
                this.setSavedOrgId(firstOrgAndDefaultUnit.organization.id);
                deferred.resolve(firstOrgAndDefaultUnit);
                return deferred.promise;
            }

            // toggles the option to change current organization if the user has multiple to choose from
            this.$rootScope.userHasOrgChoices = true;

            // else, try to get previous selected organization id from the local storage
            var storedOrgId = this.getSavedOrgId();

            if (storedOrgId) {

                // given the saved org id, find the organization in the list of organization and default org units
                var foundOrgAndDefaultUnit = this._.find(orgsAndDefaultUnits.$values, function (orgAndUnit: { organization }) {
                    return orgAndUnit.organization.id == storedOrgId;
                });

                if (foundOrgAndDefaultUnit != null) {
                    deferred.resolve(foundOrgAndDefaultUnit);
                    return deferred.promise;
                }

                // if we get to this point, the stored org id was useless - i.e. it referred to an organization, that the user no longer is a member of.
                // so clear it
                this.clearSavedOrgId();
            }

            // if we get to this point, there is more than organization to choose from,
            // and we couldn't use the stored organization id.
            // last resort we have to prompt the user to select an organization

            var modal = this.$uibModal.open({
                backdrop: "static",
                templateUrl: "app/components/home/choose-organization.html",
                controller: ["$scope", "$uibModalInstance", "autofocus", ($modalScope, $modalInstance, autofocus) => {
                    autofocus();

                    $modalScope.organizations = this._.map(orgsAndDefaultUnits.$values, function (orgAndUnit: { organization }) {
                        return orgAndUnit.organization;
                    });

                    $modalScope.orgChooser = {
                        selectedId: null
                    };

                    $modalScope.ok = () => {
                        var selectedOrgAndUnit = this._.find(orgsAndDefaultUnits.$values, function (orgAndUnit: { organization }) {
                            return orgAndUnit.organization.id == $modalScope.orgChooser.selectedId;
                        });
                        $modalInstance.close(selectedOrgAndUnit);
                    };
                }]
            });

            modal.result.then((selectedOrgAndUnit) => {
                this.setSavedOrgId(selectedOrgAndUnit.organization.id);
                deferred.resolve(selectedOrgAndUnit);
            }, function () {
                deferred.reject("Modal dismissed");
            });

            return deferred.promise;
        }

        // change organizational context
        changeOrganization = () => {

            const deferred = this.$q.defer();
            let organizatinalContext = null;
            let organizations = null;
            let user = null;

            this.getOrganizationsIfAuthorized()
                .then(result => {

                    organizatinalContext = result.data.response;
                    organizations = organizatinalContext.organizations;

                    this.selectAmongOrganizations(organizations)
                        .then((organization) => {
                            user = organizatinalContext.user;
                            this.saveUserInfo(user, organization);
                        });

                    deferred.resolve(organizatinalContext);

                },
                () => {

                    deferred.reject("fejlfejlfejl");

                });

            return deferred.promise;

        }
        // updates the users default org unit in the current organization
        updateDefaultOrgUnit = (newDefaultOrgUnitId) => {
            //var deferred = this.$q.defer();

            var payload = {
                orgId: this._user.currentOrganizationId,
                orgUnitId: newDefaultOrgUnitId
            };

            return this.$http.post("api/user?updateDefaultOrgUnit", payload);
            //    .success(function (result) {
            //    // now we gotta update the saved user in the userService.
            //    // the simplest is just to re-auth the user.
            //    this.getUser().then(function (user) {
            //        deferred.resolve(user);
            //    }, function () {
            //        deferred.reject("Couldn't update default org unit!");
            //    });
            //}).error(function () {
            //    deferred.reject("Couldn't update default org unit!");
            //});

            //return deferred.promise;
        }
    }
    app.service("userService", UserService);
}

