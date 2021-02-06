import { Pipe, PipeTransform } from '@angular/core';
import { isArray } from 'lodash-es';

@Pipe({
	name: 'join',
})
export class JoinPipe implements PipeTransform {
	transform(input: Array<string> | undefined, sep = ', '): string | undefined {
		return isArray(input) ? input.join(sep) : input;
	}
}
