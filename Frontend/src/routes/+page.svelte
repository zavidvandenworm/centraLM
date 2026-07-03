<script lang="ts">
	import { goto } from "$app/navigation";
    import {userManager} from "$lib/auth.svelte";
    import {onMount} from "svelte";
    import type {User} from "oidc-client-ts";
    
    let user: User | null = $state(null)
    
    onMount(async() => {
        user = await userManager.getUser()
        
        if (user == null){
            await goto("/login")
        } else {
            await goto("/app")
        }
    })
</script>
