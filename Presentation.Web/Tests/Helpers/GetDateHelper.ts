class GetDateHelper {

    public static getTodayAsString() {
        const currentDay = new Date().getDate();
        const currentMonth = new Date().getMonth() + 1; // getMonth gets counts 0. So January is 0.

        if (currentDay <= 9) {
            if (currentMonth <= 9) {
                return `0${currentDay}-0${currentMonth}-${new Date().getFullYear()}`;
            }
            return `0${currentDay}-${currentMonth}-${new Date().getFullYear()}`;
        }

        if (currentMonth <= 9) {
            return `${currentDay}-0${currentMonth}-${new Date().getFullYear()}`;
        }
        return `${currentDay}-${currentMonth}-${new Date().getFullYear()}`;

    }
}

export = GetDateHelper;