import tailwindcss from '@tailwindcss/vite';
import adapter from '@sveltejs/adapter-node';
import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig, loadEnv } from 'vite';

export default defineConfig(({ mode }) => {
	let env = loadEnv(mode, process.cwd());
	return {
		plugins: [
			tailwindcss(),
			sveltekit({
				adapter: adapter()
			})
		]
	}
})