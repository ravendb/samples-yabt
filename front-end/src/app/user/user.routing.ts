import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UserListComponent } from './list/user-list.component';

export const routes: Routes = [{ path: '', component: UserListComponent, pathMatch: 'full' }];

@NgModule({
	exports: [RouterModule],
	imports: [RouterModule.forChild(routes)],
})
export class UserRouting {}
