<script lang="ts">
    import {zPostGroupBody} from "$lib/client/zod.gen";
    import toast from "svelte-french-toast";
    import {ZodError} from "zod";
    import {postGroup} from "$lib/client";
    import {goto} from "$app/navigation";
    
    let formData = $state({
        name: "",
        description: ""
    });
    
    async function validSubmit(){
        const result = await postGroup({
            body: formData
        })
        
        if (result.error) {
            toast.error(result.error.toString());
        } else {
            toast.success("Group created");
            goto(`/app/${result.data!.id}`)
        }
    }
    
    async function submit(e: SubmitEvent){
        e.preventDefault();
        try{
            zPostGroupBody.parse(formData);
            await validSubmit()
        } catch (e) {
            if (e instanceof ZodError) {
                for (const error of e.issues) {
                    toast.error(error.message);
                }
            }
        }
    }
</script>   

<form onsubmit={submit} class="container mx-auto">
    <div class="grid grid-cols-1 gap-y-1">
        <label for="name">Name *</label>
        <input required class="input input-primary mb-3" type="text" name="name" bind:value={formData.name} />
    </div>

    <div class="grid grid-cols-1 gap-y-1">
        <label for="description">Description</label>
        <textarea class="textarea resize-none mb-3" name="description" bind:value={formData.description}></textarea>
    </div>
    
    <button class="btn btn-primary">Create</button>
</form>