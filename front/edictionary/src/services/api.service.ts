import * as queryString from 'query-string';
import { IFetchArgsData } from "../models/IFetchArgsData";
import { IFetchFormArgsData } from '../models/IFetchFormArgsData';
import { IFetchArgs } from '../models/IFetchArgs';

function getFetchUrl({ endpoint, queryParams }: IFetchArgsData | IFetchFormArgsData) {
	return `http://localhost:5000${endpoint}${queryParams ? `?${queryString.stringify(queryParams)}` : ''}`;
}

function getInitHeaders(contentType = 'application/json', hasContent = true) {
	const headers: HeadersInit = new Headers();
	//const header = authHeader();
	//headers.set('Authorization', header.Authorization);
	if (hasContent) {
	  headers.set('Content-Type', contentType);
	}
	return headers;
}

function getFetchArgs(args: IFetchArgsData): IFetchArgs {
	const headers = getInitHeaders();
  
	if (args.requestData && args.type === 'GET') {
	  throw new Error('GET request does not support request body.');
	}
  
	return {
	  method: args.type,
	  headers,
	  ...(args.type === 'GET' ? {} : { body: JSON.stringify(args.requestData) })
	};
}

async function throwIfResponseFailed(res: Response) {
	if (!res.ok) {
		if (res.status === 401) {
			//logout();
		}
		let parsedException = 'Something went wrong with request!';
		try {
			parsedException = await res.json();
			//toastr.error('Error!', parsedException);
		} catch (err) {
			// eslint-disable-next-line no-console
			console.error(`An error occured: ${err}`);
			//toastr.error('Error!', err);
		}
		throw parsedException;
	}
}

export const callWebApi = async (args: IFetchArgsData): Promise<Response> => {
	const res = await fetch(getFetchUrl(args), getFetchArgs(args));
	await throwIfResponseFailed(res);
	return res;
}