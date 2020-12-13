import { OrderDirections } from './OrderDirections';

export class ListRequest {
	orderBy: string = '';
	orderDirection: keyof typeof OrderDirections = OrderDirections.Asc;
	pageIndex: number = 0;
	pageSize: number = 0;

	constructor() {}
}
