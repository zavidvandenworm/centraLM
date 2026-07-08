<script lang="ts">
    import {postGroup, postUsersMeRegister} from "$lib/client";
    import toast from "svelte-french-toast";
    import {goto} from "$app/navigation";
    import {zPostGroupBody, zPostUsersMeRegisterBody} from "$lib/client/zod.gen";
    import {ZodError} from "zod";

    let formData = $state({
        displayName: "",
        biography: ""
    });

    async function validSubmit(){
        const result = await postUsersMeRegister({
            body: formData
        })

        if (result.error) {
            toast.error(result.error.toString());
        } else {
            toast.success("You have successfully registered.");
            goto(`/app`)
        }
    }

    async function submit(e: SubmitEvent){
        e.preventDefault();
        try{
            zPostUsersMeRegisterBody.parse(formData);
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

<div class="absolute inset-0 flex items-center justify-center">
    <div class="prose mx-auto card container">
        <h3>Welcome to centraLM.</h3>
        <p>
            It seems that this is your first time logging in.
            Please enter a display name, and optionally a biography.
            This will be visible to other users.
        </p>
        <form onsubmit={submit}>
            <input required placeholder="Display name" class="mb-2 input input-primary" type="text" bind:value={formData.displayName}/>
            <textarea placeholder="Biography" class="textarea resize-none" bind:value={formData.biography}></textarea>
            <button class="btn btn-primary" type="submit">Register</button>
        </form>
    </div>
</div>

