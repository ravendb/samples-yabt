import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { BacklogListComponent } from './backlog-list.component';
import { BacklogRouting } from './backlog.routing';

@NgModule({
	declarations: [BacklogListComponent],
	imports: [CommonModule, BacklogRouting],
})
export class BacklogModule {}
