import { OrderDirections } from './OrderDirections';

export class ListRequest {
	orderBy: string = '';
	orderDirection: keyof typeof OrderDirections = OrderDirections.asc;
	pageIndex: number = 0;
	pageSize: number = 0;

	constructor() {}
}
