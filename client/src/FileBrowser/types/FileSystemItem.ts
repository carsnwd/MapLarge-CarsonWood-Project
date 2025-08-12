export interface FileSystemItem {
    name: string;
    path: string;
    lastModified: string;
    type: 'file' | 'directory';
    size?: number;
    extension?: string;
}