export interface IUser {
    id: string;
    username: string;
    discriminator: number;
    avatarId: string;
    avatarUrl: string;
    isBotOwner: boolean;
    createdAt: Date;
}