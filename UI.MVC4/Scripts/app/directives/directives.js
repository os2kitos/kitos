(function (ng, app) {
    app.directive('holderFix', function () {
        return {
            link: function (scope, element, attrs) {
                Holder.run({ images: element[0], nocss: true });
            }
        };
    });

    app.directive('confirmClick', [
        function () {
            return {
                link: function (scope, element, attr) {
                    var msg = attr.confirmClick || "Er du sikker?";
                    var clickAction = attr.confirmedClick;
                    element.bind('click', function (event) {
                        if (window.confirm(msg)) {
                            scope.$eval(clickAction);
                        }
                    });
                }
            };
        }]);

    app.directive('selectUser', ['$http', function ($http) {
        return {
            replace: true,
            templateUrl: 'partials/directives/select-user.html',
            link: function (scope, element, attr) {

                scope.selectUserOptions = {
                    minimumInputLength: 1,
                    initSelection: function (elem, callback) {
                    },
                    ajax: {
                        data: function (term, page) {
                            return { query: term };
                        },
                        quietMillis: 500,
                        transport: function (queryParams) {
                            //console.log(queryParams);
                            var res = $http.get('api/user?q=' + queryParams.data.query).then(queryParams.success);
                            res.abort = function () {
                                return null;
                            };

                            return res;
                        },
                        results: function (data, page) {
                            console.log(data);
                            var results = [];

                            _.each(data.data.Response, function (user) {
                                //Save to cache
                                if(scope.usersCache) scope.usersCache[user.Id] = user;

                                results.push({
                                    id: user.Id,
                                    text: user.Name
                                });
                            });

                            return { results: results };
                        }
                    }


                };

                console.log(scope);
            }
        };
    }

    ]);
})(angular, app);