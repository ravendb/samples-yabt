import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { BacklogListComponent } from './backlog-list.component';
import { BacklogRouting } from './backlog.routing';

@NgModule({
	declarations: [BacklogListComponent],
	imports: [CommonModule, BacklogRouting, MatPaginatorModule, MatSortModule, MatTableModule],
})
export class BacklogModule {}
