export class LoadingAndErrorManager {
    constructor() {
        this.loadingIndicator = null;
        this.errorContainer = null;
        this.loadingIndicator = document.getElementById('loading');
        this.errorContainer = document.getElementById('error');
    }
    showLoading() {
        if (this.loadingIndicator) {
            this.loadingIndicator.style.display = 'block';
        }
    }
    hideLoading() {
        if (this.loadingIndicator) {
            this.loadingIndicator.style.display = 'none';
        }
    }
    showError(message) {
        if (this.errorContainer) {
            this.errorContainer.textContent = message;
            this.errorContainer.style.display = 'block';
        }
    }
    hideError() {
        if (this.errorContainer) {
            this.errorContainer.style.display = 'none';
        }
    }
}
//# sourceMappingURL=LoadingAndErrorManager.js.map