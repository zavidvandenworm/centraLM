<script lang="ts">
    import {userManager} from "$lib/auth.svelte";

    let password: string = $state("")
</script>

<div class="flex flex-col gap-2 items-center justify-center absolute inset-0">
    <button onclick={async() => await userManager.signinRedirect()} class="btn btn-primary btn-outline">Authentik</button>
</div>
