import { NgModule } from '@angular/core';
import { Data, RouterModule, Routes } from '@angular/router';
import { CustomFieldListComponent } from './custom-field/list';
import { SystemComponent } from './system.component';

const dataWithTitle = (pageName: string): Data => <Data>{ title: pageName };

export const routes: Routes = [
	{ path: '', component: SystemComponent, pathMatch: 'full' },
	{ path: 'custom-fields', component: CustomFieldListComponent, pathMatch: 'full', data: dataWithTitle('Custom Fields') },
];

@NgModule({
	exports: [RouterModule],
	imports: [RouterModule.forChild(routes)],
})
export class SystemRouting {}
