<script lang="ts">
	import { page } from "$app/state";
    import {appState} from "$lib/auth.svelte";
    import UserBadge from "$lib/sidebar/UserBadge.svelte";
    
    const links = [
        {
            name: "Home",
            path: "/app"
        },
        {
            name: "Groups",
            path: "/app/groups"
        },
        {
            name: "Projects",
            path: "/app/projects"
        },
        {
            name: "Insights",
            path: "/app/insights"
        }
    ]
    
</script>

<div class="h-14 p-2 bg-base-300 mb-3">
    <div class="flex-row flex items-center container mx-auto">
        <div class="flex items-center gap-2">
            {#each links as link}
                <a class:bg-base-100!={link.path == page.route.id} href="{link.path}" class="btn btn-ghost text-sm">{link.name}</a>
            {/each}
        </div>
        <UserBadge user={appState.user!} />
    </div>
</div>