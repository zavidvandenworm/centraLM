<script lang="ts">
	import { authState } from '$lib/auth.svelte';
	import { resolve } from '$app/paths';
</script>

<a
	class="btn btn-ghost gap-2 flex-row ml-auto text-xs flex items-center"
	href={resolve('/app/settings')}
>
	<div
		class="rounded bg-base-content font-semibold flex items-center justify-center text-base-100 w-5 h-5"
	>
		{authState.appUser?.displayName?.substring(0, 2)}
	</div>
	<div class="grid grid-cols-1">
		<p>{authState.appUser?.displayName}</p>
		<p class="text-xs text-base-content/50 text-left">
			{authState.appUser?.userType}
		</p>
	</div>
</a>
