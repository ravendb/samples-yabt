import { Injectable } from '@angular/core';

abstract class BaseStorageService {
	protected constructor(protected storage: Storage) {}

	public setItem(key: string, value: string): void {
		this.storage.setItem(key, value);
	}

	public getItem(key: string): string | null {
		return this.storage.getItem(key);
	}

	public removeItem(key: string): void {
		this.storage.removeItem(key);
	}

	public clear(): void {
		this.storage.clear();
	}
}

@Injectable({
	providedIn: 'root',
})
export class LocalStorageService extends BaseStorageService {
	constructor() {
		super(localStorage);
	}
}

@Injectable({
	providedIn: 'root',
})
export class SessionStorageService extends BaseStorageService {
	constructor() {
		super(sessionStorage);
	}
}
