import { fileURLToPath } from 'node:url';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import basicSsl from '@vitejs/plugin-basic-ssl';

export default defineConfig({
    plugins: [react(), basicSsl()],
    server: { https: true },
    preview: { https: true },
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url)),
        },
    },
    test: {
        globals: true,
        environment: 'jsdom',
        setupFiles: ['./src/test/setup.js'],
        css: true,
        // Generous margin for cold module resolution on slower dev machines — the actual
        // per-test work here is sub-second; see the timing breakdown in vitest's own output.
        testTimeout: 20000,
        hookTimeout: 20000,
        coverage: {
            provider: 'v8',
            reporter: ['text', 'html'],
            exclude: ['src/test/**', 'src/main.jsx'],
        },
    },
});
