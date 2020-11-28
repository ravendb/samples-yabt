import { first } from 'lodash-es';

export class MainMenuMapItem {
	get mainLink(): string {
		return !!this.highlightedLinks ? first(this.highlightedLinks)! : this.uriStartsWith;
	}

	constructor(public title: string, public uriStartsWith: string, public highlightedLinks: string[], public icon: string) {}
}
