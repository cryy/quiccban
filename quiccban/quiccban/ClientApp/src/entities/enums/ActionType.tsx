export enum ActionType {
    Warn,
    Kick,
    TempMute,
    Mute,
    TempBan,
    Ban,
    HackBan,
    Unwarn,
    Unmute,
    Unban,
    None
}

export function toPassive(action: ActionType): string {
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
        default:
            return null;
    }
}