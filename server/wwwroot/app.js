import { FileBrowser } from './FileBrowser/FileBrowser.js';
import { SearchManager } from './SearchManager/SearchManager.js';
import { UploadFileManager } from './UploadFileManager/UploadFileManager.js';
import { UrlManager } from './UrlManager/UrlManager.js';
function initializeApp() {
    const urlManager = new UrlManager();
    const searchManager = new SearchManager();
    const fileBrowser = new FileBrowser(urlManager, searchManager);
    const uploadFileManager = new UploadFileManager(urlManager, fileBrowser);
    window.app = {
        fileBrowser,
        uploadFileManager
    };
}
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeApp);
}
else {
    initializeApp();
}
//# sourceMappingURL=app.js.map