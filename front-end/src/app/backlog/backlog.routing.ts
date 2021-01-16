import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { BacklogListComponent } from './list/backlog-list.component';

export const routes: Routes = [{ path: '', component: BacklogListComponent, pathMatch: 'full' }];

@NgModule({
	exports: [RouterModule],
	imports: [RouterModule.forChild(routes)],
})
export class BacklogRouting {}
