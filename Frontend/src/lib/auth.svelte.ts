import {type User, UserManager} from 'oidc-client-ts'
import {env} from "$env/dynamic/public";

export const userManager = new UserManager({
    authority: env.PUBLIC_OPENID_AUTHORITY!,
    client_id: env.PUBLIC_OPENID_CLIENTID!,
    redirect_uri: `${env.PUBLIC_FRONTEND_URL}/callback`,
    scope: 'openid profile email',
})

export let appState: {
    user: User | null,
} = $state({
    user: null,
})

userManager.getUser().then(user => {
    appState.user = user
})
