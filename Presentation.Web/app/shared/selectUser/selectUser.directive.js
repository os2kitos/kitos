(function(ng, app) {
    'use strict';

    app.directive('selectUser', function() {

            //format of dropdown options
            function formatResult(obj) {
                var result = "<div>" + obj.text + "</div>";

                //obj.user might contain more info about the user
                if (obj.user) {
                    result += "<div class='small'>" + obj.user.email;

                    if (obj.user.defaultOrganizationUnitName)
                        result += ", " + obj.user.defaultOrganizationUnitName;

                    result += "</div>";
                }

                return result;
            }

            //format of the selected user
            function formatSelection(obj) {
                return obj.text;
            }

            return {
                scope: {
                    inputName: '@?name',
                    userModel: '=',
                    addUser: '@?',
                    allowClear: '@?',
                    onSelect: '&?',
                    ngDisabled: '=?',
                    orgId: '@?'
                },
                replace: true,
                templateUrl: 'app/shared/selectUser/selectUser.view.html',
                controller: [
                    '$scope', '$http', '$timeout', function($scope, $http, $timeout) {
                        $scope.onChange = function() {

                            //for some reason (probably a bugger in select2)
                            //this is called 2 times, once with the original select value (like 1 or "")
                            //and once with the object value of select2 {id, text}.
                            //we only need the last one
                            if (typeof $scope.userModel !== 'object') return;

                            //timeout, otherwise we get the bad version of the model.
                            $timeout($scope.onSelect);
                        };

                        var userSrc = typeof $scope.orgId !== 'undefined' ? 'api/organization/' + $scope.orgId + '?users=true&q=' : 'api/user?q=';

                        $scope.selectUserOptions = {
                            //don't escape markup, otherwise formatResult will be bugged
                            escapeMarkup: function(m) { return m; },
                            formatResult: formatResult,
                            formatSelection: formatSelection,

                            allowClear: !!$scope.allowClear,

                            minimumInputLength: 1,
                            initSelection: function(elem, callback) {
                            },
                            ajax: {
                                data: function(term, page) {
                                    return { query: term };
                                },
                                quietMillis: 500,
                                transport: function(queryParams) {
                                    var res = $http.get(userSrc + queryParams.data.query, { ignoreLoadingBar: true }).then(queryParams.success);
                                    res.abort = function() {
                                        return null;
                                    };

                                    return res;
                                },

                                results: function(data, page) {
                                    var results = [];

                                    _.each(data.data.response, function(user) {

                                        results.push({
                                            id: user.id, //select2 mandatory
                                            text: user.fullName, //select2 mandatory
                                            user: user //not mandatory, for extra info when formatting
                                        });
                                    });

                                    return { results: results };
                                }
                            }
                        };
                    }
                ]
            };
        }
    );
})(angular, app);
