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
        isReadOnly: boolean;
        hasApi: boolean;

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
        reAuthorize(): any;
        getUser(): any;
        login(email: string, password: string, rememberMe: boolean): ng.IPromise<any>;
        logout(): ng.IHttpPromise<any>;
        auth(adminRoles): ng.IPromise<any>;
        patchUser(payload): ng.IPromise<any>;
        updateDefaultOrgUnit(newDefaultOrgUnitId: number): ng.IPromise<any>;
    }

    export class UserService implements IUserService {
        _user: Kitos.Services.IUser = null;
        _loadUserDeferred = null;

        static $inject = ["$http", "$window", "$q", "$rootScope", "$uibModal", "_"];
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
            var isReadOnly = this._.some(user.organizationRights, function (userRight: { role; organizationId; }) {
                return userRight.role == Kitos.API.Models.OrganizationRole.ReadOnly && userRight.organizationId == currOrg.id;
            });
            var hasApi = this._.some(user.organizationsRights, function(userRight: { role; organizationId; }) {
                return userRight.role == Kitos.API.Models.OrganizationRole.ApiAccess &&
                    userRight.organizationId == currOrg.id;
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
                isReadOnly: isReadOnly,
                hasApi: hasApi,

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
            var deferred = this.$q.defer();
            // loadUser(null) triggers a re-auth
            deferred.resolve(this.loadUser(null));
            return deferred.promise;
        }

        getOrganizations = () => {
            //returns the organizations for the user whos credentials have been authorized
            return this.$http.get<Kitos.API.Models.IApiWrapper<any>>("api/authorize/GetOrganizations");
        }

        getOrganizationWithDefault = (orgId) => {
            //returns the organizational context for the specific org
            return this.$http.get(`api/authorize/GetOrganization(${orgId})`).then((result: any) => { return result.data.response; });
        }

        getCurrentUserIfAuthorized = () => {
            //returns the organizational context for the user whos credentials have been authorized
            //triggers a re-auth like reAuthorize
            return this.$http.get<Kitos.API.Models.IApiWrapper<any>>("api/authorize");
        }

        authorizeUser = (userLoginInfo) => {
            //returns the organizational context for the user whos credentials have been authorized
            return this.$http.post<Kitos.API.Models.IApiWrapper<any>>("api/authorize", userLoginInfo);
        }

        saveUserInfo = (user, orgAndDefaultUnit) => {
            this.saveUser(user, orgAndDefaultUnit);
            this.setDefaultOrganizationInBackend(orgAndDefaultUnit.organization.id);
        }

        auth = (adminRoles) => {

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

        setDefaultOrganizationInBackend = (organizationId) => {
            this.$http.post(`api/user?updateDefaultOrganization=true&organizationId=${organizationId}`, undefined);
        }

        login = (email: string, password: string, rememberMe: boolean) => {

            var deferred = this.$q.defer();

            if (!email || !password) {
                deferred.reject("Email or password cannot be empty");
            } else {
                var data = {
                    "email": email,
                    "password": password,
                    "rememberMe": rememberMe
                };

                this.loadUser(data).then((resp) => {
                    deferred.resolve(resp)
                }, (resp) => {
                    deferred.reject(resp)
                });
            }

            return deferred.promise;
        }

        loginSSO = (token) => {
            var deferred = this.$q.defer();
            var data = {
                "token": token
            }
            deferred.resolve(this.loadUser(data));
            return deferred.promise;
        }

        logout = () => {

            this.clearSavedOrgId();
            this._loadUserDeferred = null;
            this._user = null;
            this.$rootScope.user = null;

            return this.$http.post("api/authorize?logout", undefined);
        }

        loadUser = (userLoginInfo) => {

            if (!this._loadUserDeferred) {

                this._loadUserDeferred = this.$q.defer();
                // login or re-auth? If userLoginInfo is null then re-auth otherwise login
                var httpDeferred = userLoginInfo ? this.authorizeUser(userLoginInfo) : this.getCurrentUserIfAuthorized();

                httpDeferred.then(result => {

                    var user = result.data.response;
                    this.$rootScope.userHasOrgChoices = user.isGlobalAdmin || this._.uniqBy(user.organizationRights, 'organizationId').length > 1;

                    this.determineLoginProcedure().then((orgAndDefaultUnit: any) => {
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

        determineLoginProcedure = () => {

            var deferred = this.$q.defer();

            // the users has previously been logged in so the last chosen organization will be selected
            if (this.currentlyLoggedIn()) {

                var storedOrgId = this.getSavedOrgId();

                if (storedOrgId) {
                    deferred.resolve(this.getOrganizationWithDefault(storedOrgId));
                }

            } else {
                deferred.resolve(this.chooseOrganization());
            }

            return deferred.promise;
        }

        currentlyLoggedIn() {
            return this.getSavedOrgId();
        }

        selectAvailableOrganization = (orgsAndDefaultUnits) => {

            var firstOrgAndDefaultUnit = orgsAndDefaultUnits.$values[0];
            this.setSavedOrgId(firstOrgAndDefaultUnit.organization.id);

            return firstOrgAndDefaultUnit;
        }

        lastChosenOrganization = (orgsAndDefaultUnits, storedOrgId) => {

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

        chooseOrganization = () => {

            var deferred = this.$q.defer();
            this.getOrganizations().then((data) => {
                var orgs = data.data.response;

                if (!this.$rootScope.userHasOrgChoices) {
                    var result = this.getOrganizationWithDefault(orgs[0].id);

                    deferred.resolve(result);
                    return deferred.promise;
                }

                var modal = this.showOrganizationsModal(orgs);

                modal.result.then((data) => {

                    if (data.userCancelled) {
                        deferred.resolve(true);
                    }
                    this.setSavedOrgId(data.id);

                    var result = this.getOrganizationWithDefault(data.id);

                    deferred.resolve(result);

                }, () => {

                    deferred.reject("Modal dismissed");

                });
            });
            return deferred.promise;
        }

        showOrganizationsModal = (orgsAndDefaultUnits) => {

            var modal = this.$uibModal.open({
                backdrop: "static",
                templateUrl: "app/components/home/choose-organization.html",
                controller: ["$scope", "$uibModalInstance", "autofocus", ($modalScope, $modalInstance, autofocus) => {
                    autofocus();

                    $modalScope.organizations = orgsAndDefaultUnits;
                    $modalScope.orgChooser = {
                        selectedId: null
                    };

                    $modalScope.ok = () => {
                        var selectedOrgAndUnit = this._.find($modalScope.organizations, function (org: any) { return org.id === $modalScope.orgChooser.selectedId; });

                        $modalInstance.close(selectedOrgAndUnit);
                    };

                }]
            });

            return modal;
        }

        // change organizational context
        changeOrganization = () => {

            //save the current organization id if the user chooses to cancel
            let currentOrgId = this.getSavedOrgId();

            //the saved org id needs to be cleared in order for the user to have the option to choose another organization
            //if the org id is not cleared then the last organization the user choose will be reselected instead
            this.clearSavedOrgId();

            let user = null;

            const deferred = this.$q.defer();

            this.getCurrentUserIfAuthorized().then(result => {

                user = result.data.response;
                this.chooseOrganization().then((orgAndDefaultUnit: any) => {

                    this.saveUserInfo(user, orgAndDefaultUnit);

                    deferred.resolve(this._user);

                }, error => {

                    this.setSavedOrgId(currentOrgId);

                    deferred.reject("Could not select an organization.");

                });

            }, error => {

                deferred.reject("Could not retrieve organizational context.");

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

