<script lang="ts">
    import {onMount} from "svelte";
    import {userManager} from "$lib/auth.svelte";
    import toast from "svelte-french-toast";
    import {goto} from "$app/navigation";

    onMount(async() => {
        await userManager.signinRedirectCallback()
        toast.success("Logged in.")
        goto("/app")
    })
</script>