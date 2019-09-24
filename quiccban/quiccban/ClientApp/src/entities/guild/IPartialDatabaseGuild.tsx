import { LogStyle } from "../enums/LogStyle";
import { ActionType } from "../enums/ActionType";
import { IAutoMod } from "./IAutoMod";

export interface IPartialDatabaseGuild {
    id: string;
    muteRoleId: string;
    modlogChannelId: string;
    logStyle: LogStyle;
    warnThreshold: number;
    warnExpiry: number;
    warnThresholdActionType: ActionType;
    warnThresholdActionExpiry: number;
    autoMod: IAutoMod;
}