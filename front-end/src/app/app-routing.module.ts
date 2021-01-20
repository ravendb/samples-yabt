import { NgModule } from '@angular/core';
import { Data, RouterModule, Routes } from '@angular/router';
import { NoContentComponent } from './no-content';

const fullTitle = (pageName: string): Data => <Data>{ title: `${pageName} | YABT` };

const routes: Routes = [
	{ path: '', redirectTo: 'backlog', pathMatch: 'full' },
	{ path: 'backlog', loadChildren: () => import('./backlog').then(m => m.BacklogModule), data: fullTitle('Backlog Items') },
	{ path: 'user', loadChildren: () => import('./user').then(m => m.UserModule), data: fullTitle('Users') },
	{ path: 'system', loadChildren: () => import('./system').then(m => m.SystemModule), data: fullTitle('System') },
	{ path: '**', component: NoContentComponent },
];

@NgModule({
	imports: [RouterModule.forRoot(routes)],
	exports: [RouterModule],
})
export class AppRoutingModule {}
