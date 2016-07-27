(function (ng, app) {
    var _user = null;

    app.factory('userService', ['$http', '$window', '$q', '$rootScope', '$uibModal', function ($http, $window, $q, $rootScope, $modal) {
        // formats and saves the user
        function saveUser(user, orgAndDefaultUnit) {
            var currOrg = orgAndDefaultUnit.organization;
            var defaultOrgUnit = orgAndDefaultUnit.defaultOrgUnit;
            var defaultOrgUnitId = defaultOrgUnit == null ? null : defaultOrgUnit.id;

            var isLocalAdmin = _.some(user.organizationRights, function (userRight: { role; organizationId; }) {
                return userRight.role == Kitos.API.Models.OrganizationRole.LocalAdmin && userRight.organizationId == currOrg.id;
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

            _user = {
                isAuth: true,
                id: user.id,
                name: user.name,
                lastName: user.lastName,
                fullName: user.fullName,
                email: user.email,
                phoneNumber: user.phoneNumber,
                uuid: user.uuid,

                isGlobalAdmin: user.isGlobalAdmin,
                isLocalAdmin: isLocalAdmin,

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

            $rootScope.user = _user;

            return _user;
        }

        function patchUser(payload) {
            var deferred = $q.defer();

            if (_user == null) {
                deferred.reject("Not authenticated.");
            } else {
                $http({
                    method: "PATCH",
                    url: `api/user/${_user.id}?organizationId=${_user.currentOrganizationId}`,
                    data: payload
                }).success(result => {
                    var newUser = result.response;

                    // updating the user. the organization and default org unit is unchanged
                    saveUser(newUser, _user.orgAndDefaultUnit);
                    loadUserDeferred = null;
                    deferred.resolve(_user);
                }).error(() => {
                    deferred.reject("Couldn't patch the user!");
                });
            }

            return deferred.promise;
        }

        function getUser() {
            var deferred = $q.defer();
            deferred.resolve(loadUser(null));
            return deferred.promise;
        }

        var loadUserDeferred = null;

        function loadUser(payload) {
            if (!loadUserDeferred) {
                loadUserDeferred = $q.defer();

                // login or re-auth?
                var httpDeferred = payload ? $http.post("api/authorize", payload) : $http.get("api/authorize");

                httpDeferred.then(result => {
                    var user = result.data.response.user;
                    var orgsAndDefaultUnits = result.data.response.organizations;

                    resolveOrganization2(orgsAndDefaultUnits).then(orgAndDefaultUnit => {
                        saveUser(user, orgAndDefaultUnit);

                        setDefaultOrganizationInBackend(orgAndDefaultUnit.organization.id);

                        loadUserDeferred.resolve(_user);
                        loadUserDeferred = null;
                    }, () => {
                        loadUserDeferred.reject("No organization selected");
                        loadUserDeferred = null;
                    });
                }, result => {
                    loadUserDeferred.reject(result.data);
                    loadUserDeferred = null;
                    clearSavedOrgId();
                });
            }

            return loadUserDeferred.promise;
        }

        function setDefaultOrganizationInBackend(organizationId) {
            $http.post(`api/user?updateDefaultOrganization=true&organizationId=${organizationId}`);
        }

        function login(email, password, rememberMe) {
            var deferred = $q.defer();

            if (!email || !password) {
                deferred.reject("Email or password cannot be empty");
            } else {
                var data = {
                    "email": email,
                    "password": password,
                    "rememberMe": rememberMe
                };

                deferred.resolve(loadUser(data));
            }

            return deferred.promise;
        }

        function logout() {
            clearSavedOrgId();
            loadUserDeferred = null;
            _user = null;
            $rootScope.user = null;

            return $http.post("api/authorize?logout");
        }

        function auth(adminRoles) {
            return getUser().then(user => {
                if (adminRoles) {
                    var hasRequiredRole = _.some(adminRoles, role => ((role == "GlobalAdmin" && user.isGlobalAdmin) || (role == "LocalAdmin" && user.isLocalAdmin)));

                    if (!hasRequiredRole) return $q.reject("User doesn't have the required permissions");
                }

                return true;
            });
        }

        function getSavedOrgId() {
            var orgId = localStorage.getItem("currentOrgId");
            return orgId != null && $window.JSON.parse(orgId);
        }

        function setSavedOrgId(orgId) {
            localStorage.setItem("currentOrgId", $window.JSON.stringify(orgId));
        }

        function clearSavedOrgId() {
            localStorage.setItem("currentOrgId", null);
        }

        // resolve which organization context, the user will be working in.
        // when a user logs in, the user is prompted with a select-organization modal.
        // the organization that is selected here, will be saved in local storage, for the next
        // time the user is visiting.
        function resolveOrganization2(orgsAndDefaultUnits) {
            var deferred = $q.defer();

            // first, if the user is only member of one organization, just use that
            if (orgsAndDefaultUnits.$values.length == 1) {
                var firstOrgAndDefaultUnit = orgsAndDefaultUnits.$values[0];
                setSavedOrgId(firstOrgAndDefaultUnit.organization.id);
                deferred.resolve(firstOrgAndDefaultUnit);
                return deferred.promise;
            }

            // else, try to get previous selected organization id from the local storage
            var storedOrgId = getSavedOrgId();

            if (storedOrgId) {

                // given the saved org id, find the organization in the list of organization and default org units
                var foundOrgAndDefaultUnit = _.find(orgsAndDefaultUnits.$values, function (orgAndUnit: { organization }) {
                    return orgAndUnit.organization.id == storedOrgId;
                });

                if (foundOrgAndDefaultUnit != null) {
                    deferred.resolve(foundOrgAndDefaultUnit);
                    return deferred.promise;
                }

                // if we get to this point, the stored org id was useless - i.e. it referred to an organization, that the user no longer is a member of.
                // so clear it
                clearSavedOrgId();
            }

            // if we get to this point, there is more than organization to choose from,
            // and we couldn't use the stored organization id.
            // last resort we have to prompt the user to select an organization

            var modal = $modal.open({
                backdrop: "static",
                templateUrl: "app/components/home/choose-organization.html",
                controller: ["$scope", "$uibModalInstance", "autofocus", function ($modalScope, $modalInstance, autofocus) {
                    autofocus();

                    $modalScope.organizations = _.map(orgsAndDefaultUnits.$values, function (orgAndUnit: { organization }) {
                        return orgAndUnit.organization;
                    });

                    $modalScope.orgChooser = {
                        selectedId: null
                    };

                    $modalScope.ok = function () {
                        var selectedOrgAndUnit = _.find(orgsAndDefaultUnits.$values, function (orgAndUnit: { organization }) {
                            return orgAndUnit.organization.id == $modalScope.orgChooser.selectedId;
                        });
                        $modalInstance.close(selectedOrgAndUnit);
                    };
                }]
            });

            modal.result.then(function (selectedOrgAndUnit) {
                setSavedOrgId(selectedOrgAndUnit.organization.id);
                deferred.resolve(selectedOrgAndUnit);
            }, function () {
                deferred.reject("Modal dismissed");
            });

            return deferred.promise;
        }

        // updates the users default org unit in the current organization
        function updateDefaultOrgUnit(newDefaultOrgUnitId) {
            var deferred = $q.defer();

            var payload = {
                orgId: _user.currentOrganizationId,
                orgUnitId: newDefaultOrgUnitId
            };

            $http.post("api/user?updateDefaultOrgUnit", payload).success(function (result) {
                // now we gotta update the saved user in the userService.
                // the simplest is just to re-auth the user.
                getUser().then(function (user) {
                    deferred.resolve(user);
                }, function () {
                    deferred.reject("Couldn't update default org unit!");
                });
            }).error(function () {
                deferred.reject("Couldn't update default org unit!");
            });

            return deferred.promise;
        }

        return {
            getUser: getUser,
            login: login,
            logout: logout,
            auth: auth,
            patchUser: patchUser,
            updateDefaultOrgUnit: updateDefaultOrgUnit
        };
    }]);
})(angular, app);
