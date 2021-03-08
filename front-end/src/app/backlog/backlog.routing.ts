import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { BacklogItemComponent } from './item';
import { BacklogListComponent } from './list/backlog-list.component';

export const routes: Routes = [
	{ path: '', component: BacklogListComponent, pathMatch: 'full' },
	{ path: 'create', component: BacklogItemComponent, data: { title: 'Create' } },
	{ path: ':id', component: BacklogItemComponent, data: { title: '' } },
];

@NgModule({
	exports: [RouterModule],
	imports: [RouterModule.forChild(routes)],
})
export class BacklogRouting {}
