import { UrlManager } from '../UrlManager/UrlManager'
import { DirectoryData } from './types/DirectoryData';
import { FileSystemItem } from "./types/FileSystemItem";
import { SearchManager } from '../SearchManager/SearchManager';

/**
 * SVG icon for download action.
 */
const DownloadIcon = `
<svg version="1.1" id="Layer_1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" 
     viewBox="0 0 20 20" enable-background="new 0 0 20 20" xml:space="preserve">
<g id="download-icon">
    <g>
        <path fill-rule="evenodd" clip-rule="evenodd" d="M10,0C4.48,0,0,4.48,0,10c0,5.52,4.48,10,10,10s10-4.48,10-10
            C20,4.48,15.52,0,10,0z M14.71,11.71l-4,4C10.53,15.89,10.28,16,10,16s-0.53-0.11-0.71-0.29l-4-4C5.11,11.53,5,11.28,5,11
            c0-0.55,0.45-1,1-1c0.28,0,0.53,0.11,0.71,0.29L9,12.59V5c0-0.55,0.45-1,1-1s1,0.45,1,1v7.59l2.29-2.29
            C13.47,10.11,13.72,10,14,10c0.55,0,1,0.45,1,1C15,11.28,14.89,11.53,14.71,11.71z"/>
    </g>
</g>
</svg>
`;

/**
 * SVG icon for file action.
 */
const FileIcon = `<svg class="icon document-icon" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
            <path fill-rule="evenodd" clip-rule="evenodd" d="M11.98,0h-8c-0.55,0-1,0.45-1,1v18c0,0.55,0.45,1,1,1h13c0.55,0,1-0.45,1-1V6
                L11.98,0z M15.98,18h-11V2h6v5h5V18z"/>
        </svg>`;

/**
 * SVG icon for directory action.
 */
const DirectoryIcon = `<svg class="icon folder-icon" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
            <path fill-rule="evenodd" clip-rule="evenodd" d="M0,17c0,0.55,0.45,1,1,1h18c0.55,0,1-0.45,1-1V7H0V17z M19,4H9.41L7.71,2.29v0
                C7.53,2.11,7.28,2,7,2H1C0.45,2,0,2.45,0,3v3h20V5C20,4.45,19.55,4,19,4z"/>
        </svg>`;

/**
 * Formats a file size in bytes into a human-readable string.
 * @param bytes The file size in bytes.
 * @returns The formatted file size string.
 */
const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
}

/**
 * Creates a table row for a file or directory.
 * @param props - The properties for the file row.
 * @returns The created table row element.
 */
export const FileRow = (props: { item: FileSystemItem, isDirectory: boolean, loadDirectoryCallback: (path: string) => void, isSearchResult?: boolean }) => {
    const { item, isDirectory, loadDirectoryCallback, isSearchResult = false } = props;
    const row = document.createElement('tr');

    const nameCell = document.createElement('td');
    const icon = isDirectory ? DirectoryIcon : FileIcon;

    const itemNameSpan = document.createElement('span');
    itemNameSpan.className = 'item-name';
    itemNameSpan.textContent = item.name;
    itemNameSpan.title = item.name;
    let nameContent = `<span class="icon">${icon}</span>${itemNameSpan.outerHTML}`;

    const downloadCell = document.createElement('td');
    if (!isDirectory) {
        downloadCell.innerHTML = `<a href="/api/files/download?path=${encodeURIComponent(item.path)}" target="_blank" style="margin-left: 0.5rem; text-decoration: none;"><span class="icon">${DownloadIcon}</span></a>`;
    }

    if (isSearchResult) {
        nameContent += `<br><small style="color: #666; font-style: italic;">Path: ${item.path}</small>`;
    }

    nameCell.innerHTML = nameContent;

    if (isDirectory) {
        nameCell.style.cursor = 'pointer';
        nameCell.onclick = () => loadDirectoryCallback(item.path);
    }

    const typeCell = document.createElement('td');
    typeCell.textContent = isDirectory ? 'Directory' : (item.extension || 'File');

    const sizeCell = document.createElement('td');
    sizeCell.className = 'file-size';
    sizeCell.textContent = isDirectory ? '-' : formatFileSize(item.size || 0);

    const dateCell = document.createElement('td');
    dateCell.textContent = new Date(item.lastModified).toLocaleString();

    row.appendChild(nameCell);
    row.appendChild(typeCell);
    row.appendChild(sizeCell);
    row.appendChild(dateCell);
    row.appendChild(downloadCell);

    return row;
}

/**
 * Manages the file browsing functionality
 */
export class FileBrowser {
    constructor(private urlManager: UrlManager, private searchManager: SearchManager) {
        this.init();
    }

