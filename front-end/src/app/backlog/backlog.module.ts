import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { SharedModule } from '@shared';
import { BacklogRouting } from './backlog.routing';
import { BacklogItemComponent } from './item';
import { BacklogItemCommentComponent } from './item/comment';
import { BacklogItemSectionComponent } from './item/section';
import { BacklogListComponent } from './list/backlog-list.component';
import { FilterBarComponent } from './list/filter-bar';
import { BacklogFilterDialogComponent } from './list/filter-dialog';
import { BacklogFiltersComponent } from './list/filters';
import { CommentsIconComponent } from './ui-elements/comments-icon';
import { TagsComponent } from './ui-elements/tags';

@NgModule({
	declarations: [
		BacklogListComponent,
		BacklogFiltersComponent,
		CommentsIconComponent,
		FilterBarComponent,
		BacklogFilterDialogComponent,
		BacklogItemComponent,
		BacklogItemCommentComponent,
		BacklogItemSectionComponent,
		TagsComponent,
	],
	imports: [
		CommonModule,
		BacklogRouting,
		MatAutocompleteModule,
		MatButtonModule,
		MatChipsModule,
		MatDialogModule,
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
