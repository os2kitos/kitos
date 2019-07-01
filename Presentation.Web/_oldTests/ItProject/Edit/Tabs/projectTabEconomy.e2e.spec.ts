import mock = require("protractor-http-mock");
import Helper = require("../../../helper");
import PageObject = require("../../../../app/components/it-project/tabs/it-project-tab-economy.po");

describe("project edit tab economy", () => {
    var mockHelper: Helper.Mock;
    var pageObject: PageObject;
    var mockDependencies: Array<string> = ["itproject", "itprojecttype", "itprojectstatus", "economyYear"];

    beforeEach(() => {
        browser.driver.manage().window().maximize();

        mockHelper = new Helper.Mock();
        pageObject = new PageObject();
    });

    afterEach(() => {
        mock.teardown();
    });

    describe("with no write access", () => {
        beforeEach(done => {
            mock(["itProjectNoWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        it("should disable budget inputs", () => {
            // arrange
            var inputs = element.all(pageObject.budgetLocator);

            // act

            // assert
            inputs.each(element => {
                expect(element).toBeDisabled();
            });
        });

        it("should disable rea inputs", () => {
            // arrange
            var inputs = element.all(pageObject.reaLocator);

            // act

            // assert
            inputs.each(element => {
                expect(element).toBeDisabled();
            });
        });
    });

    describe("with write access", () => {
        beforeEach(done => {
            mock(["itProjectWriteAccess"].concat(mockDependencies));
            pageObject.getPage()
                .then(() => mock.clearRequests())
                .then(() => done());
        });

        describe("on budget inputs", () => {
            it("should calculate row sum for inputs in first row", () => {
                // arrange
                var rowIndex = 1;
                var yearCols = pageObject.rowRepeater.select(rowIndex, pageObject.budgetLocator);

                // act
                yearCols.sendKeys("1", protractor.Key.TAB);

                // assert
                var totalBudget = pageObject.rowRepeater
                    .select(rowIndex, pageObject.totalBudgetLocator)
                    .first()
                    .getAttribute("value")
                    .then(v => parseInt(v));

                expect(totalBudget).toBe(yearCols.count());
            });

            it("should calculate sub sum for rows 1 to 4", () => {
                // arrange
                var rows = { first: 1, last: 4 };
                var subSumRowIndex = 5;
                var expectedValue = rows.last - rows.first + 1;

                // act
                pageObject.rowRepeater.each((element, index) => {
                    if (index >= rows.first && index <= rows.last) {
                        element.all(pageObject.budgetLocator)
                            .first()
                            .sendKeys("1", protractor.Key.TAB);
                    }
                });

                // assert
                var subsum = pageObject.rowRepeater
                    .select(subSumRowIndex, pageObject.sumBudgetLocator)
                    .first()
                    .getAttribute("value")
                    .then(v => parseInt(v));

                expect(subsum).toBe(expectedValue);
            });

            it("should calculate sub sum for rows 6 to 9", () => {
                // arrange
                var rows = { first: 6, last: 9 };
                var subSumRowIndex = 10;
                var expectedValue = rows.last - rows.first + 1;

                // act
                pageObject.rowRepeater.each((element, index) => {
                    if (index >= rows.first && index <= rows.last) {
                        element.all(pageObject.budgetLocator)
                            .first()
                            .sendKeys("1", protractor.Key.TAB);
                    }
                });

                // assert
                var subsum = pageObject.rowRepeater
                    .select(subSumRowIndex, pageObject.sumBudgetLocator)
                    .first()
                    .getAttribute("value")
                    .then(v => parseInt(v));

                expect(subsum).toBe(expectedValue);
            });

            it("should calculate sub sum for rows 13 to 14", () => {
                // arrange
                var rows = { first: 13, last: 14 };
                var subSumRowIndex = 15;
                var expectedValue = rows.last - rows.first + 1;

                // act
                pageObject.rowRepeater.each((row, index) => {
                    if (index >= rows.first && index <= rows.last) {
                        row.all(pageObject.budgetLocator)
                            .first()
                            .sendKeys("1", protractor.Key.TAB);
                    }
                });

                // assert
                var subsum = pageObject.rowRepeater
                    .select(subSumRowIndex, pageObject.sumBudgetLocator)
                    .first()
                    .getAttribute("value")
                    .then(v => parseInt(v));

                expect(subsum).toBe(expectedValue);
            });

            it("should calculate sub sum for rows 16 to 18", () => {
                // arrange
                var rows = { first: 16, last: 18 };
                var subSumRowIndex = 19;
                var expectedValue = rows.last - rows.first + 1;

                // act
                pageObject.rowRepeater.each((element, index) => {
                    if (index >= rows.first && index <= rows.last) {
                        element.all(pageObject.budgetLocator)
                            .first()
                            .sendKeys("1", protractor.Key.TAB);
                    }
                });

                // assert
                var subsum = pageObject.rowRepeater
                    .select(subSumRowIndex, pageObject.sumBudgetLocator)
                    .first()
                    .getAttribute("value")
                    .then(v => parseInt(v));

                expect(subsum).toBe(expectedValue);
            });

            it("should calculate sum for rows 1-4 and 6-9", () => {
                // arrange
                var rows = [{ first: 1, last: 4 }, { first: 6, last: 9 }];
                var sumRowIndex = 11;
                var expectedValue = 8;

                // act
                rows.forEach(row => {
                    pageObject.rowRepeater.each((element, index) => {
                        if (index >= row.first && index <= row.last) {
                            element.all(pageObject.budgetLocator)
                                .first()
                                .sendKeys("1", protractor.Key.TAB);
                        }
                    });
                });

                // assert
                var subsum = pageObject.rowRepeater
                    .select(sumRowIndex, pageObject.sumBudgetLocator)
                    .first()
                    .getAttribute("value")
                    .then(v => parseInt(v));

                expect(subsum).toBe(expectedValue);
            });

            it("should calculate sum for rows 13-14 and 16-18", () => {
                // arrange
                var rows = [{ first: 13, last: 14 }, { first: 16, last: 18 }];
                var sumRowIndex = 20;
                var expectedValue = 5;

                // act
                rows.forEach(row => {
                    pageObject.rowRepeater.each((element, index) => {
                        if (index >= row.first && index <= row.last) {
                            element.all(pageObject.budgetLocator)
                                .first()
                                .sendKeys("1", protractor.Key.TAB);
                        }
                    });
                });

                // assert
                var subsum = pageObject.rowRepeater
                    .select(sumRowIndex, pageObject.sumBudgetLocator)
                    .first()
                    .getAttribute("value")
                    .then(v => parseInt(v));

                expect(subsum).toBe(expectedValue);
            });

            it("should calculate super sum for all rows", () => {
                // arrange
                var rows = [{ first: 1, last: 4 }, { first: 6, last: 9 }, { first: 13, last: 14 }, { first: 16, last: 18 }];
                var sumRowIndex = 21;
                var expectedValue = -3;

                // act
                rows.forEach(row => {
                    pageObject.rowRepeater.each((element, index) => {
                        if (index >= row.first && index <= row.last) {
                            element.all(pageObject.budgetLocator)
                                .first()
                                .sendKeys("1", protractor.Key.TAB);
                        }
                    });
                });

                // assert
                var subsum = pageObject.rowRepeater
                    .select(sumRowIndex, pageObject.sumBudgetLocator)
                    .first()
                    .getAttribute("value")
                    .then(v => parseInt(v));

                expect(subsum).toBe(expectedValue);
            });

            it("should save when input looses focus", () => {
                // arrange
                var rowIndex = 1;
                var yearCol = pageObject.rowRepeater.select(rowIndex, pageObject.reaLocator).first();

                // act
                yearCol.sendKeys("1", protractor.Key.TAB);

                // assert
                expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyYear" });
            });
        });

        describe("on rea inputs", () => {
            it("should calculate row sum for inputs in first row", () => {
                // arrange
                var rowIndex = 1;
                var yearCols = pageObject.rowRepeater.select(rowIndex, pageObject.reaLocator);

                // act
                yearCols.sendKeys("1", protractor.Key.TAB);

                // assert
                var totalReaLocator = pageObject.rowRepeater
                    .select(rowIndex, pageObject.totalReaLocator)
                    .first()
                    .getAttribute("value")
                    .then(v => parseInt(v));

                expect(totalReaLocator).toBe(yearCols.count());
            });

            it("should calculate sub sum for rows 1 to 4", () => {
                // arrange
                var rows = { first: 1, last: 4 };
                var subSumRowIndex = 5;
                var expectedValue = rows.last - rows.first + 1;

                // act
                pageObject.rowRepeater.each((element, index) => {
                    if (index >= rows.first && index <= rows.last) {
                        element.all(pageObject.reaLocator)
                            .first()
                            .sendKeys("1", protractor.Key.TAB);
                    }
                });

                // assert
                var subsum = pageObject.rowRepeater
                    .select(subSumRowIndex, pageObject.sumReaLocator)
                    .first()
                    .getAttribute("value")
                    .then(v => parseInt(v));

                expect(subsum).toBe(expectedValue);
            });

            it("should calculate sub sum for rows 6 to 9", () => {
                // arrange
                var rows = { first: 6, last: 9 };
                var subSumRowIndex = 10;
                var expectedValue = rows.last - rows.first + 1;

                // act
                pageObject.rowRepeater.each((element, index) => {
                    if (index >= rows.first && index <= rows.last) {
                        element.all(pageObject.reaLocator)
                            .first()
                            .sendKeys("1", protractor.Key.TAB);
                    }
                });

                // assert
                var subsum = pageObject.rowRepeater
                    .select(subSumRowIndex, pageObject.sumReaLocator)
                    .first()
                    .getAttribute("value")
                    .then(v => parseInt(v));

                expect(subsum).toBe(expectedValue);
            });

            it("should calculate sub sum for rows 13 to 14", () => {
                // arrange
                var rows = { first: 13, last: 14 };
                var subSumRowIndex = 15;
                var expectedValue = rows.last - rows.first + 1;

                // act
                pageObject.rowRepeater.each((element, index) => {
                    if (index >= rows.first && index <= rows.last) {
                        element.all(pageObject.reaLocator)
                            .first()
                            .sendKeys("1", protractor.Key.TAB);
                    }
                });

                // assert
                var subsum = pageObject.rowRepeater
                    .select(subSumRowIndex, pageObject.sumReaLocator)
                    .first()
                    .getAttribute("value")
                    .then(v => parseInt(v));

                expect(subsum).toBe(expectedValue);
            });

            it("should calculate sub sum for rows 16 to 18", () => {
                // arrange
                var rows = { first: 16, last: 18 };
                var subSumRowIndex = 19;
                var expectedValue = rows.last - rows.first + 1;

                // act
                pageObject.rowRepeater.each((element, index) => {
                    if (index >= rows.first && index <= rows.last) {
                        element.all(pageObject.reaLocator)
                            .first()
                            .sendKeys("1", protractor.Key.TAB);
                    }
                });

                // assert
                var subsum = pageObject.rowRepeater
                    .select(subSumRowIndex, pageObject.sumReaLocator)
                    .first()
                    .getAttribute("value")
                    .then(v => parseInt(v));

                expect(subsum).toBe(expectedValue);
            });

            it("should calculate sum for rows 1-4 and 6-9", () => {
                // arrange
                var rows = [{ first: 1, last: 4 }, { first: 6, last: 9 }];
                var sumRowIndex = 11;
                var expectedValue = 8;

                // act
                rows.forEach(row => {
                    pageObject.rowRepeater.each((element, index) => {
                        if (index >= row.first && index <= row.last) {
                            element.all(pageObject.reaLocator)
                                .first()
                                .sendKeys("1", protractor.Key.TAB);
                        }
                    });
                });

                // assert
                var subsum = pageObject.rowRepeater
                    .select(sumRowIndex, pageObject.sumReaLocator)
                    .first()
                    .getAttribute("value")
                    .then(v => parseInt(v));

                expect(subsum).toBe(expectedValue);
            });

            it("should calculate sum for rows 13-14 and 16-18", () => {
                // arrange
                var rows = [{ first: 13, last: 14 }, { first: 16, last: 18 }];
                var sumRowIndex = 20;
                var expectedValue = 5;

                // act
                rows.forEach(row => {
                    pageObject.rowRepeater.each((element, index) => {
                        if (index >= row.first && index <= row.last) {
                            element.all(pageObject.reaLocator)
                                .first()
                                .sendKeys("1", protractor.Key.TAB);
                        }
                    });
                });

                // assert
                var subsum = pageObject.rowRepeater
                    .select(sumRowIndex, pageObject.sumReaLocator)
                    .first()
                    .getAttribute("value")
                    .then(v => parseInt(v));

                expect(subsum).toBe(expectedValue);
            });

            it("should calculate super sum for all rows", () => {
                // arrange
                var rows = [{ first: 1, last: 4 }, { first: 6, last: 9 }, { first: 13, last: 14 }, { first: 16, last: 18 }];
                var sumRowIndex = 21;
                var expectedValue = -3;

                // act
                rows.forEach(row => {
                    pageObject.rowRepeater.each((element, index) => {
                        if (index >= row.first && index <= row.last) {
                            element.all(pageObject.reaLocator)
                                .first()
                                .sendKeys("1", protractor.Key.TAB);
                        }
                    });
                });

                // assert
                var subsum = pageObject.rowRepeater
                    .select(sumRowIndex, pageObject.sumReaLocator)
                    .first()
                    .getAttribute("value")
                    .then(v => parseInt(v));

                expect(subsum).toBe(expectedValue);
            });

            it("should save when input looses focus", () => {
                // arrange
                var rowIndex = 1;
                var yearCol = pageObject.rowRepeater.select(rowIndex, pageObject.reaLocator).first();

                // act
                yearCol.sendKeys("1", protractor.Key.TAB);

                // assert
                expect(mockHelper.lastRequest()).toMatchRequest({ method: "PATCH", url: "api/economyYear" });
            });
        });
    });
});
