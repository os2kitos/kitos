interface IPageObject {
    getPage(): webdriver.promise.Promise<void>;
}

export = IPageObject;
