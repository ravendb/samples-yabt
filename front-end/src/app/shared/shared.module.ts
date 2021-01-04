import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatTableModule } from '@angular/material/table';
import { BacklogItemIconComponent } from './elements/backlog-item-icon';
import { BacklogItemStateComponent } from './elements/backlog-item-state';
import { BacklogItemTagsComponent } from './elements/backlog-item-tags';
import { FilterMultiSelectComponent } from './filters/filter-multi-select';
import { FilterSingleSelectComponent } from './filters/filter-single-select';

@NgModule({
	declarations: [
		BacklogItemIconComponent,
		BacklogItemStateComponent,
		BacklogItemTagsComponent,
		FilterSingleSelectComponent,
		FilterMultiSelectComponent,
	],
	exports: [
		CommonModule,
		BacklogItemIconComponent,
		BacklogItemStateComponent,
		BacklogItemTagsComponent,
		FilterSingleSelectComponent,
		FilterMultiSelectComponent,
	],
	imports: [CommonModule, MatButtonModule, MatIconModule, MatListModule, MatTableModule, MatMenuModule],
})
export class SharedModule {}
