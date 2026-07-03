<script lang="ts">
    import type {User} from "oidc-client-ts";
    let {user}: {user: User} = $props();
</script>

<a href="/app/settings" class="btn btn-ghost gap-2 flex-row ml-auto text-xs flex items-center">
    {#if user?.profile.picture}
        <img alt="profile" class="w-5 h-5" src="{user?.profile.picture}"/>
    {:else}
        <div class="rounded bg-base-content font-semibold flex items-center justify-center text-base-100 w-5 h-5">
            {user?.profile.nickname?.substring(0, 2)}
        </div>
    {/if}
    <div>
        <p>{user?.profile.nickname}</p>
    </div>
</a>