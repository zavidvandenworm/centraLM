<script>
	import { authState } from '$lib/auth.svelte';
</script>

{authState.openIdUser?.access_token}
