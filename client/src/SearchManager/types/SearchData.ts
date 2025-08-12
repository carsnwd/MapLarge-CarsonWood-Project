import { FileSystemItem } from "../../FileBrowser/types/FileSystemItem";

export interface SearchData {
    query: string;
    searchPath: string;
    results: FileSystemItem[];
}