module Kitos.Shared.Time {
    export enum TimeUnit {
        Seconds = "Seconds",
        Minutes = "Minutes",
        Hours = "Hours"
    }

    const timeUnitToMultiplier: Record<TimeUnit, number> = {
        Seconds: 1000,
        Minutes: 1000 * 60,
        Hours: 1000 * 60 * 60
    }

    export class Offset {
        /**
         * Computes an offset with an optional relative date
         * @param unit
         * @param value
         * @param relativeTo the time the offset will be relative to. If unspecified, the current time will be used
         */
        static compute(unit: TimeUnit, value: number, relativeTo?: Date): Date {
            const now = relativeTo ?? new Date(Date.now());
            const offset = value * timeUnitToMultiplier[unit];
            return new Date(now.getTime() + offset);
        }
    }
}