(function(ng, app) {
    app.config([
        '$stateProvider', function($stateProvider) {
            $stateProvider.state('global-admin.local-users', {
                url: '/local-admins',
                templateUrl: 'partials/global-admin/local-admins.html',
                controller: 'globalAdmin.localAdminsCtrl',
                authRoles: ['GlobalAdmin'],
                resolve: {
                    localAdminRole: [
                        '$http', function($http) {
                            return $http.get('api/adminrole?getLocalAdminRole=true').then(function(result) {
                                return result.data.response;
                            });
                        }
                    ],
                    adminRights: [
                        '$http', function($http) {
                            return $http.get('api/adminrights').then(function(result) {
                                return result.data.response;
                            });
                        }
                    ]
                }
            });
        }
    ]);

    app.controller('globalAdmin.localAdminsCtrl', [
        '$rootScope', '$scope', '$http', '$state', 'notify', 'localAdminRole', 'adminRights',
        function($rootScope, $scope, $http, $state, notify, localAdminRole, adminRights) {
            $rootScope.page.title = 'Lokal administratorer';
            $scope.adminRights = adminRights;

            function newLocalAdmin() {
                // select2 changes the value twice, first with invalid values
                // so ignore invalid values
                if (typeof $scope.newUser !== 'object') return;
                if (!(localAdminRole && $scope.newOrg && $scope.newUser)) return;

                var user = $scope.newUser;
                var uId = user.id;
                var oId = $scope.newOrg.id;
                var orgName = $scope.newOrg.text;

                var rId = localAdminRole.id;

                if (!(uId && oId && rId)) return;

                var data = {
                    userId: uId,
                    roleId: rId,
                };

                var msg = notify.addInfoMessage("Arbejder ...", false);
                $http.post("api/adminrights/" + oId, data, { handleBusy: true }).success(function(result) {
                    msg.toSuccessMessage(user.text + " er blevet lokal administrator for " + orgName);
                    reload();
                }).error(function() {
                    msg.toErrorMessage("Kunne ikke gøre " + user.text + " til lokal administrator for " + orgName);
                });
            }

            function reload() {
                $state.go('.', null, { reload: true });
            }

            $scope.$watch("newUser", function(newVal, oldVal) {
                if (newVal === oldVal || !newVal) return;

                newLocalAdmin();
            });

            $scope.$watch("newOrg", function(newVal, oldVal) {
                if (newVal === oldVal || !newVal) return;

                newLocalAdmin();
            });

            $scope.deleteLocalAdmin = function(right) {
                var oId = right.objectId;
                var rId = right.roleId;
                var uId = right.userId;

                var msg = notify.addInfoMessage("Arbejder ...", false);
                $http.delete("api/adminrights/" + oId + "?rId=" + rId + "&uId=" + uId).success(function(deleteResult) {
                    msg.toSuccessMessage(right.userName + " er ikke længere lokal administrator");
                    reload();
                }).error(function(deleteResult) {

                    msg.toErrorMessage("Kunne ikke fjerne " + right.userName + " som lokal administrator");
                });
            };

            $scope.organizationSelectOptions = selectLazyLoading('api/organization', formatOrganization);

            function formatOrganization(org) {
                var result = '<div>' + org.text + '</div>';
                if (org.cvr) {
                    result += '<div class="small">' + org.cvr + '</div>';
                }
                return result;
            }

            function selectLazyLoading(url, format, paramAry) {
                return {
                    minimumInputLength: 1,
                    allowClear: true,
                    placeholder: ' ',
                    formatResult: format,
                    ajax: {
                        data: function(term, page) {
                            return { query: term };
                        },
                        quietMillis: 500,
                        transport: function(queryParams) {
                            var extraParams = paramAry ? '&' + paramAry.join('&') : '';
                            var res = $http.get(url + '?q=' + queryParams.data.query + extraParams).then(queryParams.success);
                            res.abort = function() {
                                return null;
                            };

                            return res;
                        },

                        results: function(data, page) {
                            var results = [];

                            _.each(data.data.response, function(obj) {
                                results.push({
                                    id: obj.id,
                                    text: obj.name ? obj.name : 'Unavngiven',
                                    cvr: obj.cvr
                                });
                            });

                            return { results: results };
                        }
                    }
                };
            }
        }
    ]);
})(angular, app);
