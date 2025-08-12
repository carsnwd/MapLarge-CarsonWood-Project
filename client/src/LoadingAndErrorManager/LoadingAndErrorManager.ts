export class LoadingAndErrorManager {
    private loadingIndicator: HTMLElement | null = null;
    private errorContainer: HTMLElement | null = null;

    constructor() {
        this.loadingIndicator = document.getElementById('loading');
        this.errorContainer = document.getElementById('error');
    }

    public showLoading(): void {
        if (this.loadingIndicator) {
            this.loadingIndicator.style.display = 'block';
        }
    }

    public hideLoading(): void {
        if (this.loadingIndicator) {
            this.loadingIndicator.style.display = 'none';
        }
    }


    public showError(message: string): void {
        if (this.errorContainer) {
            this.errorContainer.textContent = message;
            this.errorContainer.style.display = 'block';
        }
    }

    public hideError(): void {
        if (this.errorContainer) {
            this.errorContainer.style.display = 'none';
        }
    }

}
