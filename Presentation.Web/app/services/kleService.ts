module Kitos.Services {
    "use strict";

    interface IKLEStatus
    {
        UpdateReady: boolean;
        VersionNumber: string;
    }

    interface IKLEChange
    {
        KLENumber: string;
        Change: string;
    }
        
    interface IKLEChanges
    {
        Changes: Array<IKLEChange>;
    }

    interface IKLEUpdateStatus
    {
        Success: boolean;
    }

    export class KLEservice
    {
        public static $inject: string[] = ["$http"];

        constructor(private $http: IHttpServiceWithCustomConfig) {
        }
        
        GetStatus = () => {
            return this.$http.get<IKLEStatus>(`api/kle`);
        }

        GetChanges = () => {
            //return this.$http.get<IKLEChanges>(`api/KLEchanges`);
            return null;
        }

        UpdateKLE = () => {
            //return this.$http.get<IKLEUpdateStatus>(`api/KleUpdate`);
            return null;
        }

    }
    app.service("KLEservice", KLEservice);
}