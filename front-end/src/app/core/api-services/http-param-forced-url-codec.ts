import { HttpParameterCodec } from '@angular/common/http';

/*
 *	Enforce the URI on query string parameters (encoding '+', '-', etc.)
 *	It's a workaround for official Angular bug https://github.com/angular/angular/issues/11058
 */
export class HttpParamForcedUriCodec implements HttpParameterCodec {
	encodeKey(key: string): string {
		return encodeURIComponent(key);
	}

	encodeValue(value: string): string {
		return encodeURIComponent(value);
	}

	decodeKey(key: string): string {
		return decodeURIComponent(key);
	}

	decodeValue(value: string): string {
		return decodeURIComponent(value);
	}
}
