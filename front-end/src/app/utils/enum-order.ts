import { KeyValue } from '@angular/common';

// A workaround to preserve the original sorting order of the enum. By default, it's sorted alphabetically
export const originalEnumOrder = <TEnum>(a: KeyValue<string, TEnum>, b: KeyValue<string, TEnum>): number => 0;
