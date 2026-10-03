<script lang="ts">
	import { zPostGroupBody } from '$lib/client/zod.gen';
	import toast from 'svelte-french-toast';
	import { ZodError } from 'zod';
	import { postGroup } from '$lib/client';
	import { goto } from '$app/navigation';
	import { formatFieldName, showRequestError } from '$lib/errors';

	let formData = $state({
		name: '',
		description: ''
	});

	async function validSubmit() {
		const result = await postGroup({
			body: formData
		});

		if (result.error) {
			showRequestError(result.error);
		} else {
			toast.success('Group created');
			goto(`/app/group/${result.data!.id}`);
		}
	}

	async function submit(e: SubmitEvent) {
		e.preventDefault();
		try {
			zPostGroupBody.parse(formData);
			await validSubmit();
		} catch (e) {
			if (e instanceof ZodError) {
				for (const error of e.issues) {
					const field = error.path.length > 0 ? `${formatFieldName(String(error.path[0]))}: ` : '';
					toast.error(`${field}${error.message}`);
				}
			}
		}
	}
</script>

<form class="container mx-auto" onsubmit={submit}>
	<div class="grid grid-cols-1 gap-y-1">
		<label for="name">Name *</label>
		<input
			bind:value={formData.name}
			class="input input-primary mb-3"
			name="name"
			required
			type="text"
		/>
	</div>

	<div class="grid grid-cols-1 gap-y-1">
		<label for="description">Description</label>
		<textarea bind:value={formData.description} class="textarea resize-none mb-3" name="description"
		></textarea>
	</div>

	<button class="btn btn-primary">Create</button>
</form>
