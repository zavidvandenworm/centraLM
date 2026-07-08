<script>
    import {appState} from "$lib/auth.svelte";
</script>

{appState.openIdUser?.access_token}