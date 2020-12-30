import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { SharedModule } from '@shared';
import { BacklogListComponent } from './backlog-list.component';
import { BacklogRouting } from './backlog.routing';
import { FilterBarComponent } from './filter-bar/filter-bar.component';

@NgModule({
	declarations: [BacklogListComponent, FilterBarComponent],
	imports: [
		CommonModule,
		BacklogRouting,
		MatButtonModule,
		MatIconModule,
		MatInputModule,
		MatMenuModule,
		MatPaginatorModule,
		MatProgressSpinnerModule,
		MatSortModule,
		MatTableModule,
		ReactiveFormsModule,
		SharedModule,
	],
})
export class BacklogModule {}
