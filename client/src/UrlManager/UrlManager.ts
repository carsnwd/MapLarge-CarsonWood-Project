export class UrlManager {
    private currentPath: string = '';
    private searchQuery: string = '';

    constructor() {
        this.initFromUrl();
        this.setupPopstateListener();
    }

    private initFromUrl(): void {
        const urlPath = window.location.pathname;
        if (urlPath && urlPath !== '/') {
            this.currentPath = urlPath.substring(1);
        }
    }

    private setupPopstateListener(): void {
        window.addEventListener('popstate', () => {
            this.parseUrl();
            this.onUrlChanged?.(this.currentPath);
        });
    }

    public parseUrl(): void {
        const urlPath = window.location.pathname;
        const newPath = urlPath && urlPath !== '/' ? urlPath.substring(1) : '';

        if (newPath !== this.currentPath) {
            this.currentPath = newPath;
        }
    }

    public updateUrl(path: string, searchQuery: string = ''): void {
        this.currentPath = path;
        this.searchQuery = searchQuery;

        const newUrl = path ? `/${path}` : '/';
        window.history.replaceState(null, '', newUrl);
    }

    public navigateToPath(path: string): void {
        this.currentPath = path;
        const newUrl = path ? `/${path}` : '/';
        window.history.pushState(null, '', newUrl);
        this.onUrlChanged?.(path);
    }

    // Getters
    public get path(): string {
        return this.currentPath;
    }

    public get query(): string {
        return this.searchQuery;
    }

    // Callback for when URL changes
    public onUrlChanged?: (path: string) => void;
}