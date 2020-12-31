import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { BacklogItemIconComponent } from './elements/backlog-item-icon';
import { BacklogItemStateComponent } from './elements/backlog-item-state';
import { BacklogItemTagsComponent } from './elements/backlog-item-tags';

@NgModule({
	declarations: [BacklogItemIconComponent, BacklogItemStateComponent, BacklogItemTagsComponent],
	exports: [CommonModule, BacklogItemIconComponent, BacklogItemStateComponent, BacklogItemTagsComponent],
	imports: [CommonModule, MatTableModule, MatIconModule],
})
export class SharedModule {}
