export abstract class UserItemBaseDto {
	firstName: string | undefined;
	lastName: string | undefined;
	avatarUrl: string | undefined;
	email!: string;
}
