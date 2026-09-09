import { fileURLToPath } from 'node:url';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import basicSsl from '@vitejs/plugin-basic-ssl';

export default defineConfig({
    plugins: [react(), basicSsl()],
    server: {
        https: true,
        // Not wired up yet: nothing in the app calls these relative paths — every
        // RTK Query base URL in src/config/env.js is still absolute (VITE_*_API_URL).
        // Kept here, disarmed, for whichever future change moves dev traffic to
        // relative same-origin paths; targets are each *.Api project's http
        // launchSettings.json profile (CLAUDE.md §5: no gateway yet, one base URL
        // per owning microservice).
        proxy: {
            '/api/audit': { target: 'http://localhost:5062', changeOrigin: true },
            '/api/finding': { target: 'http://localhost:5153', changeOrigin: true },
            '/api/action-plan': { target: 'http://localhost:5057', changeOrigin: true },
            '/api/notification': { target: 'http://localhost:5079', changeOrigin: true },
            '/api/reporting': { target: 'http://localhost:5026', changeOrigin: true },
        },
    },
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
