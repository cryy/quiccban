import { DiscordPermission } from "./DiscordPermission";
import { IDiscordPermissions } from "./IDiscordPermissions";

export class DiscordPermissions implements IDiscordPermissions {

    rawValue: number;

    constructor(rawValue: number) {
        this.rawValue = rawValue;
    }
    
    public has(flag: DiscordPermission): boolean {
        return (this.rawValue & flag) === flag;
    }
}
