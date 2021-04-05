import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { SharedModule } from '@shared';
import { BacklogRouting } from './backlog.routing';
import {
	BacklogItemCommentComponent,
	BacklogItemComponent,
	BacklogItemCustomFieldsComponent,
	BacklogItemFullHistoryDialogComponent,
	BacklogItemHistoryItemComponent,
	BacklogItemRelatedItemsComponent,
	BacklogItemSectionComponent,
	CustomFieldsAddDialogComponent,
	RelatedItemsAddDialogComponent,
} from './item';
import { TagsComponent } from './item/ui-elements/tags';
import { BacklogListComponent } from './list/backlog-list.component';
import { FilterBarComponent } from './list/filter-bar';
import { BacklogFilterDialogComponent } from './list/filter-dialog';
import { BacklogFiltersComponent } from './list/filters';
import { CommentsIconComponent } from './ui-elements/comments-icon';

@NgModule({
	declarations: [
		BacklogListComponent,
		BacklogFiltersComponent,
		CommentsIconComponent,
		FilterBarComponent,
		BacklogFilterDialogComponent,
		BacklogItemComponent,
		BacklogItemCommentComponent,
		BacklogItemHistoryItemComponent,
		BacklogItemSectionComponent,
		TagsComponent,
		BacklogItemFullHistoryDialogComponent,
		BacklogItemRelatedItemsComponent,
		RelatedItemsAddDialogComponent,
		BacklogItemCustomFieldsComponent,
		CustomFieldsAddDialogComponent,
	],
	imports: [
		CommonModule,
		BacklogRouting,
		FormsModule,
		MatAutocompleteModule,
		MatButtonModule,
		MatCheckboxModule,
		MatChipsModule,
		MatDatepickerModule,
		MatDialogModule,
		MatIconModule,
		MatInputModule,
		MatMenuModule,
		MatPaginatorModule,
		MatProgressSpinnerModule,
		MatSelectModule,
		MatSortModule,
		MatTableModule,
		ReactiveFormsModule,
		SharedModule,
	],
})
export class BacklogModule {}
