import {type User, UserManager} from 'oidc-client-ts'
import {env} from "$env/dynamic/public";
import {type UserDto} from "$lib/client";
import {client} from "$lib/client/client.gen";

export const userManager = new UserManager({
    authority: env.PUBLIC_OPENID_AUTHORITY!,
    client_id: env.PUBLIC_OPENID_CLIENTID!,
    redirect_uri: `${env.PUBLIC_FRONTEND_URL}/callback`,
    scope: 'openid profile email',
    automaticSilentRenew: true,
})

export let authState: {
    ready: boolean,
    authorized: boolean,
    openIdUser: User | null,
    appUser: UserDto | null
} = $state({
    ready: false,
    openIdUser: null,
    appUser: null,
    authorized: false
})

client.setConfig({
    baseUrl: env.PUBLIC_API_URL
})