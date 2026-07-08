<script>
    import {page} from "$app/state";

    let groupId = page.params.slug
</script>

<div class="container mx-auto">
    <div class="w-full flex flex-row gap-2 justify-end items-end">
        <a class="btn" href="/app/groups/create">Create subgroup</a>
        <a class="btn" href="/app/groups/create">Create project</a>
    </div>
</div>

