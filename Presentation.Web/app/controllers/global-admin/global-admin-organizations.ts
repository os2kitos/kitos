(function(ng, app) {
    app.config([
        '$stateProvider', function($stateProvider) {
            $stateProvider.state('global-admin.organizations', {
                url: '/organisations',
                templateUrl: 'partials/global-admin/organizations.html',
                controller: 'globalAdmin.organizationCtrl',
                authRoles: ['GlobalAdmin'],
                resolve: {
                    user: [
                        'userService', function (userService) {
                            return userService.getUser();
                        }
                    ]
                }
            });
        }
    ]);

    app.controller('globalAdmin.organizationCtrl', [
        '$rootScope', '$scope', '$http', 'notify', 'user', function($rootScope, $scope, $http, notify, user) {
            $rootScope.page.title = 'Organisationer';

            $scope.pagination = {
                search: '',
                skip: 0,
                take: 100
            };

            $scope.$watchCollection('pagination', function () {
                loadOrganizations();
            });

            function loadOrganizations() {

                var url = 'api/organization/';
                url += '?skip=' + $scope.pagination.skip + "&take=" + $scope.pagination.take;

                if ($scope.pagination.orderBy) {
                    url += '&orderBy=' + $scope.pagination.orderBy;
                    if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                }

                if ($scope.pagination.search) url += '&q=' + $scope.pagination.search;
                else url += "&q=";

                $http.get(url).success(function (result, status, headers) {
                    var paginationHeader = JSON.parse(headers('X-Pagination'));
                    $scope.totalCount = paginationHeader.TotalCount;
                    $scope.organizations = result.response;
                }).error(function () {
                    notify.addErrorMessage("Kunne ikke hente organisationer!");
                });
            }
            
            $scope.delete = function (orgId) {
                $http.delete('api/organization/' + orgId + '?organizationId=' + user.currentOrganizationId)
                    .success(function() {
                        notify.addSuccessMessage("Organisationen er blevet slettet!");
                        loadOrganizations();
                    })
                    .error(function () {
                        notify.addErrorMessage("Kunne ikke slette organisationen!");
                    });
            }
        }
    ]);
})(angular, app);
