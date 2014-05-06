(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('edit-it-project.risk', {
            url: '/risk',
            templateUrl: 'partials/it-project/tab-risk.html',
            controller: 'project.EditRiskCtrl',
            resolve: {
                risks: ['$http', 'itProject', function ($http, itProject) {
                        return $http.get('api/risk/?getByProject&projectId=' + itProject.id)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }]
            }
        });
    }]);

    app.controller('project.EditRiskCtrl', ['$scope', '$http', '$stateParams', 'notify', 'risks',
        function($scope, $http, $stateParams, notify, risks) {

            _.each(risks, function(risk) {
                risk.show = true;
                risk.userForSelect = {
                    id: risk.responsibleUserId,
                    text: risk.responsibleUser.name
                };
            });

            $scope.risks = risks;

            function resetNewRisk() {
                $scope.newRisk = {
                    consequence: 1,
                    probability: 1
                };
            }

            resetNewRisk();

            $scope.product = function (risk) {

                return risk.consequence * risk.probability;
            };

            $scope.save = function(risk) {
                 
                if (!risk.name || !risk.action) {
                    
                }

            };

        }
    ]);
})(angular, app);