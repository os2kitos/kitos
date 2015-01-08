(function(ng, app) {

    app.config(['$stateProvider', function($stateProvider) {

        $stateProvider.state('it-project.edit.economy', {
            url: '/economy',
            templateUrl: 'partials/it-project/tab-economy.html',
            controller: 'project.EditEconomyCtrl',
            resolve: {
                // re-resolve data from parent cause changes here wont cascade to it
                project: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itproject/" + $stateParams.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                }]             
            }
        });
    }]);

    app.controller('project.EditEconomyCtrl',
    ['$rootScope', '$scope', 'project',
        function ($rootScope, $scope, project) {
            $scope.columns = _.range(6);

            var expensesTitleRow = createTitleRow("OMKOSTNINGER");

            var businessExpenses = [];
            var businessExpensesSumRow = createReadonlyRow("Forretningsmæssige omkostninger", "sub-sum");
            
            var itExpenses = [];
            var itExpensesSumRow = createReadonlyRow("IT omkostninger", "sub-sum");

            var expensesSumRow = createReadonlyRow("OMKOSTNINGER TOTAL", "sum");

            var savingsTitleRow = createTitleRow("BESPARELSER");
            
            var businessSavings = [];
            var businessSavingsSumRow = createReadonlyRow("Forretningsmæssige besparelser", "sub-sum");

            var itSavings = [];
            var itSavingsSumRow = createReadonlyRow("IT besparelser", "sub-sum");
            
            var savingsSumRow = createReadonlyRow("BESPARELSER TOTAL", "sum");

            var diffRow = createReadonlyRow("Finansieringsbehov / besparelse", "super-sum");
            
            function createTitleRow(label) {
                return {
                    label: label,
                    titlerow: true
                };
            }

            function createReadonlyRow(label, classes) {
                var columns = _.map(project.economyYears, function() {
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
                    },
                    readonly: true,
                    classes: classes
                };
            }
            
            function addRow(field, label, array, update) {
                var budgetName = field + "Budget",
                    reaName = field + "Rea";

                var columns = _.map(project.economyYears, function(year) {
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
                array.push(row);
                row.update();
            }
            
            function addBusinessExpenses(field, label) {
                return addRow(field, label, businessExpenses, function (row) {
                    calcTotalColumn(row);
                    sumRows(businessExpenses, businessExpensesSumRow);
                    sumRows([businessExpensesSumRow, itExpensesSumRow], expensesSumRow);
                    subtractRows(savingsSumRow, expensesSumRow, diffRow);
                });
            }

            function addItExpenses(field, label) {
                return addRow(field, label, itExpenses, function (row) {
                    calcTotalColumn(row);
                    sumRows(itExpenses, itExpensesSumRow);
                    sumRows([businessExpensesSumRow, itExpensesSumRow], expensesSumRow);
                    subtractRows(savingsSumRow, expensesSumRow, diffRow);
                });
            }

            function addBusinessSavings(field, label) {
                return addRow(field, label, businessSavings, function (row) {
                    calcTotalColumn(row);
                    sumRows(businessSavings, businessSavingsSumRow);
                    sumRows([businessSavingsSumRow, itSavingsSumRow], savingsSumRow);
                    subtractRows(savingsSumRow, expensesSumRow, diffRow);
                });
            }

            function addItSavings(field, label) {
                return addRow(field, label, itSavings, function (row) {
                    calcTotalColumn(row);
                    sumRows(itSavings, itSavingsSumRow);
                    sumRows([businessSavingsSumRow, itSavingsSumRow], savingsSumRow);
                    subtractRows(savingsSumRow, expensesSumRow, diffRow);
                });
            }

            addBusinessExpenses("consultant", "Konsulentbistand");
            addBusinessExpenses("education", "Uddannelse (medarbejdere/borgere mv.)");
            addBusinessExpenses("otherBusinessExpenses", "Andre forretningsmæssige omkostninger");
            addBusinessExpenses("increasedBusinessExpenses", "Øgede forretningsmæssige omkostninger");

            addItExpenses("hardware", "Hardware");
            addItExpenses("software", "Software");
            addItExpenses("otherItExpenses", "Andre IT omkostninger");
            addItExpenses("increasedItExpenses", "Øgede IT omkostninger");

            addBusinessSavings("salary", "Lønbesparelser");
            addBusinessSavings("otherBusinessSavings", "Andre forretningsmæssige besparelser");

            addItSavings("licenseSavings", "Besparelser på licenser");
            addItSavings("systemMaintenanceSavings", "Besparelser på systemvedligehold");
            addItSavings("otherItSavings", "Andre IT driftsbesparelser");

            $scope.rows = [].concat(
                [expensesTitleRow],
                businessExpenses, [businessExpensesSumRow],
                itExpenses, [itExpensesSumRow],
                [expensesSumRow],
                [savingsTitleRow],
                businessSavings, [businessSavingsSumRow],
                itSavings, [itSavingsSumRow],
                [savingsSumRow],
                [diffRow]);

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
            function sumRows(rows, resultRow) {
                
                //sum each of the year columns
                _.each(resultRow.columns, function(column, index) {
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
                resultRow.total = _.reduce(rows, function(memo, row) {
                    return {
                        budget: memo.budget + row.total.budget,
                        rea: memo.rea + row.total.rea
                    };
                }, { budget: 0, rea: 0 });
            }
            
            function subtractRows(rowA, rowB, resultRow) {
                //subtract each of the year columns
                _.each(resultRow.columns, function(column, index) {
                    column.budget = rowA.columns[index].budget - rowB.columns[index].budget;
                    column.rea = rowA.columns[index].rea - rowB.columns[index].rea;
                });

                //sum the total column
                resultRow.total = {
                    budget: rowA.total.budget - rowB.total.budget,
                    rea: rowA.total.rea - rowB.total.rea
                };
            }
            
            
        }]);


})(angular, app);
    