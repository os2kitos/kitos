class GetDateHelper {

    private static getDataComponentAsString(dayOrMonth: number) {
        return (`0${dayOrMonth}`).slice(-2);
    }

    static getTodayAsString() {
        const now = new Date();
        const currentDay = now.getDate();
        const currentMonth = now.getMonth() + 1; // getMonth gets counts 0. So January is 0.
        return `${this.getDataComponentAsString(currentDay)}-${this.getDataComponentAsString(currentMonth)}-${now.getFullYear()}`;

    }
}

export = GetDateHelper;