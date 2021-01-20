import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { SharedModule } from '@shared';
import { CustomFieldListComponent } from './custom-field/list';
import { SystemComponent } from './system.component';
import { SystemRouting } from './system.routing';

@NgModule({
	declarations: [CustomFieldListComponent, SystemComponent],
	imports: [
		CommonModule,
		SystemRouting,
		MatCardModule,
		MatIconModule,
		MatInputModule,
		MatListModule,
		MatPaginatorModule,
		MatProgressSpinnerModule,
		MatSortModule,
		MatTableModule,
		ReactiveFormsModule,
		SharedModule,
	],
})
export class SystemModule {}
