export interface IUser {
    id: string;
    username: string;
    discriminator: string;
    avatarId: string;
    avatarUrl: string;
    isBotOwner: boolean;
    createdAt: Date;
}