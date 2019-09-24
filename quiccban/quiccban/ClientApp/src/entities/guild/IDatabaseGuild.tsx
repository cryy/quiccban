import { IPartialDatabaseGuild } from "./IPartialDatabaseGuild";
import { ICase } from "./ICase";

export interface IDatabaseGuild extends IPartialDatabaseGuild {
    cases: ICase[];
}