(function (ng, app) {

    var _user = null;

    app.factory('userService', ['$http', '$q', '$rootScope', '$modal', function ($http, $q, $rootScope, $modal) {
        
        //formats and saves the user
        function saveUser(user, orgAndDefaultUnit) {
            var currOrg = orgAndDefaultUnit.organization;
            var defaultOrgUnit = orgAndDefaultUnit.defaultOrgUnit;
            var defaultOrgUnitId = defaultOrgUnit == null ? null : defaultOrgUnit.id;

            var isLocalAdmin = _.some(user.adminRights, function(userRight) {
                return userRight.roleName == "LocalAdmin" && userRight.organizationId == currOrg.id;
            });

            //the current org unit is the default org unit for this organization if the user has selected one
            //otherwise it's the root of this organization
            var currentOrgUnitId = user.defaultOrganizationUnitId;
            var currentOrgUnitName = user.defaultOrganizationUnitName;
            var isUsingDefaultOrgUnit = true;
            
            if (response.defaultOrganizationUnitOrganizationId != currOrg.id) {
                currentOrgUnitId = currOrg.root.id;
                currentOrgUnitName = currOrg.root.name;

                isUsingDefaultOrgUnit = false;
            }


            _user = {
                isAuth: true,
                id: user.id,
                name: user.name,
                lastName: user.lastName,
                email: user.email,
                phoneNumber: user.phoneNumber,

                isGlobalAdmin: user.isGlobalAdmin,
                isLocalAdmin: isLocalAdmin,

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
                    method: 'PATCH',
                    url: 'api/user/' + _user.id + '?organizationId=' + _user.currentOrganizationId,
                    data: payload
                }).success(function (result) {
                    var newUser = result.response;
                    
                    saveUser(newUser, _user.currentOrganization);
                    loadUserDeferred = null;
                    deferred.resolve(_user);

                }).error(function () {
                    deferred.reject("Couldn't patch the user!");
                });

            }
            return deferred.promise;
        }

        function getUser() {

            var deferred = $q.defer();

            deferred.resolve(loadUser());

            return deferred.promise;
        }

        var loadUserDeferred = null;
        function loadUser(payload)
        {
            if (!loadUserDeferred) {
                loadUserDeferred = $q.defer();

                //login or re-auth?
                var httpDeferred = payload ? $http.post('api/authorize', payload) : $http.get('api/authorize');

                httpDeferred.success(function (result) {

                    var user = result.response.user;
                    var orgsAndDefaultUnits = result.response.organizations;

                    resolveOrganization2(orgsAndDefaultUnits).then(function (orgAndDefaultUnit) {
                        saveUser(user, orgAndDefaultUnit);
                        loadUserDeferred.resolve(_user);

                    }, function() {
                        loadUserDeferred.reject("No organization selected");
                        loadUserDeferred = null;
                    });


                }).error(function (result) {
                    
                    loadUserDeferred.reject(result);
                    loadUserDeferred = null;
                    clearSavedOrgId();
                });
            }

            return loadUserDeferred.promise;
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
            

            var deferred = $q.defer();

            $http.post('api/authorize?logout').success(function (result) {
                loadUserDeferred = null;
                _user = null;
                $rootScope.user = null;

                deferred.resolve();

                clearSavedOrgId();

            }).error(function(result) {
                deferred.reject("Could not log out");

            });
            
            return deferred.promise;
        }
        
        function auth(adminRoles) {

            return getUser().then(function (user) {
                
                if (adminRoles) {
                    var hasRequiredRole = _.some(adminRoles, function(role) {
                        //if the state role is global admin, and the user is global admin, it's cool
                        //same for local admin
                        return (role == "GlobalAdmin" && user.isGlobalAdmin) || (role == "LocalAdmin" && user.isLocalAdmin);
                    });

                    if (!hasRequiredRole) return $q.reject("User doesn't have the required permissions");
                }

                return true;

            });

        }
        
        function getSavedOrgId() {
            var orgId = localStorage.getItem("currentOrgId");
            return orgId != null && JSON.parse(orgId);
        }
            
        function setSavedOrgId(orgId) {
            localStorage.setItem("currentOrgId", JSON.stringify(orgId));
        }
        
        function clearSavedOrgId() {
            localStorage.setItem("currentOrgId", null);
        }

        //resolve which organization context, the user will be working in.
        //when a user logs in, the user is prompted with a select-organization modal.
        //the organization that is selected here, will be saved in local storage, for the next
        //time the user is visiting.
        function resolveOrganization2(orgsAndDefaultUnits) {

            var deferred = $q.defer();

            //first, if the user is only member of one organization, just use that
            if (orgsAndDefaultUnits.length == 1) {
                var firstOrgAndDefaultUnit = orgsAndDefaultUnits[0];
                setSavedOrgId(firstOrgAndDefaultUnit.organization.id);

                deferred.resolve(firstOrgAndDefaultUnit);
                return deferred.promise;
            }
            
            //else, try to get previous selected organization id from the local storage
            var storedOrgId = getSavedOrgId();

            if (storedOrgId) {

                //given the saved org id, find the organization in the list of organization and default org units
                var foundOrgAndDefaultUnit = _.find(orgsAndDefaultUnits, function(orgAndUnit) {
                    return orgAndUnit.organization.id == storedOrgId;
                });

                if (foundOrgAndDefaultUnit != null) {
                    deferred.resolve(foundOrgAndDefaultUnit);
                    return deferred.promise;
                }

                //if we get to this point, the stored org id was useless - i.e. it referred to an organization, that the user no longer is a member of.
                //so clear it
                clearSavedOrgId();
            }

            //if we get to this point, there is more than organization to choose from,
            //and we couldn't use the stored organization id.
            //last resort we have to prompt the user to select an organization

            var modal = $modal.open({
                backdrop: 'static',
                templateUrl: 'partials/home/choose-organization.html',
                controller: ['$scope', '$modalInstance', 'autofocus', function ($modalScope, $modalInstance, autofocus) {
                    autofocus();

                    $modalScope.organizations = _.map(orgsAndDefaultUnits, function(orgAndUnit) {
                        return orgAndUnit.organization;
                    });

                    $modalScope.ok = function () {

                        var selectedOrgAndUnit = _.find(orgsAndDefaultUnits, function(orgAndUnit) {
                            return orgAndUnit.organization.id == storedOrgId;
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

        //resolve which organization context, the user will be working in.
        //when a user logs in, the user is prompted with a select-organization modal.
        //the organization that is selected here, will be saved in local storage, for the next
        //time the user is visiting.
        /*
        function resolveOrganization() {

            var deferred = $q.defer();
            
            //first, try to get previous selected organization id from the local storage
            var storedOrgId = getSavedOrgId();

            if (storedOrgId) {
                
                //given the saved org id, fetch the org details and config from the server 
                $http.get("api/organization/" + storedOrgId).success(function(result) {
                    deferred.resolve(result.response);
                    
                }).error(function(result) {

                    //the saved org was probably bad
                    clearSavedOrgId();
                    
                    //prompt the user to select an org via modal
                    openModal();
                });

            } else {

                //no previous selected org in local storage. 
                
                //prompt the user to select an org via modal
                openModal();
            }
            
            //helper function for displaying the choose-organization modal
            function openModal() {
                //fetch the relevant organizations
                $http.get("api/user?organizations").success(function (result) {

                    var organizations = result.response.organizations;

                    //if there's only one, just select that
                    if (organizations.length == 1) {

                        var firstOrg = organizations[0];
                        setSavedOrgId(firstOrg.id);

                        deferred.resolve(firstOrg);

                    } else {

                        //otherwise, open a modal 
                        var modal = $modal.open({
                            backdrop: 'static',
                            templateUrl: 'partials/home/choose-organization.html',
                            controller: ['$scope', '$modalInstance', 'autofocus', function ($modalScope, $modalInstance, autofocus) {
                                autofocus();
                                $modalScope.orgChooser = {
                                    selectedId: result.response.defaultOrganizationId
                                };
                                
                                $modalScope.organizations = organizations;

                                $modalScope.ok = function () {

                                    var selectedOrg = _.findWhere(organizations, { id: $modalScope.orgChooser.selectedId });

                                    $modalInstance.close(selectedOrg);

                                };

                            }]
                        });

                        modal.result.then(function (selectedOrg) {

                            setSavedOrgId(selectedOrg.id);
                            deferred.resolve(selectedOrg);

                        }, function () {

                            deferred.reject("Modal dismissed");
                        });

                    }

                }).error(function () {
                    deferred.reject("Request for the users organizations failed!");
                });
            }

            return deferred.promise;
        }
        */


        return {
            getUser: getUser,
            login: login,
            logout: logout,
            auth: auth,
            patchUser: patchUser
        };

    }]);


})(angular, app);