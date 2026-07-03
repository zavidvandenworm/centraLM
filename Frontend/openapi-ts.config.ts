import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
    input: 'http://localhost:5228/openapi/v1.json',
    output: 'src/lib/client',
    plugins: [
        '@hey-api/typescript', 
        '@hey-api/sdk',
        'zod'
    ],
});