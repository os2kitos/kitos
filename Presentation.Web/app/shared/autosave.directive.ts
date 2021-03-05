(function (ng, app) {
    'use strict';

    app.directive('autosave', [
        '$http', '$timeout', 'notify', 'userService', 'moment', function ($http, $timeout, notify, userService, moment) {
            return {
                restrict: 'A',
                require: 'ngModel',
                priority: 0,
                link: function (scope, element, attrs, ctrl) {
                    var user;

                    userService.getUser().then(function (result) {
                        user = result;
                    });

                    var oldValue;
                    $timeout(function () {
                        oldValue = ctrl.$modelValue; // get initial value
                    });

                    function saveIfNew() {
                        var newValue = ctrl.$modelValue;
                        if (attrs.pluck)
                            newValue = _.map(newValue, attrs.pluck);

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
                            newValue = _.map(newValue, attrs.pluck);

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
                        $timeout(function () {
                            if (attrs.globalOptionId) {
                                saveLocalOption();
                            } else {
                                var newValue = ctrl.$modelValue
                                var payload = {};
                                payload[attrs.field] = newValue;
                                save(payload);
                            }
                        });
                    }

                    function saveLocalOption() {
                        var globalOptionId = parseInt(attrs.globalOptionId, 10);
                        var isActive = ctrl.$modelValue;
                        var payload = {};
                        payload[attrs.field] = globalOptionId;
                        var msg = notify.addInfoMessage("Gemmer...", false);

                        if (isActive) {
                            $http({ method: "POST", url: `${attrs.autosave}?organizationId=${user.currentOrganizationId}`, data: payload, ignoreLoadingBar: true })
                                .then((successResponse) => {
                                    msg.toSuccessMessage("Feltet er opdateret.");
                                    oldValue = ctrl.$modelValue;
                                }, (errorResponse) => {
                                    if (errorResponse.status === 409) {
                                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres da værdien den allerede findes i KITOS!");
                                    } else {
                                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                    }
                                });
                        } else {
                            $http({ method: "DELETE", url: `${attrs.autosave}(${globalOptionId})?organizationId=${user.currentOrganizationId}`, ignoreLoadingBar: true })
                                .then((successResponse) => {
                                    msg.toSuccessMessage("Feltet er opdateret.");
                                    oldValue = ctrl.$modelValue;
                                }, (errorResponse) => {
                                    if (errorResponse.status === 409) {
                                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres da værdien den allerede findes i KITOS!");
                                    } else {
                                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                    }
                                });
                        }
                    }

                    function saveTypeahead() {
                        $timeout(function () {
                            var payload = {};
                            payload[attrs.field] = ctrl.$modelValue;

                            save(payload);
                        }, 200);
                    }

                    function saveSelect2() {

                        // ctrl.$viewValue reflects the old state.
                        // using timeout to wait for the value to update
                        $timeout(function () {
                            if (attrs.field) {
                                if (angular.isUndefined(user)) {
                                    notify.addWarnMessage("Brugeren er endnu ikke indlæst. Vent venligst og prøv igen.", true);
                                    return;
                                }

                                var newValue;

                                var viewValue = ctrl.$viewValue;
                                if (angular.isArray(viewValue)) {
                                    newValue = _.map(viewValue, 'id');
                                } else if (angular.isObject(viewValue)) {
                                    newValue = viewValue.id;
                                } else {
                                    newValue = viewValue;
                                }

                                var payload = {};
                                payload[attrs.field] = newValue;

                                save(payload);
                            }
                        });
                    }

                    function saveMultipleSelect2(e) {
                        var id, msg = notify.addInfoMessage("Gemmer...", false);
                        if (e.added) {
                            id = e.added.id;
                            $http.post(attrs.autosave + '?organizationId=' + user.currentOrganizationId + '&' + attrs.field + '=' + id)
                                .then(function onSuccess(result) {
                                    msg.toSuccessMessage("Feltet er opdateret.");
                                }, function onError(result) {
                                    msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                });
                        } else if (e.removed) {
                            id = e.removed.id;
                            $http.delete(attrs.autosave + '?organizationId=' + user.currentOrganizationId + '&' + attrs.field + '=' + id)
                                .then(function onSuccess(result) {
                                    msg.toSuccessMessage("Feltet er opdateret.");
                                }, function onError(result) {
                                    msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                });
                        }
                    }

                    function save(payload) {
                        var msg = notify.addInfoMessage("Gemmer...", false);
                        if (!attrs.appendurl)
                            attrs.appendurl = '';

                        $http({ method: 'PATCH', url: attrs.autosave + '?organizationId=' + user.currentOrganizationId + attrs.appendurl, data: payload, ignoreLoadingBar: true })
                            .then(function onSuccess(result) {
                                msg.toSuccessMessage("Feltet er opdateret.");
                                oldValue = ctrl.$modelValue;
                            }, function onError(result) {
                                if (result.status === 409) {
                                    msg.toErrorMessage("Fejl! Feltet kunne ikke ændres da værdien den allerede findes i KITOS!");
                                } else {
                                    msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                }
                            });
                    }

                    // ui select fields trigger the change event
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
                    } else if (!angular.isUndefined(attrs.autosaveTypeahead)) {
                        element.bind('blur', saveTypeahead);
                    } else {
                        element.bind('blur', saveIfNew);
                    }
                }
            };
        }
    ]);
})(angular, app);
