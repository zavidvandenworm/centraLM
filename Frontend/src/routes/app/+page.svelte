<script>
    import {appState} from "$lib/auth.svelte";
</script>

<div class="container mx-auto">
    <p>Welcome back, {appState.user?.profile.nickname}.</p>
</div>