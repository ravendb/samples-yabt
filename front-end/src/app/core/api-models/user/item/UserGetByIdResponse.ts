import { UserItemBaseDto } from './UserItemBaseDto';

export class UserGetByIdResponse extends UserItemBaseDto {
	fullName: string | undefined;
	nameWithInitials: string | undefined;
}
