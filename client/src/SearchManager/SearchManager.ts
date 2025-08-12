import { FileSystemItem } from "../FileBrowser/types/FileSystemItem";
import { SearchData } from "./types/SearchData";

/**
 * Manages the search functionality within the file browser.
 * Added via dependency injection to the FileBrowser class.
 */
export class SearchManager {
    private isSearchMode: boolean = false;
    private currentSearchQuery: string = '';

    constructor() {
        this.setupSearchListeners();
    }

    /**
     * Sets up DOM listeners for the search input, pressing the btns, etc
     */
    private setupSearchListeners(): void {
        const searchButton = document.getElementById('searchButton');
        const searchInput = document.getElementById('searchInput') as HTMLInputElement;

        if (searchButton && searchInput) {
            searchButton.addEventListener('click', () => {
                const query = searchInput.value.trim();
                if (query) {
                    this.onSearchRequested?.(query);
                }
            });

            searchInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    const query = searchInput.value.trim();
                    if (query) {
                        this.onSearchRequested?.(query);
                    }
                }
            });

            searchInput.addEventListener('input', () => {
                if (searchInput.value.trim() === '' && this.isSearchMode) {
                    this.clearSearch();
                    this.onClearSearchRequested?.();
                }
            });
        }
    }

    /**
     * Performs a search for files and directories
     * @param query The search query
     * @param currentPath The current directory path
     * @returns A promise that resolves to the search results
     */
    public async performSearch(query: string, currentPath: string = ''): Promise<SearchData> {
        this.currentSearchQuery = query;
        this.isSearchMode = true;

        const searchPath = currentPath ? `&path=${encodeURIComponent(currentPath)}` : '';
        const apiPath = `/api/files/search?query=${encodeURIComponent(query)}${searchPath}`;

        const response = await fetch(apiPath);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        return await response.json() as SearchData;
    }

    /**
     * Clears the current search state
     */
    public clearSearch(): void {
        this.isSearchMode = false;
        this.currentSearchQuery = '';

        const searchInput = document.getElementById('searchInput') as HTMLInputElement;
        if (searchInput) {
            searchInput.value = '';
        }
    }

    /**
     * Renders the search results in the file list
     * @param data The search data containing the results
     * @param fileRowCreator A function to create file row elements
     * @returns 
     */
    public renderSearchResults(data: SearchData, fileRowCreator: (item: FileSystemItem, isDirectory: boolean, isSearchResult: boolean) => HTMLTableRowElement): void {
        const breadcrumb = document.getElementById('breadcrumb');
        if (breadcrumb) {
            const searchPath = data.searchPath ? ` in ${data.searchPath}` : '';
            breadcrumb.innerHTML = `🔍 Search results for "<strong>${data.query}</strong>"${searchPath} (${data.results.length} results)`;
        }

        const backLink = document.getElementById('backLink');
        if (backLink) {
            backLink.innerHTML = `<a href="#" onclick="app.clearSearch()">🔙 Back to directory</a>`;
        }

        const tbody = document.getElementById('fileListBody');
        if (!tbody) return;

        tbody.innerHTML = '';

        if (data.results.length === 0) {
            const row = document.createElement('tr');
            row.innerHTML = '<td colspan="5" style="text-align: center; font-style: italic; color: #666;">No results found</td>';
            tbody.appendChild(row);
            return;
        }

        data.results.forEach(item => {
            const isDirectory = item.type === 'directory';
            tbody.appendChild(fileRowCreator(item, isDirectory, true));
        });
    }

    // callback funcstions that the file browser uses to update its state based on search state changes
    public onSearchRequested?: (query: string) => void;
    public onClearSearchRequested?: () => void;

    // state getters
    public get searchMode(): boolean {
        return this.isSearchMode;
    }

    public get searchQuery(): string {
        return this.currentSearchQuery;
    }
}