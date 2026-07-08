<script lang="ts">
    import {authState} from "$lib/auth.svelte";
    import AuthSync from "$lib/AuthSync.svelte";
    import {goto} from "$app/navigation";
	let { children } = $props();
    
    $effect(() => {
        if (authState.authorized){
            goto("/app")
        }
    })
    
</script>

<AuthSync/>

{#if authState.ready}
    {@render children()}
{:else}
    <div class="absolute inset-0 flex items-center justify-center">
        <p class="animate-spin w-4 h-4 text-center">|</p>
    </div>
{/if}