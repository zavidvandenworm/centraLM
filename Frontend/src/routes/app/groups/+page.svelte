<script lang="ts">
	import { onMount } from 'svelte';
	import { resolve } from '$app/paths';
	import { getGroup, type GroupListingDto, type GroupSortDirection } from '$lib/client';
	import { showRequestError } from '$lib/errors';

	const pageSize = 25;

	const sortOptions = [
		{ label: 'Newest first', value: 1 },
		{ label: 'Oldest first', value: 0 }
	] as const;

	let groups = $state<GroupListingDto[]>([]);
	let keyword = $state('');
	let sortByCreated = $state<GroupSortDirection>(1);
	let skip = $state(0);
	let loading = $state(true);
	let requestId = 0;
	let canGoBack = $derived(skip > 0);
	let canGoForward = $derived(groups.length === pageSize);

	async function loadGroups() {
		const currentRequest = ++requestId;
		loading = true;

		const result = await getGroup({
			query: {
				Skip: skip,
				Limit: pageSize,
				Keyword: keyword.trim() || undefined,
				SortByCreated: sortByCreated
			}
		});

		if (currentRequest !== requestId) return;

		if (result.error) {
			groups = [];
			showRequestError(result.error);
		} else {
			groups = result.data ?? [];
		}

		loading = false;
	}

	function submitSearch(event: SubmitEvent) {
		event.preventDefault();
		skip = 0;
		void loadGroups();
	}

	function changeSort(event: Event) {
		sortByCreated = Number((event.currentTarget as HTMLSelectElement).value) as GroupSortDirection;
		skip = 0;
		void loadGroups();
	}

	function goBack() {
		skip = Math.max(0, skip - pageSize);
		void loadGroups();
	}

	function goForward() {
		skip += pageSize;
		void loadGroups();
	}

	onMount(() => {
		void loadGroups();
	});
</script>

<div class="container mx-auto flex flex-col gap-3">
	<div class="flex w-full flex-row flex-wrap items-end justify-between gap-2">
		<form class="flex flex-row flex-wrap items-end gap-2" onsubmit={submitSearch}>
			<div class="text-sm flex gap-2">
				<span class="label">Search</span>
				<input class="input border-none outline-none" bind:value={keyword} placeholder="Name or description" type="search" />
			</div>

			<div class="text-sm flex gap-2">
				<span class="label">Sort</span>
				<select class="select border-none outline-none" value={sortByCreated} onchange={changeSort}>
					{#each sortOptions as option (option.value)}
						<option value={option.value}>{option.label}</option>
					{/each}
				</select>
			</div>

			<button class="btn btn-primary" type="submit">Search</button>
		</form>

		<a class="btn" href={resolve('/app/groups/create')}>+ Create group</a>
	</div>

	{#if loading}
		<div class="flex w-full justify-center p-8">
			<span class="loading loading-spinner"></span>
		</div>
	{:else if groups.length === 0}
		<div class="border border-base-300 p-8 text-center">
			<p class="text-base-content/70">No groups found.</p>
		</div>
	{:else}

		<ul class="list border border-base-300">
			{#each groups as group (group.id)}
				<li class="list-row">
					<div>
						<a class="link link-hover font-semibold" href={resolve(`/app/group/${group.id}`)}
							>{group.name}</a
						>
						<p class="text-sm text-base-content/70">{group.description}</p>
					</div>
				</li>
			{/each}
		</ul>
	{/if}

	<div class="flex w-full flex-row items-center gap-2">
		<button class="btn btn-sm" disabled={!canGoBack || loading} onclick={goBack} type="button">
			Previous
		</button>
		<button
			class="btn btn-sm"
			disabled={!canGoForward || loading}
			onclick={goForward}
			type="button"
		>
			Next
		</button>
	</div>
</div>
