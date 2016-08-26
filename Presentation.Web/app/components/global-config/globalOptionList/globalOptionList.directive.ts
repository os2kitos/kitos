(function(ng, app) {
    'use strict';

    app.directive('globalOptionList', [
        '$http', '$timeout', '$state', '$stateParams', 'notify', function($http, $timeout, $state, $stateParams, notify) {
            return {
                scope: {
                    optionsUrl: '@',
                    title: '@',
                    orgid: '@'
                },
                templateUrl: 'app/shared/optionList/optionList.view.html',
                link: function(scope, element, attrs) {

                    scope.list = [];
                    $http.get(scope.optionsUrl + '?organizationId=' + scope.orgid + '&nonsuggestions').success(function(result) {
                        _.each(result.response, function(v) {
                            scope.list.push({
                                id: v.id,
                                name: v.name,
                                note: v.note,
                                isActive: v.isActive
                            });
                        });
                    });

                    scope.suggestions = [];
                    $http.get(scope.optionsUrl + '?organizationId=' + scope.orgid + '&suggestions').success(function(result) {
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
                        $http({ method: 'PATCH', url: scope.optionsUrl + '/' + id + '?organizationId=' + scope.orgid, data: { isSuggestion: false } })
                            .success(function() {
                                msg.toSuccessMessage("Valgmuligheden er opdateret.");
                                // reload page to show changes
                                reload();
                            })
                            .error(function() {
                                msg.toErrorMessage("Fejl! Valgmuligheden kunne ikke ændres!");
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
