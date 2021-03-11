import { UserItemBaseDto } from './UserItemBaseDto';

export interface UserGetByIdResponse extends UserItemBaseDto {
	fullName: string | undefined;
	nameWithInitials: string | undefined;
}