    private init(): void {
        this.urlManager.onUrlChanged = (path: string) => this.loadDirectory(path);
        this.searchManager.onSearchRequested = (query: string) => this.performSearch(query);
        this.searchManager.onClearSearchRequested = () => this.clearSearch();
        this.loadDirectory(this.urlManager.path);
    }

    private async performSearch(query: string): Promise<void> {
        this.showLoading();

        try {
            const data = await this.searchManager.performSearch(query, this.urlManager.path);
            this.searchManager.renderSearchResults(data, (item, isDirectory, isSearchResult) => {
                return FileRow({
                    item,
                    isDirectory,
                    loadDirectoryCallback: (path) => {
                        this.clearSearch();
                        this.loadDirectory(path);
                        this.urlManager.navigateToPath(path);
                    },
                    isSearchResult
                });
            });
        } catch (error) {
            this.showError(`Error performing search: ${error instanceof Error ? error.message : 'Unknown error'}`);
        } finally {
            await this.hideLoading();
        }
    }

    public clearSearch(): void {
        this.searchManager.clearSearch();
        this.loadDirectory(this.urlManager.path);
    }

    private showLoading(): void {
        const loader = document.getElementById('loading');
        if (loader) {
            loader.ariaBusy = 'true';
        }

        const errorEl = document.getElementById('error');
        if (errorEl) {
            errorEl.textContent = '';
        }
    }

    private async hideLoading(): Promise<void> {
        const loader = document.getElementById('loading');
        if (loader) {
            loader.ariaBusy = 'false';
        }
    }

    private showError(message: string): void {
        const errorEl = document.getElementById('error');
        if (errorEl) {
            errorEl.textContent = message;
        }
        this.hideLoading();
    }

    private renderDirectoryCounts(directoryCount: number, fileCount: number): void {
        const countsEl = document.getElementById('directoryCounts');
        if (countsEl) {
            const total = directoryCount + fileCount;
            const directoryText = directoryCount === 1 ? 'directory' : 'directories';
            const fileText = fileCount === 1 ? 'file' : 'files';

            countsEl.innerHTML = `
                ${DirectoryIcon} ${directoryCount} ${directoryText} | ${FileIcon} ${fileCount} ${fileText} | Total: ${total} items
            `;
        }
    }

    public async loadDirectory(path: string): Promise<void> {
        if (this.searchManager.searchMode) {
            this.searchManager.clearSearch();
        }

        this.showLoading();

        try {
            const apiPath = path ?
                `/api/files/browse?path=${encodeURIComponent(path)}` :
                '/api/files/browse';
            const response = await fetch(apiPath);

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data: DirectoryData = await response.json();
            this.renderDirectory(data);
            this.urlManager.updateUrl(path, this.searchManager.searchQuery);
        } catch (error) {
            this.showError(`Error loading directory: ${error instanceof Error ? error.message : 'Unknown error'}`);
        } finally {
            await this.hideLoading();
        }
    }

    private renderDirectory(data: DirectoryData): void {
        this.renderBreadcrumb(data.currentPath);
        this.renderBackLink(data.parentPath);

        this.renderDirectoryCounts(data.directories.length ?? 0, data.files.length ?? 0);

        const tbody = document.getElementById('fileListBody');
        if (!tbody) return;

        tbody.innerHTML = '';

        data.directories.forEach(dir => {
            tbody.appendChild(FileRow({
                item: dir,
                isDirectory: true,
                loadDirectoryCallback: (path) => {
                    this.loadDirectory(path);
                    this.urlManager.navigateToPath(path);
                }
            }));
        });

        data.files.forEach(file => {
            tbody.appendChild(FileRow({
                item: file,
                isDirectory: false,
                loadDirectoryCallback: (path) => this.loadDirectory(path)
            }));
        });
    }

    private renderBreadcrumb(path: string): void {
        const breadcrumb = document.getElementById('breadcrumb');
        if (!breadcrumb) return;

        if (!path) {
            breadcrumb.innerHTML = '<a href="#" onclick="app.loadDirectory(\'\')">Home</a>';
            return;
        }

        const parts = path.split('/').filter(p => p);
        let currentPath = '';
        let breadcrumbHtml = '<a href="#" onclick="app.loadDirectory(\'\')">Home</a>';

        parts.forEach(part => {
            currentPath += (currentPath ? '/' : '') + part;
            breadcrumbHtml += ` / <a href="#" onclick="app.loadDirectory('${currentPath}')">${part}</a>`;
        });

        breadcrumb.innerHTML = breadcrumbHtml;
    }

    private renderBackLink(parentPath: string | null): void {
        const backLink = document.getElementById('backLink');
        if (!backLink) return;

        if (parentPath !== null) {
            backLink.innerHTML = `<a href="#" onclick="app.loadDirectory('${parentPath || ''}')">🔙 Back</a>`;
        } else {
            backLink.innerHTML = '';
        }
    }
}