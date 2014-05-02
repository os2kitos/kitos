(function (ng, app) {

    var user = null;

    app.factory('userFactory', ['$http', '$q', function ($http, $q) {

        //formats and saves the user
        function saveUser(response) {
            var isLocalAdmin = _.some(response.adminRights, function(userRight) {
                return userRight.roleName == "LocalAdmin";
            });

            user = {
                isAuth: true,
                id: response.id,
                name: response.name,
                email: response.email,
                isGlobalAdmin: response.isGlobalAdmin,
                isLocalAdmin: isLocalAdmin,
                isLocalAdminFor: _.pluck(response.adminRights, 'organizationId'),
                defaultOrganizationUnitId: response.defaultOrganizationUnitId
            };
        }
        
        function getUser() {
            var deferred = $q.defer();
            
            if (user != null) {
                deferred.resolve(user);

            } else {
                $http.get('api/authorize').success(function(result) {
                    saveUser(result.response);
                    deferred.resolve(user);

                }).error(function(result) {
                    deferred.reject("Not authorized");
                });

            }

            return deferred;
        }
        
        function logIn(email, password, rememberMe) {
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

                    deferred.resolve(user);

                }).error(function (result) {

                    deferred.reject("Bad credentials");

                });

            };

            return deferred;
        }

        return {
            getUser: getUser,
            logIn: logIn
        };

    }]);


})(angular, app);