(function(ng, app) {
    'use strict';

    app.directive('selectUser', function() {

            //format of dropdown options
            function formatResult(obj) {
                var result = "<div>" + obj.text + "</div>";

                //obj.user might contain more info about the user
                if (obj.user) {
                    result += "<div class='small'>" + obj.user.Email;

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
                                transport: queryParams => {
                                    var res = userSearchParameters(queryParams.data.query).then(queryParams.success);
                                    res.abort = () => {
                                        return null;
                                    };
                                    return res;
                                },

                                results: (data, page) => {
                                    var results = [];

                                    _.each(data.data.value, user => {
                                        results.push({
                                            id: user.Id, //select2 mandatory
                                            text: user.Name + " " + user.LastName, //select2 mandatory
                                            user: user //not mandatory, for extra info when formatting
                                        });
                                    });

                                    return { results: results };
                                }
                            }
                        };

                        function userSearchParameters(queryParams) {
                            console.log(queryParams);
                            var userInputString1: string = "", userInputString2: string = "", userInputString3: string = "";
                            var userInputString = [userInputString1, userInputString2, userInputString3];
                            var userStrings = queryParams.split(' ', 3);
                            console.log(userStrings);
                            var index: number = 0;
                            for (let userString of userStrings) {
                                userInputString[index] = userString;
                                index++;
                            }
                            var result = $http.get(`/odata/Users?$filter=OrganizationRights/any(o:o/OrganizationId eq ${$scope.orgId}) and contains(concat(concat(concat(concat(Name, ' '), LastName), ' '), Email), '${userInputString[0]}') and contains(concat(concat(concat(concat(Name, ' '), LastName), ' '), Email), '${userInputString[1]}') and contains(concat(concat(concat(concat(Name, ' '), LastName), ' '), Email), '${userInputString[2]}')`, { ignoreLoadingBar: true })
                            console.log(result);
                            return result;
                        };
                    }
                ]
            };
        }
    );
})(angular, app);
