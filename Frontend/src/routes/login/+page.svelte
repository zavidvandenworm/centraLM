<script lang="ts">
	import { userManager } from '$lib/auth.svelte';
	import toast from 'svelte-french-toast';

	async function login() {
		try {
			await userManager.signinRedirect();
		} catch (e) {
			console.error('signinRedirect failed', e);
			toast.error('Could not reach the identity provider.');
		}
	}
</script>

<div class="flex flex-col gap-2 items-center justify-center absolute inset-0">
	<button onclick={login} class="btn btn-primary btn-outline">Authentik</button>
</div>
