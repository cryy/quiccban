import { IPartialDatabaseGuild } from "./IPartialDatabaseGuild";
import { ActionType } from "../enums/ActionType";
import { IUser } from "../user/IUser";

export interface ICase {
    id: number;
    discordMessageId: string;
    guild: IPartialDatabaseGuild;
    guildId: string;
    actionType: ActionType;
    actionExpiry: number;
    resolved: boolean;
    forceResolved: boolean;
    targetId: string;
    issuerId: string;
    targetUser?: IUser;
    issuerUser?: IUser;
    reason: string;
    unixTimestamp: string;
}
