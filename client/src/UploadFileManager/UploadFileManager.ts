import { FileBrowser } from "../FileBrowser/FileBrowser";
import { UrlManager } from "../UrlManager/UrlManager";

export class UploadFileManager {
    private urlManager: UrlManager;
    private fileBrowser: FileBrowser;

    constructor(urlManager: UrlManager, fileBrowser: FileBrowser) {
        this.urlManager = urlManager;
        this.fileBrowser = fileBrowser;
        this.init();
    }

    private init() {
        const uploadButton = document.getElementById('uploadButton') as HTMLButtonElement;
        const fileInput = document.getElementById('fileUploadInput') as HTMLInputElement;

        uploadButton.addEventListener('click', async () => {
            if (fileInput.files) {
                try {
                    await Promise.all(Array.from(fileInput.files).map(file => this.uploadFile(file)));
                    this.fileBrowser.loadDirectory(this.urlManager.path);
                } catch (e) {
                    console.error('Error uploading file:', e);
                }
            }
        });
    }

    public async uploadFile(file: File): Promise<void> {
        const formData = new FormData();
        formData.append('file', file);
        const path = this.urlManager.path;
        const uploadUrl = path ?
            `/api/files/upload?path=${encodeURIComponent(path)}` :
            '/api/files/upload';

        try {
            const response = await fetch(uploadUrl, {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                throw new Error('File upload failed');
            }

            return await response.json();
        } catch (error) {
            console.error(error);
        }
    }
}