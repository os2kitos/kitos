class navigationBarWrapper {

    public mainNavigationElement = element(by.id("navbar-top"));

    public navBarkitosLogo = this.mainNavigationElement.element(by.css("a.navbar-brand"));

    public navBarOrg = this.mainNavigationElement.element(by.css("ul.nav.navbar-nav")).all(by.css("a.ng-scope")).get(0);

    public navBarProj = this.mainNavigationElement.element(by.css("ul.nav.navbar-nav")).all(by.css("a.ng-scope")).get(1);

    public navBarSys = this.mainNavigationElement.element(by.css("ul.nav.navbar-nav")).all(by.css("a.ng-scope")).get(2);

    public navBarCon = this.mainNavigationElement.element(by.css("ul.nav.navbar-nav")).all(by.css("a.ng-scope")).get(3);

    public navBarRep = this.mainNavigationElement.element(by.css("ul.nav.navbar-nav")).all(by.css("a.ng-scope")).get(4);




}

export = navigationBarWrapper;