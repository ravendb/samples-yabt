import { NgModule } from '@angular/core';
import { Data, RouterModule, Routes } from '@angular/router';
import { NoContentComponent } from './no-content';

const dataWithTitle = (pageName: string): Data => <Data>{ title: pageName };

const routes: Routes = [
	{ path: '', redirectTo: 'backlog-items', pathMatch: 'full' },
	{ path: 'backlog-items', loadChildren: () => import('./backlog').then(m => m.BacklogModule), data: dataWithTitle('Backlog Items') },
	{ path: 'users', loadChildren: () => import('./user').then(m => m.UserModule), data: dataWithTitle('Users') },
	{ path: 'system', loadChildren: () => import('./system').then(m => m.SystemModule), data: dataWithTitle('System') },
	{ path: '**', component: NoContentComponent },
];

@NgModule({
	imports: [RouterModule.forRoot(routes)],
	exports: [RouterModule],
})
export class AppRoutingModule {}
