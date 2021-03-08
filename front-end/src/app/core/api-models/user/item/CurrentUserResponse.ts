import { UserGetByIdResponse } from './UserGetByIdResponse';

export class CurrentUserResponse extends UserGetByIdResponse {
	id!: string;
}
