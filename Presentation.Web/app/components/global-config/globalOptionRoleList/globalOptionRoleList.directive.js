(function(ng, app) {
    'use strict';

    app.directive('globalOptionRoleList', [
        '$http', '$timeout', '$state', '$stateParams', 'notify', 'userService', function($http, $timeout, $state, $stateParams, notify, userService) {
            return {
                scope: {
                    optionsUrl: '@',
                    title: '@',
                },
                templateUrl: 'app/components/global-config/globalOptionRoleList/globalOptionRoleList.view.html',
                link: function(scope, element, attrs) {
                    var user;
                    userService.getUser().then(function(result) {
                        user = result;
                    });
                    scope.list = [];
                    $http.get(scope.optionsUrl + '?nonsuggestions').success(function(result) {
                        _.each(result.response, function(v) {
                            scope.list.push({
                                id: v.id,
                                name: v.name,
                                note: v.note,
                                isActive: v.isActive,
                                hasWriteAccess: v.hasWriteAccess
                            });
                        });
                    });

                    scope.suggestions = [];
                    $http.get(scope.optionsUrl + '?suggestions').success(function(result) {
                        _.each(result.response, function(v) {
                            scope.suggestions.push({
                                id: v.id,
                                name: v.name,
                                note: v.note
                            });
                        });
                    });

                    scope.approve = function(id) {
                        var msg = notify.addInfoMessage("Gemmer...", false);
                        $http({ method: 'PATCH', url: scope.optionsUrl + '/' + id + '?organizationId=' + user.currentOrganizationId, data: { isSuggestion: false } })
                            .success(function() {
                                msg.toSuccessMessage("Rollen er opdateret.");
                                // reload page to show changes
                                reload();
                            })
                            .error(function() {
                                msg.toErrorMessage("Fejl! Rollen kunne ikke ændres!");
                            });
                    };

                    // work around for $state.reload() not updating scope
                    // https://github.com/angular-ui/ui-router/issues/582
                    function reload() {
                        return $state.transitionTo($state.current, $stateParams, {
                            reload: true
                        }).then(function() {
                            scope.hideContent = true;
                            return $timeout(function() {
                                return scope.hideContent = false;
                            }, 1);
                        });
                    };
                }
            };
        }
    ]);
})(angular, app);
