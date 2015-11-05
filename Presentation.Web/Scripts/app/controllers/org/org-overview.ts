﻿(function(ng, app) {
    function indent(level) {
        var result = "";
        for (var i = 0; i < level; i++) result += ".....";

        return result;
    };

    app.config([
        '$stateProvider', function($stateProvider) {
            $stateProvider.state('organization.overview', {
                url: '/overview',
                templateUrl: 'partials/org/overview.html',
                controller: 'org.OverviewCtrl',
                resolve: {
                }
            });
        }
    ]);

    app.controller('org.OverviewCtrl', [
        '$rootScope', '$scope', '$http', 'notify', '$modal', 'user',
        function($rootScope, $scope, $http, notify, $modal, user) {
            $rootScope.page.title = 'Organisation - Overblik';

            function checkForDefaultUnit() {
                if (!user.currentOrganizationUnitId) return;

                $scope.orgUnitId = user.currentOrganizationUnitId;
            }
            checkForDefaultUnit();

            $scope.pagination = {
                skip: 0,
                take: 10,
                orderBy: 'taskRef.taskKey'
            };

            $scope.csvUrl = 'api/taskusage/?csv&orgUnitId=' + $scope.orgUnitId + '&onlyStarred=true' + '&organizationId=' + user.currentOrganizationId;

            $scope.$watchCollection('pagination', function() {
                //var url = 'api/taskusage/?csv&orgUnitId=' + $scope.orgUnitId + '&onlyStarred=true';

                //url += '&skip=' + $scope.pagination.skip;
                //url += '&take=' + $scope.pagination.take;

                //if ($scope.pagination.orderBy) {
                //    url += '&orderBy=' + $scope.pagination.orderBy;
                //    if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                //}

                //$scope.csvUrl = url;
                loadUsages();
            });

            /* load task usages */
            function loadUsages() {
                if (!$scope.orgUnitId) return;

                var url = 'api/taskusage/?orgUnitId=' + $scope.orgUnitId + '&onlyStarred=true' + '&organizationId=' + user.currentOrganizationId;

                url += '&skip=' + $scope.pagination.skip;
                url += '&take=' + $scope.pagination.take;

                if ($scope.pagination.orderBy) {
                    url += '&orderBy=' + $scope.pagination.orderBy;
                    if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                }

                $http.get(url).success(function(result, status, headers) {
                    var paginationHeader = JSON.parse(headers('X-Pagination'));
                    $scope.totalCount = paginationHeader.TotalCount;
                    $scope.taskUsages = result.response;

                    /* visit every task usage and delegation */
                    function visit(usage, parent, level) {
                        usage.updateUrl = 'api/taskUsage/' + usage.id;
                        usage.parent = parent;
                        usage.level = level;

                        if (parent) usage.hasWriteAccess = parent.hasWriteAccess;

                        /* if this task hasn't been delegated, it's a leaf. A leaf can select and update the statuses
                         * at which point we need to update the parents statuses as well
                        */
                        if (!usage.hasDelegations) {
                            $scope.$watch(function() { return usage.technologyStatus; }, function(newVal, oldVal) {
                                updateTechStatus(usage);
                            });
                            $scope.$watch(function() { return usage.usageStatus; }, function(newVal, oldVal) {
                                updateUsageStatus(usage);
                            });
                        }

                        /* visit children */
                        _.each(usage.children, function(child) {
                            visit(child, usage, level + 1);
                        });
                    }

                    /* each of these are root usages */
                    _.each($scope.taskUsages, function(usage: { isRoot }) {
                        usage.isRoot = true;

                        visit(usage, null, 0);

                        updateTechStatus(usage);
                        updateUsageStatus(usage);
                    });
                });
            };

            $scope.loadUsages = loadUsages;

            function updateTechStatus(usage) {
                if (usage.parent) {
                    updateTechStatus(usage.parent);
                } else {
                    calculateTechStatus(usage);
                }
            };

            function updateUsageStatus(usage) {
                if (usage.parent) {
                    updateUsageStatus(usage.parent);
                } else {
                    calculateUsageStatus(usage);
                }
            };

            /* helper function to aggregate status-trafficlight */
            function addToStatusResult(status, result) {
                if (status == 3) result.green++;
                else if (status == 2) result.yellow++;
                else if (status == 1) result.red++;
                else result.white++;

                result.max++;

                return result;
            }

            /* helper function to sum two status-trafficlights */
            function sumStatusResult(result1, result2) {
                return {
                    max: result1.max + result2.max,
                    white: result1.white + result2.white,
                    red: result1.red + result2.red,
                    yellow: result1.yellow + result2.yellow,
                    green: result1.green + result2.green
                };
            }

            function calculateTechStatus(usage) {

                /* this will hold the aggregated tech status of this node */
                var result = {
                    max: 0,
                    white: 0,
                    red: 0,
                    yellow: 0,
                    green: 0,
                };

                /* if the usage isn't delegated, the agg result is just this tech status */
                if (!usage.hasDelegations) {
                    result = addToStatusResult(usage.technologyStatus, result);
                } else {
                    _.each(usage.children, function(child) {
                        var delegationResult = calculateTechStatus(child);
                        result = sumStatusResult(result, delegationResult);
                    });
                }

                usage.calculatedTechStatus = result;

                return result;
            };


            function calculateUsageStatus(usage) {
                var result = {
                    max: 0,
                    white: 0,
                    red: 0,
                    yellow: 0,
                    green: 0
                };

                if (!usage.hasDelegations) {
                    return addToStatusResult(usage.usageStatus, result);
                }

                _.each(usage.children, function(child) {
                    var delegationResult = calculateUsageStatus(child);
                    result = sumStatusResult(result, delegationResult);
                });

                usage.calculatedUsageStatus = result;

                return result;
            };

            $scope.indent = indent;

            $scope.openComment = function(usage) {
                $modal.open({
                    templateUrl: 'partials/org/overview/comment-modal.html',
                    controller: [
                        '$scope', '$modalInstance', 'autofocus', function($modalScope, $modalInstance, autofocus) {
                            autofocus();
                            $modalScope.usage = usage;
                            $modalScope.hasWriteAccess = usage.hasWriteAccess;
                        }
                    ]
                });
            };
        }
    ]);
})(angular, app);
