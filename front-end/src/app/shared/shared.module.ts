import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { BacklogItemIconComponent } from './elements/backlog-item-icon/backlog-item-icon.component';

@NgModule({
	declarations: [BacklogItemIconComponent],
	exports: [CommonModule, BacklogItemIconComponent],
	imports: [CommonModule, MatTableModule, MatIconModule],
})
export class SharedModule {}
