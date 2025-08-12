export class UploadFileManager {
    constructor(urlManager, fileBrowser, loadingAndErrorManager) {
        this.urlManager = urlManager;
        this.fileBrowser = fileBrowser;
        this.loadingAndErrorManager = loadingAndErrorManager;
        this.init();
    }
    init() {
        const uploadButton = document.getElementById('uploadButton');
        const fileInput = document.getElementById('fileUploadInput');
        uploadButton.addEventListener('click', async () => {
            if (fileInput.files) {
                try {
                    await Promise.all(Array.from(fileInput.files).map(file => this.uploadFile(file)));
                    this.fileBrowser.loadDirectory(this.urlManager.path);
                }
                catch (e) {
                    this.loadingAndErrorManager.showError('Error uploading file');
                    console.error('Error uploading file:', e);
                }
            }
        });
    }
    /**
     * uploads a file to the server, using the current path as well
     * @param file
     * @returns
     */
    async uploadFile(file) {
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
        }
        catch (error) {
            console.error(error);
        }
    }
}
//# sourceMappingURL=UploadFileManager.js.map