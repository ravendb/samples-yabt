import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { BacklogItemIconComponent } from './elements/backlog-item-icon';
import { BacklogItemStateComponent } from './elements/backlog-item-state';

@NgModule({
	declarations: [BacklogItemIconComponent, BacklogItemStateComponent],
	exports: [CommonModule, BacklogItemIconComponent, BacklogItemStateComponent],
	imports: [CommonModule, MatTableModule, MatIconModule],
})
export class SharedModule {}
