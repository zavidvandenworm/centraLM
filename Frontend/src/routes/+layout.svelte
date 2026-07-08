<script lang="ts">
	import './layout.css';
	import favicon from '$lib/assets/favicon.svg';
    import {Toaster} from "svelte-french-toast";
	let { children } = $props();
</script>

<svelte:head>
    <link rel="icon" href={favicon} />
    <title>CentraLM</title>
</svelte:head>

<Toaster/>

<div class="absolute inset-0">
    {@render children()}
</div>