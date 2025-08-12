import { FileBrowser } from './FileBrowser/FileBrowser.js';
import { SearchManager } from './SearchManager/SearchManager.js';
import { UrlManager } from './UrlManager/UrlManager.js';
function initializeApp() {
    const urlManager = new UrlManager();
    const searchManager = new SearchManager();
    const app = new FileBrowser(urlManager, searchManager);
    window.app = app;
}
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeApp);
}
else {
    initializeApp();
}
//# sourceMappingURL=app.js.map