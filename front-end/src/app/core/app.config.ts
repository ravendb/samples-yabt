export const AppConfig = {
	get PageSizeOptions(): number[] {
		return [10, 15, 20, 30, 50, 100];
	},

	get PageSize(): number {
		return this.PageSizeOptions[1];
	},

	get AppServerUrl(): string {
		const injectedVar: string = '#{AppServerUrl}';
		return injectedVar.lastIndexOf('#', 0) === 0 ? 'https://localhost:5001' : injectedVar;
	},

	get ApiKey(): string {
		const injectedVar: string = '#{ApiKey}';
		return injectedVar.lastIndexOf('#', 0) === 0 ? '4D84AE02-C989-4DC5-9518-8D0CB2FB5F61' : injectedVar;
	},
};
