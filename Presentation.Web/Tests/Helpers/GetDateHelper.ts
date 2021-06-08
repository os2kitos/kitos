class GetDateHelper {

    private static getDataComponentAsString(dayOrMonth: number) {
        return (`0${dayOrMonth}`).slice(-2);
    }

    private static format(date: Date) {
        const currentDay = date.getDate();
        const currentMonth = date.getMonth() + 1; // getMonth gets counts 0. So January is 0.
        return `${this.getDataComponentAsString(currentDay)}-${this.getDataComponentAsString(currentMonth)}-${date.getFullYear()}`;
    }

    static getTodayAsString() {
        return GetDateHelper.format(new Date());
    }

    static getDateWithOffsetFromTodayAsString(offset : number) {
        const offsetDate = new Date();
        offsetDate.setDate(offsetDate.getDate() + offset);
        return GetDateHelper.format(offsetDate);
    }
}

export = GetDateHelper;