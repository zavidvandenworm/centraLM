<script lang="ts">
    import {client} from "./client/client.gen";
    import {authState, userManager} from "$lib/auth.svelte";
    import {onMount} from "svelte";
    import {getUsersMe} from "$lib/client";
    import {goto} from "$app/navigation";

    $effect(() => {
        authState.authorized = authState.ready && !!authState.appUser && !!authState.openIdUser
    })

    function registerAuthEvents() {
        userManager.events.addUserLoaded((user) => {
            authState.openIdUser = user

            client.setConfig({
                headers: {
                    Authorization: `Bearer ${user.access_token}`
                }
            })
        })

        userManager.events.addSilentRenewError((err) => {
            client.setConfig({
                headers: {
                    Authorization: null
                }
            })
        })
    }

    onMount(async () => {
        const user = await userManager.getUser()

        if (!user) {
            authState.ready = true
            await goto("/login")
            return
        }

        client.setConfig({
            headers: {
                Authorization: `Bearer ${user.access_token}`
            }
        })

        registerAuthEvents()
        userManager.startSilentRenew()

        authState.openIdUser = user

        const result = await getUsersMe();

        if (!result.data) {
            authState.ready = true
            await goto("/onboard")
            return
        }

        authState.appUser = result.data
        authState.ready = true
    })

</script>
