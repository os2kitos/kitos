module Kitos.Helpers {
    export class ConnectionChangeLogHelper {
        static createDictionaryFromChangeLogList(changeLogs: Array<Models.Api.Organization.ConnectionChangeLogDTO>) {
            return changeLogs.reduce((acc, next, _) => {
                acc[next.id] = {
                    id: next.id,
                    text: ConnectionChangeLogHelper.getDropdownTextBasedOnOrigin(next),
                    optionalObjectContext: next
                };
                return acc;
            }, {});
        }

        static getDropdownTextBasedOnOrigin(changeLog: Models.Api.Organization.ConnectionChangeLogDTO): string {
            return changeLog.origin === Models.Api.Organization.ConnectionChangeLogOrigin.Background ? "FK Organisation" : `${changeLog.user.name} (${changeLog.user.email})`
        }
    }
}