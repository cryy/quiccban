import { IUser } from "./IUser";
import { ISelfGuild } from "../guild/ISelfGuild";

export interface ISelfUser {
    user: IUser;
    guilds: ISelfGuild[];
}