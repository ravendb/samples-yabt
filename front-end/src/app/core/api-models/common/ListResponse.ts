export class ListResponse<T> {
	// TotalRecords
	totalRecords: number = 0;
	// PageIndex
	pageIndex: number = 0;
	// TotalPages
	totalPages: number = 0;
	// Entries
	entries: T[];

	constructor() {
		this.entries = [];
	}
}
