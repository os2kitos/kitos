(function (ng, app) {
   
    var subnav = [
            { state: "global-admin", text: "Opret organisation" },
            { state: "global-admins", text: "Globale administratorer" },
            { state: "local-admins", text: "Lokale administratorer" }
    ];

    
    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('global-admin', {
            url: '/global-admin',
            templateUrl: 'partials/global-admin/new-organization.html',
            controller: 'globalAdmin.NewOrganizationCtrl',
            authRoles: ['GlobalAdmin']
        }).state('local-admins', {
            url: '/global-admin/local-admins',
            templateUrl: 'partials/global-admin/local-admins.html',
            controller: 'globalAdmin.LocalAdminsCtrl',
            authRoles: ['GlobalAdmin']
        })
            .state('global-admins', {
                url: '/global-admin/global-admins',
                templateUrl: 'partials/global-admin/global-admins.html',
                controller: 'globalAdmin.GlobalAdminsCtrl',
                authRoles: ['GlobalAdmin']
            });

    }]);
    
    
    app.controller('globalAdmin.NewOrganizationCtrl', ['$rootScope', '$scope', '$http', 'notify', function ($rootScope, $scope, $http, notify) {
        $rootScope.page.title = 'Ny organisation';
        $rootScope.page.subnav = subnav;

        $scope.submit = function() {
            if ($scope.addForm.$invalid) return;

            var data = { "name": $scope.name };
            
            $http.post('api/organization', data).success(function (result) {
                notify.addSuccessMessage("Organisationen " + $scope.name + " er blevet oprettet!");

                $scope.name = "";
            }).error(function (result) {
                notify.addErrorMessage("Organisationen " + $scope.name + " kunne ikke oprettes!");
            });
        };
    }]);

    /*
    app.controller('globalAdmin.NewLocalAdminCtrl', ['$rootScope', '$scope', '$http', 'notify', function ($rootScope, $scope, $http, notify) {
        $rootScope.page.title = 'Ny lokal admin';
        $rootScope.page.subnav = subnav;

        $scope.organizations = [];
        $http.get("api/organization").success(function(result) {
            $scope.organizations = result.response;
        });

        $scope.submit = function () {
            if ($scope.addForm.$invalid) return;

            var selectedUser = $scope.selectedUser;
            var orgId = $scope.organization;

            var data = {
                "userId": selectedUser.id,
                "organizationId": orgId
            };

            $http.post('api/localadmin', data).success(function (result) {
                notify.addSuccessMessage(selectedUser.text + " er blevet lokal admin!");

                $scope.selectedUser = null;
                $scope.organization = ""; 
                
            }).error(function (result) {
                notify.addErrorMessage("Fejl! " + selectedUser.text + " blev ikke lokal admin!");
            });
        };
    }]);

    app.controller('globalAdmin.NewGlobalAdminCtrl', ['$rootScope', '$scope', '$http', 'notify', function ($rootScope, $scope, $http, notify) {
        $rootScope.page.title = 'Ny global admin';
        $rootScope.page.subnav = subnav;

        $scope.submit = function () {
            if ($scope.addForm.$invalid) return;

            var selectedUser = $scope.selectedUser;

            var data = {
                "userId": selectedUser.id,
            };

            $http.post('api/globaladmin', data).success(function (result) {
                notify.addSuccessMessage(selectedUser.text + " er blevet global admin!");

                $scope.selectedUser = null;

            }).error(function (result) {
                notify.addErrorMessage("Fejl! " + selectedUser.text + " blev ikke global admin!");
            });
        };
    }]);*/
    
    app.controller('globalAdmin.LocalAdminsCtrl', ['$rootScope', '$scope', '$http', 'notify', function ($rootScope, $scope, $http, notify) {
        $rootScope.page.title = 'Lokal administratorer';
        $rootScope.page.subnav = subnav;

        $scope.organizations = {};
        $http.get("api/organization").success(function (result) {
            _.each(result.response, function (org) {
                $scope.organizations[org.id] = org;
            });
        });

        var localAdminRole = null;
        $http.get("api/adminrole?getLocalAdminRole=true").success(function(result) {
            localAdminRole = result.response;
        });

        $scope.adminRights = [];
        function pushRight(right) {
            right.show = true;

            right.orgForSelect = right.objectId;
            right.userForSelect = {
                id: right.user.id,
                text: right.user.name,
                user: right.user
            };

            $scope.adminRights.push(right);
        }

        $http.get("api/adminright").success(function(result) {
            _.each(result.response, pushRight);
        });
        
        function newLocalAdmin() {
            if (!(localAdminRole && $scope.newOrg && $scope.newUser)) return;

            var user = $scope.newUser;
            var uId = user.id;
            var oId = parseInt($scope.newOrg);
            var orgName = $scope.organizations[oId].name;
            
            var rId = localAdminRole.id;

            if (!(uId && oId && rId)) return;

            var data = {
                objectId: oId,
                userId: uId,
                roleId: rId,
            };

            console.log(data);
            var msg = notify.addInfoMessage("Arbejder ...", false);
            
            $http.post("api/adminright", data, { handleBusy: true }).success(function(result) {
                msg.toSuccessMessage(user.text + " er blevet lokal administrator for " + orgName);
                $scope.newUser = null;
                $scope.newOrg = null;

                pushRight(result.response);

            }).error(function (result) {
                msg.toErrorMessage("Kunne ikke gøre " + user.text + " til lokal administrator for " + orgName);
            });
        }

        $scope.$watch("newUser", function(newVal, oldVal) {
            if (newVal === oldVal || !newVal) return;

            newLocalAdmin();
        });
        
        $scope.$watch("newOrg", function (newVal, oldVal) {
            if (newVal === oldVal || !newVal) return;

            newLocalAdmin();
        });

        $scope.deleteLocalAdmin = function(right) {

            var oId = right.objectId;
            var rId = right.roleId;
            var uId = right.userId;

            var msg = notify.addInfoMessage("Arbejder ...", false);
            $http.delete("api/adminRight?oId=" + oId + "&rId=" + rId + "&uId=" + uId).success(function (deleteResult) {
                right.show = false;
                msg.toSuccessMessage(right.user.name + " er ikke længere lokal administrator");
            }).error(function (deleteResult) {

                msg.toErrorMessage("Kunne ikke fjerne " + right.user.name + " som lokal administrator");
            });
        };

        $scope.updateLocalAdmin = function(right) {

            if (!right.orgForSelect || !right.userForSelect || !localAdminRole) return;

            //old values
            var oIdOld = right.objectId;
            var uIdOld = right.userId;

            //new values
            var oIdNew = parseInt(right.orgForSelect);
            var uIdNew = right.userForSelect.id;

            //if nothing was changed, just exit edit-mode
            if (oIdOld == oIdNew && uIdOld == uIdNew) {
                right.edit = false;
                return;
            }
            
            //otherwise, we should delete the old entry, then add a new one

            var rId = localAdminRole.id;

            var msg = notify.addInfoMessage("Arbejder ...", false);
            
            $http.delete("api/adminRight?oId=" + oIdOld + "&rId=" + rId + "&uId=" + uIdOld, {handleBusy: true}).success(function (deleteResult) {

                var newData = {
                    objectId: oIdNew,
                    userId: uIdNew,
                    roleId: rId,
                };
                
                var orgName = $scope.organizations[oIdNew].name;

                $http.post("api/adminRight", newData).success(function (result) {

                    right.roleId = result.response.roleId;
                    right.user = result.response.user;
                    right.userId = result.response.userId;

                    right.edit = false;
                    
                    msg.toSuccessMessage(right.user.name + " er blevet lokal administrator for " + orgName);

                }).error(function (result) {

                    //we successfully deleted the old entry, but didn't add a new one
                    //fuck

                    right.show = false;
                    
                    msg.toErrorMessage("Kunne ikke gøre " + right.userForSelect.text + " til lokal administrator for " + orgName);
                });

            }).error(function (deleteResult) {

                //couldn't delete the old entry, just reset select options
                right.userForSelect = { id: right.user.id, text: right.user.name };
                right.orgForSelect = right.objectId;

                msg.toErrorMessage("Fejl!");
            });
        };
    }]);
        
        /* GLOBAL ADMIN */
        app.controller('globalAdmin.GlobalAdminsCtrl', ['$rootScope', '$scope', '$http', 'notify', function ($rootScope, $scope, $http, notify) {
            $rootScope.page.title = 'Globale administratorer';
            $rootScope.page.subnav = subnav;


            $scope.globalAdmins = [];
            function pushAdmin(user) {
                var admin = {
                    show: true,
                    user: user,
                    userForSelect: {
                        id: user.id,
                        text: user.name,
                        user: user
                    }
                };

                $scope.globalAdmins.push(admin);
            }

            $http.get("api/globaladmin").success(function(result) {
                _.each(result.response, pushAdmin);
            });
        
            function newGlobalAdmin() {
                if (!$scope.newUser) return;

                var user = $scope.newUser;
                var uId = user.id;
            

                if (!uId) return;

                var data = {
                    userId: uId
                };

                var msg = notify.addInfoMessage("Arbejder ...", false);
            
                $http.post("api/globaladmin", data, { handleBusy: true }).success(function (result) {
                    msg.toSuccessMessage(user.text + " er blevet global administrator");
                    $scope.newUser = null;

                    pushAdmin(result.response);

                }).error(function (result) {
                    msg.toErrorMessage("Kunne ikke gøre " + user.text + " til global administrator");
                });
            }

            $scope.$watch("newUser", function(newVal, oldVal) {
                if (newVal === oldVal || !newVal) return;

                newGlobalAdmin();
            });

            $scope.deleteGlobalAdmin = function(admin) {

                var uId = admin.user.id;

                var msg = notify.addInfoMessage("Arbejder ...", false);
                $http.delete("api/globaladmin?userId=" + uId).success(function (deleteResult) {
                    admin.show = false;
                    msg.toSuccessMessage(admin.user.name + " er ikke længere global administrator");
                    
                }).error(function (deleteResult) {
                    msg.toErrorMessage("Kunne ikke fjerne " + admin.user.name + " som global administrator");
                });
            };

            $scope.updateGlobalAdmin = function(admin) {

                if (!admin.userForSelect) return;

                var user = admin.userForSelect;

                //old values
                var uIdOld = admin.user.id;

                //new values
                var uIdNew = user.id;

                if (!uIdNew) return;

                //if nothing was changed, just exit edit-mode
                if (uIdOld == uIdNew) {
                    admin.edit = false;
                    return;
                }
            
                //otherwise, we should delete the old entry, then add a new one

                var msg = notify.addInfoMessage("Arbejder ...", false);
            
                $http.delete("api/globaladmin?userId=" + uIdOld, { handleBusy: true }).success(function (deleteResult) {


                    var data = {
                        userId: uIdNew
                    };

                    $http.post("api/globaladmin", data, { handleBusy: true }).success(function (result) {
                        msg.toSuccessMessage(user.text + " er blevet global administrator");
                        $scope.newUser = null;

                        var newUser = result.response;

                        admin.user = newUser;
                        admin.userForSelect = {
                            id: newUser.id,
                            text: newUser.text,
                            user: newUser
                        };

                        admin.edit = false;

                    }).error(function (result) {
                        //we successfully deleted the old entry, but didn't add a new one
                        //fuck
                        admin.show = false;
                        
                        msg.toErrorMessage("Kunne ikke gøre " + user.text + " til global administrator");
                    });
                    
                }).error(function (deleteResult) {

                    //couldn't delete the old entry, just reset select options
                    admin.userForSelect = { id: admin.user.id, text: admin.user.name };

                    msg.toErrorMessage("Fejl!");
                });
            };

    }]);

})(angular, app);