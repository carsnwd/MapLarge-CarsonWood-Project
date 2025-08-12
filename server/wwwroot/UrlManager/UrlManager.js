export class UrlManager {
    constructor() {
        this.currentPath = '';
        this.searchQuery = '';
        this.initFromUrl();
        this.setupPopstateListener();
    }
    initFromUrl() {
        const urlPath = window.location.pathname;
        if (urlPath && urlPath !== '/') {
            this.currentPath = urlPath.substring(1);
        }
    }
    setupPopstateListener() {
        window.addEventListener('popstate', () => {
            this.parseUrl();
            this.onUrlChanged?.(this.currentPath);
        });
    }
    parseUrl() {
        const urlPath = window.location.pathname;
        const newPath = urlPath && urlPath !== '/' ? urlPath.substring(1) : '';
        if (newPath !== this.currentPath) {
            this.currentPath = newPath;
        }
    }
    updateUrl(path, searchQuery = '') {
        this.currentPath = path;
        this.searchQuery = searchQuery;
        const newUrl = path ? `/${path}` : '/';
        window.history.replaceState(null, '', newUrl);
    }
    navigateToPath(path) {
        this.currentPath = path;
        const newUrl = path ? `/${path}` : '/';
        window.history.pushState(null, '', newUrl);
        this.onUrlChanged?.(path);
    }
    // Getters
    get path() {
        return this.currentPath;
    }
    get query() {
        return this.searchQuery;
    }
}
//# sourceMappingURL=UrlManager.js.map