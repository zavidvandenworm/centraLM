<script>
    import {appState} from "$lib/auth.svelte";
</script>

{appState.user?.access_token}