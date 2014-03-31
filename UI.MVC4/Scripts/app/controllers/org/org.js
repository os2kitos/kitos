(function (ng, app) {

    var subnav = [
            { state: 'org-view', text: 'Organisation' }
    ];


    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('org-view', {
            url: '/organization',
            templateUrl: 'partials/org/org.html',
            controller: 'org.OrgViewCtrl',
            resolve: {
                orgRolesHttp: ['$http', function($http) {
                    return $http.get('api/organizationRole');
                }]
            }
        });

    }]);

    app.controller('org.OrgViewCtrl', ['$rootScope', '$scope', '$http', 'growl', 'orgRolesHttp', '$q', function ($rootScope, $scope, $http, growl, orgRolesHttp, $q) {
        $rootScope.page.title = 'Organisation';
        $rootScope.page.subnav = subnav;

        var userId = $rootScope.user.id;
        
        //cache
        var orgs = [];

        //flatten map of all loaded orgUnits
        $scope.orgUnits = [];

        $scope.orgRoles = {};
        _.each(orgRolesHttp.data.Response, function(orgRole) {
            $scope.orgRoles[orgRole.Id] = orgRole;
        });

        console.log($scope.orgRoles);
        
        
        function flatten(orgUnit) {
            $scope.orgUnits[orgUnit.Id] = orgUnit;

            _.each(orgUnit.Children, flatten);
        }
        
        $http.get('api/organizationunit?userId=' + userId).success(function(result) {
            $scope.nodes = result.Response;

            _.each(result.Response, flatten);
        });

        
        $scope.chosenOrgUnit = null;
        
        $scope.chooseOrgUnit = function (node) {
            
            if (!node.Organization) {

                //try get from cache
                if (orgs[node.Organization_Id]) {
                    
                    node.Organization = orgs[node.Organization_Id];
                    
                } else {
                    //else get from server
                    
                    $http.get('api/organization/' + node.Organization_Id).success(function (data) {
                        node.Organization = data.Response;
                        
                        //save to cache
                        orgs[node.Organization_Id] = data.Response;
                    });
                }
            }
            
            if (!node.OrgRights) {
                $http.get('api/organizationRight?organizationUnitId=' + node.Id).success(function(data) {
                    node.OrgRights = data.Response;
                });
            }
            
            $scope.chosenOrgUnit = node;
        };

        $scope.userSelectOptions = {
            initSelection: function (element, callback) {
            },
            ajax: {
                data: function (term, page) {
                    return { query: term };
                },
                quietMillis: 500,
                transport: function (queryParams) {
                    //console.log(queryParams);
                    var res = $http.get('api/user?q=' + queryParams.data.query).then(queryParams.success);
                    res.abort = function () {
                        console.log('Aborting...');
                        return null;
                    };

                    return res;
                },
                results: function (data, page) {
                    console.log(data);
                    var results = [];

                    _.each(data.data.Response, function (user) {
                        results.push({
                            id: user.Id,
                            text: user.Name
                        });
                    });

                    return { results: results };
                }
            }


        };

        $scope.submitRight = function () {
            var data = {
                'Object_Id': $scope.chosenOrgUnit.Id,
                'Role_Id': $scope.newRole,
                'User_Id': $scope.selectedUser.id
            };

            console.log(data);
        };

        $scope.updateTask = function (task) {
            task.selected = !task.selected;
            var orgUnitId = $scope.chosenOrgUnit.Id;
            if (task.selected === true) {
                $http.post('api/organizationUnit/' + orgUnitId + '?taskref=' + task.Id).finally(updateLists());
            } else {
                $http.delete('api/organizationUnit/' + orgUnitId + '?taskref=' + task.Id).finally(updateLists());
            }
        };

        function updateLists() {
            var orgUnitId = $scope.chosenOrgUnit.Id,
                orgUnitParentId = $scope.chosenOrgUnit.Parent_Id;

            var listOfQs = [];
            listOfQs.push($http.get('api/organizationUnit/' + orgUnitId + '?taskrefs'));
            
            if (orgUnitParentId === 0) { // if root org unit list all taskrefs as available
                listOfQs.push($http.get('api/taskref'));
            } else { // else only show selected on parent orgunit
                listOfQs.push($http.get('api/organizationUnit/' + orgUnitParentId + '?taskrefs'));
            }

            $q.all(listOfQs).then(function (result) {
                // NOTE the order of promises is important!
                var chosenTaskRefs = result[0].Response,
                    availableTaskRefs = result[1].Response;
                
                angular.forEach(chosenTaskRefs, function(selectedTasks) {
                    var task = _.find(availableTaskRefs, function(availableTasks) {
                        return availableTasks.Id == selectedTasks.Id;
                    });
                    task.selected = true;
                });
                
                $scope.chosenTaskRefs = chosenTaskRefs;
                $scope.availableTaskRefs = availableTaskRefs;
            });
        }

        $scope.$watch('chosenOrgUnit', function (newOrgUnit, oldOrgUnit) {
            if (newOrgUnit !== null)
                updateLists();
        });
    }]);

})(angular, app);