import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTableModule } from '@angular/material/table';
import { BacklogItemIconComponent } from './elements/backlog-item-icon';
import { BacklogItemStateComponent } from './elements/backlog-item-state';
import { BacklogItemTagsComponent } from './elements/backlog-item-tags';
import { FilterDropdownComponent } from './elements/filter-dropdown-control';

@NgModule({
	declarations: [BacklogItemIconComponent, BacklogItemStateComponent, BacklogItemTagsComponent, FilterDropdownComponent],
	exports: [CommonModule, BacklogItemIconComponent, BacklogItemStateComponent, BacklogItemTagsComponent, FilterDropdownComponent],
	imports: [CommonModule, MatButtonModule, MatIconModule, MatTableModule, MatMenuModule],
})
export class SharedModule {}
