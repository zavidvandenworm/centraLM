<script>
    import {authState} from "$lib/auth.svelte";
</script>

<div class="container mx-auto">
    <p>Welcome back, {authState.appUser?.displayName}.</p>
</div>