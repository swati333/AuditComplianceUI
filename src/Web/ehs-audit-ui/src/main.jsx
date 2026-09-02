import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { ensureMsalInitialized } from '@/auth/msalInstance';
import { App } from '@/App';
async function bootstrap() {
    await ensureMsalInitialized();
    const container = document.getElementById('root');
    if (!container) {
        throw new Error('Root container #root not found in index.html');
    }
    createRoot(container).render(<StrictMode>
      <App />
    </StrictMode>);
}
void bootstrap();
