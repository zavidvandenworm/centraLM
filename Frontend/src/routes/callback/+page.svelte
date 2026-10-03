<script lang="ts">
	import { onMount } from 'svelte';
	import { userManager } from '$lib/auth.svelte';
	import toast from 'svelte-french-toast';
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';
	import { page } from '$app/state';

	onMount(async () => {
		const code = page.url.searchParams.get('code');
		const error = page.url.searchParams.get('error');

		if (error) {
			toast.error(page.url.searchParams.get('error_description') ?? `Login failed: ${error}`);
			await goto(resolve('/login'));
			return;
		}

		if (!code) {
			await goto(resolve('/app'));
			return;
		}

		try {
			await userManager.signinRedirectCallback();
			toast.success('Logged in.');
			await goto(resolve('/app'));
		} catch (e) {
			console.error('signinRedirectCallback failed', e);
			toast.error('Could not complete the login. Please try again.');
			await goto(resolve('/login'));
		}
	});
</script>
