import { UserGetByIdResponse } from './UserGetByIdResponse';

export interface CurrentUserResponse extends UserGetByIdResponse {
	id: string;
}
