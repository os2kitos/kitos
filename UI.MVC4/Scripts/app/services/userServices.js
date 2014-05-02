(function (ng, app) {

    var _user = null;

    app.factory('userService', ['$http', '$q', '$rootScope', function ($http, $q, $rootScope) {
        
        //formats and saves the user
        function saveUser(response) {
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
                defaultOrganizationUnitId: response.defaultOrganizationUnitId
            };

            $rootScope.user = _user;

            return _user;
        }
        
        function getUser() {
            console.log("GET USER");
            var deferred = $q.defer();
            
            if (_user !== null) {
                deferred.resolve(_user);

            } else {
                $http.get('api/authorize').success(function(result) {
                    saveUser(result.response);
                    deferred.resolve(_user);

                }).error(function(result) {
                    deferred.reject("Not authorized");
                });

            }

            return deferred.promise;
        }
        
        function login(email, password, rememberMe) {
            var deferred = $q.defer();

            if (!email || !password) {

                deferred.reject("Invalid credentials");

            } else {
                
                 var data = {
                    "email": email,
                    "password": password,
                    "rememberMe": rememberMe
                };

                $http.post('api/authorize', data).success(function (result) {
                    saveUser(result.response);

                    deferred.resolve(_user);

                }).error(function (result) {

                    deferred.reject("Bad credentials");

                });

            };

            return deferred.promise;
        }

        function logout() {
            
            var deferred = $q.defer();

            $http.post('api/authorize?logout').success(function (result) {
                _user = null;
                $rootScope.user = null;

                deferred.resolve();

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

                    if (!hasRequiredRole) return $q.reject();
                }

                return true;

            });

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