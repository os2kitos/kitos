(function(ng, app) {
    'use strict';

    app.directive('autosave', [
        '$http', '$timeout', 'notify', 'userService', 'moment', function($http, $timeout, notify, userService, moment) {
            return {
                restrict: 'A',
                require: 'ngModel',
                priority: 0,
                link: function(scope, element, attrs, ctrl) {
                    var user; // TODO this isn't always ready when needed and will result in an error from time to time... angular 2 support resolve on directives so yeah...

                    userService.getUser().then(function(result) {
                        user = result;
                    });

                    var oldValue;
                    $timeout(function() {
                        oldValue = ctrl.$modelValue; // get initial value
                    });

                    function saveIfNew() {
                        var newValue = ctrl.$modelValue;
                        if (attrs.pluck)
                            newValue = _.pluck(newValue, attrs.pluck);

                        if (newValue !== oldValue) {
                            if (ctrl.$valid) {
                                var payload = {};
                                payload[attrs.field] = newValue;

                                save(payload);
                            }
                        }
                    }

                    function saveDate() {
                        var newValue = ctrl.$modelValue;
                        if (attrs.pluck)
                            newValue = _.pluck(newValue, attrs.pluck);

                        if (newValue !== oldValue) {
                            if (ctrl.$valid) {
                                var dateObject = moment(newValue, "DD-MM-YYYY");
                                var payload = {};
                                if (dateObject.isValid()) {
                                    payload[attrs.field] = dateObject.format("YYYY-MM-DD");
                                } else {
                                    payload[attrs.field] = null;
                                }

                                save(payload);
                            }
                        }
                    }

                    function saveCheckbox() {
                        // ctrl.$viewValue reflects the old state.
                        // using timeout to wait for the value to update
                        $timeout(function() {
                            var newValue = ctrl.$modelValue;
                            var payload = {};
                            payload[attrs.field] = newValue;
                            save(payload);
                        });
                    }

                    function saveSelect2() {
                        // ctrl.$viewValue reflects the old state.
                        // using timeout to wait for the value to update
                        $timeout(function() {
                            var newValue;

                            var viewValue = ctrl.$viewValue;
                            if (angular.isArray(viewValue)) {
                                newValue = _.pluck(viewValue, 'id');
                            } else if (angular.isObject(viewValue)) {
                                newValue = viewValue.id;
                            } else {
                                newValue = viewValue;
                            }

                            var payload = {};
                            payload[attrs.field] = newValue;

                            save(payload);
                        });
                    }

                    function saveMultipleSelect2(e) {
                        var id, msg = notify.addInfoMessage("Gemmer...", false);
                        if (e.added) {
                            id = e.added.id;
                            $http.post(attrs.autosave + '?organizationId=' + user.currentOrganizationId + '&' + attrs.field + '=' + id)
                                .success(function() {
                                    msg.toSuccessMessage("Feltet er opdateret.");
                                })
                                .error(function() {
                                    msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                });
                        } else if (e.removed) {
                            id = e.removed.id;
                            $http.delete(attrs.autosave + '?organizationId=' + user.currentOrganizationId + '&' + attrs.field + '=' + id)
                                .success(function() {
                                    msg.toSuccessMessage("Feltet er opdateret.");
                                })
                                .error(function() {
                                    msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                });
                        }
                    }

                    function save(payload) {
                        var msg = notify.addInfoMessage("Gemmer...", false);
                        if (!attrs.appendurl)
                            attrs.appendurl = '';

                        $http({ method: 'PATCH', url: attrs.autosave + '?organizationId=' + user.currentOrganizationId + attrs.appendurl, data: payload, ignoreLoadingBar: true})
                            .success(function() {
                                msg.toSuccessMessage("Feltet er opdateret.");
                                oldValue = ctrl.$modelValue;
                            })
                            .error(function(result, status) {
                                if (status === 409) {
                                    msg.toErrorMessage("Fejl! Feltet kunne ikke ændres da værdien den allerede findes i KITOS!");
                                } else {
                                    msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                }
                            });
                    }

                    // select2 fields trigger the change event
                    if (!angular.isUndefined(attrs.uiSelect2)) {
                        if (attrs.multiple) {
                            element.bind('change', saveMultipleSelect2);
                        } else {
                            element.bind('change', saveSelect2);
                        }
                        // kendo date picker
                    } else if (!angular.isUndefined(attrs.kendoDatePicker)) {
                        element.bind('blur', saveDate);
                    } else if (attrs.type === 'checkbox') {
                        element.bind('change', saveCheckbox);
                    } else {
                        element.bind('blur', saveIfNew);
                    }
                }
            };
        }
    ]);
})(angular, app);
