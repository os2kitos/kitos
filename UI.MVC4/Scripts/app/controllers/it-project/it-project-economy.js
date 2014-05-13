(function(ng, app) {

    app.config(['$stateProvider', function($stateProvider) {

        $stateProvider.state('it-project.edit.economy', {
            url: '/economy',
            templateUrl: 'partials/it-project/tab-economy.html',
            controller: 'project.EditEconomyCtrl',
            resolve: {
                                
            }
        });
    }]);

    app.controller('project.EditEconomyCtrl',
    ['$rootScope', '$scope', 'itProject',
        function ($rootScope, $scope, itProject) {
            $scope.columns = _.range(6);

            $scope.businessExpenses = [];
            $scope.businessExpensesSumRow = createSimpleRow("Forretningsmæssige omkostninger");
            
            $scope.itExpenses = [];
            $scope.itExpensesSumRow = createSimpleRow("IT omkostninger");

            $scope.expensesSumRow = createSimpleRow("OMKOSTNINGER TOTAL");
            
            $scope.businessSavings = [];

            $scope.businessSavings = createSimpleRow("Forretningsmæssige omkostninger");

            $scope.itSavings = [];
            
            function createSimpleRow(label) {
                var columns = _.map(itProject.economyYears, function() {
                    return {
                        budget: 0,
                        rea: 0
                    };
                });

                return {
                    label: label,
                    columns: columns,
                    total: {
                        budget: 0,
                        rea: 0
                    }
                };
            }
            
            function addRow(field, label, array, update) {
                var budgetName = field + "Budget",
                    reaName = field + "Rea";

                var columns = _.map(itProject.economyYears, function(year) {
                    return {
                        updateUrl: "api/economyYear/" + year.id,
                        budget: year[budgetName],
                        rea: year[reaName]
                    };
                });

                var row = {
                    budgetName: budgetName,
                    reaName: reaName,
                    label: label,
                    columns: columns,
                    total: {
                        budget: 0,
                        rea: 0
                    }
                };

                row.update = function() {
                    return update(row);
                };

                row.update();

                array.push(row);
            }
            
            function addBusinessExpenses(field, label) {
                return addRow(field, label, $scope.businessExpenses, function(row) {
                    calcTotalColumn(row);
                    calcSumRow($scope.businessExpensesSumRow, $scope.businessExpenses);
                    calcSumRow($scope.expensesSumRow, [$scope.businessExpensesSumRow, $scope.itExpensesSumRow]);
                });
            }

            function addItExpenses(field, label) {
                return addRow(field, label, $scope.itExpenses, function(row) {
                    calcTotalColumn(row);
                    calcSumRow($scope.itExpensesSumRow, $scope.itExpenses);
                    calcSumRow($scope.expensesSumRow, [$scope.businessExpensesSumRow, $scope.itExpensesSumRow]);
                });
            }

            function addBusinessSavings(field, label) {
                return addRow(field, label, $scope.businessSavings);
            }

            function addItSavings(field, label) {
                return addRow(field, label, $scope.itSavings);
            }

            addBusinessExpenses("consultant", "Konsulentbistand");
            addBusinessExpenses("education", "Uddannelse (medarbejdere/borgere mv.)");
            addBusinessExpenses("otherBusinessExpenses", "Andre forretningsmæssige omkostninger");
            addBusinessExpenses("increasedBusinessExpenses", "Øgede forretningsmæssige omkostninger");

            addItExpenses("hardware", "Hardware");
            addItExpenses("software", "Software");
            addItExpenses("otherItExpenses", "Andre IT omkostninger");
            addItExpenses("increasedItExpenses", "Øgede IT omkostninger");

            
            //calculates the last column "total" for a given row
            function calcTotalColumn(row) {
                
                var sums = _.reduce(row.columns, function (memo, column) {
                    return {
                        budget: memo.budget + parseInt(column.budget),
                        rea: memo.rea + parseInt(column.rea)
                    };
                }, { budget: 0, rea: 0 });
                
                row.total = sums;
            }
            
            //sums the rows and stores the result in sumRow
            function calcSumRow(sumRow, rows) {
                
                //sum each of the year columns
                _.each(sumRow.columns, function(column, index) {
                    var sums = _.reduce(rows, function(memo, row) {
                        return {
                            budget: memo.budget + parseInt(row.columns[index].budget),
                            rea: memo.rea + parseInt(row.columns[index].rea)
                        };
                    }, { budget: 0, rea: 0 });

                    column.budget = sums.budget;
                    column.rea = sums.rea;
                });

                //sum the total column
                sumRow.total = _.reduce(rows, function(memo, row) {
                    return {
                        budget: memo.budget + row.total.budget,
                        rea: memo.rea + row.total.rea
                    };
                }, { budget: 0, rea: 0 });
            }
            
            
        }]);


})(angular, app);
    