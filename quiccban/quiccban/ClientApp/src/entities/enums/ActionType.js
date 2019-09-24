export var ActionType;
(function (ActionType) {
    ActionType[ActionType["Warn"] = 0] = "Warn";
    ActionType[ActionType["Kick"] = 1] = "Kick";
    ActionType[ActionType["TempMute"] = 2] = "TempMute";
    ActionType[ActionType["Mute"] = 3] = "Mute";
    ActionType[ActionType["TempBan"] = 4] = "TempBan";
    ActionType[ActionType["Ban"] = 5] = "Ban";
    ActionType[ActionType["HackBan"] = 6] = "HackBan";
    ActionType[ActionType["Unwarn"] = 7] = "Unwarn";
    ActionType[ActionType["Unmute"] = 8] = "Unmute";
    ActionType[ActionType["Unban"] = 9] = "Unban";
    ActionType[ActionType["None"] = 10] = "None";
})(ActionType || (ActionType = {}));
export function toPassive(action) {
    switch (action) {
        case ActionType.Warn:
            return "warned";
        case ActionType.Kick:
            return "kicked";
        case ActionType.TempMute:
            return "temporarily muted";
        case ActionType.Mute:
            return "muted";
        case ActionType.TempBan:
            return "temporarily banned";
        case ActionType.Ban:
            return "banned";
        case ActionType.HackBan:
            return "hackbanned";
        case ActionType.Unwarn:
            return "unwarned";
        case ActionType.Unmute:
            return "unmuted";
        case ActionType.Unban:
            return "unbanned";
    }
}
