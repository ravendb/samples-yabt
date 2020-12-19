import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
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
		MatMenuModule,
		MatPaginatorModule,
		MatSortModule,
		MatTableModule,
		SharedModule,
	],
})
export class BacklogModule {}
