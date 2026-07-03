<script lang="ts">
	import { client } from "./client/client.gen";
    import {appState} from "$lib/auth.svelte";
    import {onMount} from "svelte";
    import {env} from "$env/dynamic/public";
    
    $effect(() => {
        console.log(env.PUBLIC_API_URL)
        client.setConfig({
            baseUrl: env.PUBLIC_API_URL
        })
    })

    $effect(() => {
        client.setConfig({
            headers: {
                Authorization: `Bearer ${appState.user?.access_token}`
            }
        })
    })

</script>
