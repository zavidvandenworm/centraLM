<script lang="ts">
    import { goto } from "$app/navigation";
    import {appState, userManager} from "$lib/auth.svelte";
    import {onMount} from "svelte";
    import type {User} from "oidc-client-ts";
    import Sidebar from "$lib/sidebar/Sidebar.svelte";
    import AuthSync from "$lib/AuthSync.svelte";
	let { children } = $props();
    
    let user: User | null = $state(null)
    
    onMount(async() => {
        let getUser = await userManager.getUser()
    
        if (getUser == null){
            await goto("/login")
        }
        
        user = getUser
    })
</script>

<AuthSync/>

{#if appState.user != null}
    <Sidebar/>
    {@render children()}
{/if}