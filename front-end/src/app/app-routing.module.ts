import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { NoContentComponent } from './no-content';

const routes: Routes = [
	{ path: '', redirectTo: 'backlog', pathMatch: 'full' },
	{ path: 'backlog', loadChildren: () => import('./backlog').then(m => m.BacklogModule), data: { title: 'Backlog Items | YABT' } },
	{ path: '**', component: NoContentComponent },
];

@NgModule({
	imports: [RouterModule.forRoot(routes)],
	exports: [RouterModule],
})
export class AppRoutingModule {}
