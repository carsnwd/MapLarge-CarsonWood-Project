import { FileSystemItem } from './FileSystemItem';

export interface DirectoryData {
    currentPath: string;
    parentPath: string | null;
    directories: FileSystemItem[];
    files: FileSystemItem[];
}