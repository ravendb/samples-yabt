import { CommonModule, DatePipe } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { SharedModule } from '@shared';
import { UserFiltersComponent } from './list/filters';
import { UserListComponent } from './list/user-list.component';
import { UserRouting } from './user.routing';

@NgModule({
	declarations: [UserListComponent, UserFiltersComponent],
	imports: [
		CommonModule,
		UserRouting,
		MatIconModule,
		MatInputModule,
		MatPaginatorModule,
		MatProgressSpinnerModule,
		MatSortModule,
		MatTableModule,
		ReactiveFormsModule,
		SharedModule,
	],
	providers: [DatePipe],
})
export class UserModule {}