import { FileBrowser } from './FileBrowser/FileBrowser.js';
import { LoadingAndErrorManager } from './LoadingAndErrorManager/LoadingAndErrorManager.js';
import { SearchManager } from './SearchManager/SearchManager.js';
import { UploadFileManager } from './UploadFileManager/UploadFileManager.js';
import { UrlManager } from './UrlManager/UrlManager.js';

function initializeApp() {
    const urlManager = new UrlManager();
    const searchManager = new SearchManager();
    const loadingAndErrorManager = new LoadingAndErrorManager();
    const fileBrowser = new FileBrowser(urlManager, searchManager, loadingAndErrorManager);
    const uploadFileManager = new UploadFileManager(urlManager, fileBrowser, loadingAndErrorManager);

    (window as any).app = {
        fileBrowser,
        uploadFileManager
    };
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeApp);
} else {
    initializeApp();
}