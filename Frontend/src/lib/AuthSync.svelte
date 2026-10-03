<script lang="ts">
	import { client } from './client/client.gen';
	import { authState, userManager } from '$lib/auth.svelte';
	import { onMount } from 'svelte';
	import { getUsersMe } from '$lib/client';
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';

	client.setConfig({
		auth: async () => (await userManager.getUser())?.access_token
	});

	function clearAuth() {
		authState.openIdUser = null;
		authState.appUser = null;
		authState.authorized = false;
	}

	async function redirectToLogin() {
		clearAuth();
		await goto(resolve('/login'));
	}

	function registerAuthEvents() {
		userManager.events.addUserLoaded((user) => {
			authState.openIdUser = user;
		});

		userManager.events.addAccessTokenExpiring(() => {
			void redirectToLogin();
		});

		userManager.events.addUserUnloaded(() => {
			void redirectToLogin();
		});

		userManager.events.addUserSignedOut(() => {
			void redirectToLogin();
		});
	}

	onMount(async () => {
		const user = await userManager.getUser();

		if (!user || user.expired) {
			authState.ready = true;
			await goto(resolve('/login'));
			return;
		}

		registerAuthEvents();
		authState.openIdUser = user;

		const result = await getUsersMe();

		if (result.error) {
			if (result.response?.status === 404) {
				authState.ready = true;
				await goto(resolve('/onboard'));
				return;
			}

			await redirectToLogin();
			return;
		}

		authState.appUser = result.data ?? null;
		authState.ready = true;
	});

	$effect(() => {
		authState.authorized = authState.ready && !!authState.appUser && !!authState.openIdUser;
	});
</script>
