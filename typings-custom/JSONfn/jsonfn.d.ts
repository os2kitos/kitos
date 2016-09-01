declare var JSONfn: JSONfn.JSONfnStatic;

declare module JSONfn {
    interface JSONfnStatic {
        stringify(obj: Object): string;
        parse(str: string|JSON|any, date2obj?): Object;
        clone(obj: Object, date2obj?);
    }
}
