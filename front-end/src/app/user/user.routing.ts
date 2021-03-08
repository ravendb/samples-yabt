import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UserItemComponent } from './item';
import { UserListComponent } from './list/user-list.component';

export const routes: Routes = [
	{ path: '', component: UserListComponent, pathMatch: 'full' },
	{ path: 'create', component: UserItemComponent, data: { title: 'Create' } },
	{ path: ':id', component: UserItemComponent, data: { title: '' } },
];

@NgModule({
	exports: [RouterModule],
	imports: [RouterModule.forChild(routes)],
})
export class UserRouting {}
