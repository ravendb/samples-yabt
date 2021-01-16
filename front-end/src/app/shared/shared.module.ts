import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatTableModule } from '@angular/material/table';
import { BacklogItemIconComponent } from './elements/backlog-item-icon';
import { BacklogItemStateComponent } from './elements/backlog-item-state';
import { BacklogItemTagsComponent } from './elements/backlog-item-tags';
import { FilterMultiSelectComponent } from './filters/filter-multi-select';
import { FilterSearchComponent } from './filters/filter-search';
import { FilterSingleSelectComponent } from './filters/filter-single-select';

@NgModule({
	declarations: [
		BacklogItemIconComponent,
		BacklogItemStateComponent,
		BacklogItemTagsComponent,
		FilterMultiSelectComponent,
		FilterSingleSelectComponent,
		FilterSearchComponent,
	],
	exports: [
		CommonModule,
		BacklogItemIconComponent,
		BacklogItemStateComponent,
		BacklogItemTagsComponent,
		FilterMultiSelectComponent,
		FilterSingleSelectComponent,
		FilterSearchComponent,
	],
	imports: [
		CommonModule,
		MatButtonModule,
		MatIconModule,
		MatInputModule,
		MatListModule,
		MatTableModule,
		MatMenuModule,
		ReactiveFormsModule,
	],
})
export class SharedModule {}
