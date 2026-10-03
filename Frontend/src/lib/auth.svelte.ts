import { type User, UserManager, WebStorageStateStore } from 'oidc-client-ts';
import { env } from '$env/dynamic/public';
import { type UserDto } from '$lib/client';
import { client } from '$lib/client/client.gen';
import {browser} from "$app/environment"

export const userManager = new UserManager({
	authority: env.PUBLIC_OPENID_AUTHORITY!,
	client_id: env.PUBLIC_OPENID_CLIENTID!,
	redirect_uri: `${env.PUBLIC_FRONTEND_URL}/callback`,
	post_logout_redirect_uri: `${env.PUBLIC_FRONTEND_URL}/login`,
	response_type: 'code',
	scope: 'openid profile email',
	userStore: browser ? new WebStorageStateStore({ store: window.localStorage }) : null!
});

export const authState: {
	ready: boolean;
	authorized: boolean;
	openIdUser: User | null;
	appUser: UserDto | null;
} = $state({
	ready: false,
	openIdUser: null,
	appUser: null,
	authorized: false
});

client.setConfig({
	baseUrl: env.PUBLIC_API_URL
});
