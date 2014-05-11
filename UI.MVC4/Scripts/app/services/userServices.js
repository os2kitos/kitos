(function (ng, app) {

    var _user = null;

    app.factory('userService', ['$http', '$q', '$rootScope', '$modal', function ($http, $q, $rootScope, $modal) {
        
        //formats and saves the user
        function saveUser(response, currOrg) {
            var isLocalAdmin = _.some(response.adminRights, function(userRight) {
                return userRight.roleName == "LocalAdmin";
            });

            _user = {
                isAuth: true,
                id: response.id,
                name: response.name,
                email: response.email,
                isGlobalAdmin: response.isGlobalAdmin,
                isLocalAdmin: isLocalAdmin,
                isLocalAdminFor: _.pluck(response.adminRights, 'organizationId'),
                defaultOrganizationUnitId: response.defaultOrganizationUnitId,
                currentOrganizationId: currOrg.id,
                currentOrganizationName: currOrg.name
            };

            $rootScope.user = _user;

            return _user;
        }

        function getUser() {

            var deferred = $q.defer();

            deferred.resolve(loadUser());

            loadUser().then(function(user) {
                console.log(user);
            });

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

                    resolveOrganization().then(function(currOrg) {
                        saveUser(result.response, currOrg);
                        loadUserDeferred.resolve(_user);

                    }, function() {
                        loadUserDeferred.reject("No organization selected");
                        loadUserDeferred = null;
                    });


                }).error(function(result) {
                    loadUserDeferred.reject("Not authorized");
                    loadUserDeferred = null;
                    clearSavedOrg();
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
                
            };

            return deferred.promise;
        }

        function logout() {
            

            var deferred = $q.defer();

            $http.post('api/authorize?logout').success(function (result) {
                loadUserDeferred = null;
                _user = null;
                $rootScope.user = null;

                deferred.resolve();

                clearSavedOrg();

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
        
        function getSavedOrg() {
            var org = localStorage.getItem("currentOrg");
            return org != "undefined" && JSON.parse(org);
        }
            
        function setSavedOrg(org) {
            localStorage.setItem("currentOrg", JSON.stringify(org));
        }
        
        function clearSavedOrg() {
            localStorage.setItem("currentOrg", "undefined");
        }

        function resolveOrganization() {

            var deferred = $q.defer();

            var storedOrganization = getSavedOrg();

            if (storedOrganization) {
                deferred.resolve(storedOrganization);

            } else {

                //fetch the relevant organizations
                $http.get("api/user?organizations").success(function (result) {

                    var organizations = result.response.organizations;
                    
                    //if there's only one, just select that
                    if (organizations.length == 1) {
                        
                        var org = organizations[0];
                        setSavedOrg(org);
                        
                        deferred.resolve(org);
                        
                    } else {
                        
                        //otherwise, open a modal 
                        var modal = $modal.open({
                            backdrop: 'static',
                            templateUrl: 'partials/home/choose-organization.html',
                            controller: ['$scope', '$modalInstance', function($modalScope, $modalInstance) {

                                $modalScope.orgChooser = {
                                    selectedId: result.response.defaultOrganizationId
                                };
                                $modalScope.organizations = organizations;

                                $modalScope.ok = function() {

                                    var org = _.findWhere(organizations, { id: $modalScope.orgChooser.selectedId });

                                    $modalInstance.close(org);

                                };

                            }]
                        });

                        modal.result.then(function(org) {

                            setSavedOrg(org);
                            deferred.resolve(org);

                        }, function () {
                            
                            deferred.reject("Modal dismissed");
                        });
                        
                    }

                }).error(function() {
                    deferred.reject("Request for the users organizations failed!");
                });

            }

            return deferred.promise;
        }


        return {
            getUser: getUser,
            login: login,
            logout: logout,
            auth: auth
        };

    }]);
    
    //which orgs do the user belong to - for the select box
    //$http.get("api/user?organizations").success(function (result) {
    //    var orgs = result.response.organizations;
    //    $rootScope.user.organizations = orgs;

    //    var defaultOrgId = result.response.defaultOrganizationId;

    //    $rootScope.user.defaultOrganizationId = defaultOrgId;

    //    if (defaultOrgId != 0) {
    //        $rootScope.user.currentOrganizationId = defaultOrgId;
    //    } else if (orgs.length > 0) {
    //        $rootScope.user.currentOrganizationId = orgs[0].id;
    //    }

    //});


})(angular, app);