import { IGuild } from "./IGuild";
import { IDiscordPermissions } from "../permissions/IDiscordPermissions";

export interface ISelfGuild extends IGuild {
    isOwner: boolean;
    permissions: IDiscordPermissions;
}