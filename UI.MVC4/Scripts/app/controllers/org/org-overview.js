(function (ng, app) {
    
    function indent(level) {
        var result = "";
        for (var i = 0; i < level; i++) result += ".....";

        return result;
    };
    
    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('organization.overview', {
            url: '/overview',
            templateUrl: 'partials/org/overview.html',
            controller: 'org.OverviewCtrl',
            resolve: {
            }
        });

    }]);

    app.controller('org.OverviewCtrl', ['$rootScope', '$scope', '$http', 'notify', '$modal', 'user',
        function ($rootScope, $scope, $http, notify, $modal, user) {
        $rootScope.page.title = 'Organisation - Overblik';
            
        $scope.orgUnits = {};
        $scope.orgUnitTree = [];

        loadUnits();
        checkForDefaultUnit();

        $scope.selectOrgUnitOptions = {
            escapeMarkup: function(m) { return m; }
        };

        function visitOrgUnit(orgUnit) {
            
            $scope.orgUnits[orgUnit.id] = orgUnit;

            _.each(orgUnit.children, function(child) {
                return visitOrgUnit(child);
            });
        }

        function hasWriteAccess(orgUnit, inherit) {
            if (inherit) {
                orgUnit.hasWriteAccess = true;

                _.each(orgUnit.children, function(child) {
                    hasWriteAccess(child, true);
                });
            } else {
                $http.get('api/organizationUnit/' + orgUnit.id + '?hasWriteAccess').success(function(result) {
                    orgUnit.hasWriteAccess = result.response;

                    _.each(orgUnit.children, function(child) {
                        hasWriteAccess(child, result.response);
                    });

                });

            }
        }
        
        function checkForDefaultUnit() {
            if (!user.defaultOrganizationUnitId) return;

            $scope.orgUnitId = user.defaultOrganizationUnitId;
            loadUsages();
        }
        
        function loadUnits() {

            return $http.get('api/organizationunit?organization=' + user.currentOrganizationId).success(function (result) {
                var rootNode = result.response;

                $scope.nodes = [rootNode];

                visitOrgUnit(rootNode);
                hasWriteAccess(rootNode, false);
            });
        }

        /* load task usages */
        function loadUsages() {
            if (!$scope.orgUnitId) return;

            $scope.taskUsages = {};
            $http.get('api/taskusage?orgUnitId=' + $scope.orgUnitId + "&onlyStarred=true").success(function (result) {
                $scope.taskUsages = result.response;

                /* visit every task usage and delegation */
                function visit(usage, parent, level, altStyle, task) {
                    usage.usage.updateUrl = 'api/taskUsage/' + usage.usage.id;
                    usage.usage.task = task;
                    usage.usage.orgUnit = $scope.orgUnits[usage.usage.orgUnitId];

                    usage.parent = parent;
                    usage.level = level;
                    usage.altStyle = altStyle;

                    /* if this task hasn't been delegated, it's a leaf. A leaf can select and update the statuses
                     * at which point we need to update the parents statuses as well */
                    if (!usage.hasDelegations) {
                        $scope.$watch(function () { return usage.usage.technologyStatus; }, function (newVal, oldVal) {
                            updateTechStatus(usage);
                        });
                        $scope.$watch(function () { return usage.usage.usageStatus; }, function (newVal, oldVal) {
                            updateUsageStatus(usage);
                        });
                    }

                    /* visit children */
                    _.each(usage.delegations, function (del) {
                        visit(del, usage, level + 1, !altStyle, task);
                    });
                }

                /* each of these are root usages */
                _.each($scope.taskUsages, function (usage) {
                    usage.isRoot = true;

                    $http.get('api/taskusage/' + usage.usage.id + '?projects').success(function(result) {
                        usage.usage.projects = result.response;
                    });
                    
                    $http.get('api/taskusage/' + usage.usage.id + '?systems').success(function (result) {
                        usage.usage.systems = result.response;
                    });


                    $http.get('api/taskref/' + usage.usage.taskRefId).success(function (result) {
                        visit(usage, null, 0, false, result.response);

                        updateTechStatus(usage);
                        updateUsageStatus(usage);
                    });
                });
            });
        };
        $scope.loadUsages = loadUsages;

        function getTask(usage) {
            if (usage.parent) return getTask(usage.parent);

        }

        function updateTechStatus(usage) {
            if (usage.parent) return updateTechStatus(usage.parent);

            calculateTechStatus(usage);
        };

        function updateUsageStatus(usage) {
            if (usage.parent) updateUsageStatus(usage.parent);

            calculateUsageStatus(usage);
        };

        /* helper function to aggregate status-trafficlight */
        function addToStatusResult(status, result) {
            if (status == 2) result.green++;
            else if (status == 1) result.yellow++;
            else result.red++;

            result.max++;

            return result;
        }

        /* helper function to sum two status-trafficlights */
        function sumStatusResult(result1, result2) {
            return {
                max: result1.max + result2.max,
                red: result1.red + result2.red,
                yellow: result1.yellow + result2.yellow,
                green: result1.green + result2.green
            };
        }

        function calculateTechStatus(usage) {

            /* this will hold the aggregated tech status of this node */
            var result = {
                max: 0,
                red: 0,
                yellow: 0,
                green: 0
            };

            /* if the usage isn't delegated, the agg result is just this tech status */
            if (!usage.hasDelegations) {
                result = addToStatusResult(usage.usage.technologyStatus, result);
            } else {
                _.each(usage.delegations, function (delegation) {
                    var delegationResult = calculateTechStatus(delegation);
                    result = sumStatusResult(result, delegationResult);
                });
            }

            usage.calculatedTechStatus = result;

            return result;
        };


        function calculateUsageStatus(usage) {

            var result = {
                max: 0,
                red: 0,
                yellow: 0,
                green: 0
            };

            if (!usage.hasDelegations) {
                return addToStatusResult(usage.usage.usageStatus, result);
            }



            _.each(usage.delegations, function (delegation) {
                var delegationResult = calculateUsageStatus(delegation);
                result = sumStatusResult(result, delegationResult);
            });

            usage.calculatedUsageStatus = result;

            return result;
        };

        $scope.indent = indent;

        $scope.openComment = function (usage) {
            $modal.open({
                templateUrl: 'partials/org/overview/comment-modal.html',
                controller: ['$scope', '$modalInstance', function ($modalScope, $modalInstance) {
                    $modalScope.usage = usage;
                    
                    $modalScope.hasWriteAccess = usage.usage.orgUnit.hasWriteAccess;
                }]
            });
        };
        
    }]);
})(angular, app);