const formatFileSize = (bytes) => {
    if (bytes === 0)
        return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
};
export const FileRow = (props) => {
    const { item, isDirectory, loadDirectoryCallback } = props;
    const row = document.createElement('tr');
    const nameCell = document.createElement('td');
    const icon = isDirectory ?
        `<svg class="file-icon folder-icon" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
            <path fill-rule="evenodd" clip-rule="evenodd" d="M0,17c0,0.55,0.45,1,1,1h18c0.55,0,1-0.45,1-1V7H0V17z M19,4H9.41L7.71,2.29v0
                C7.53,2.11,7.28,2,7,2H1C0.45,2,0,2.45,0,3v3h20V5C20,4.45,19.55,4,19,4z"/>
        </svg>` :
        `<svg class="file-icon document-icon" viewBox="0 0 20 20" xmlns="http://www.w3.org/2000/svg">
            <path fill-rule="evenodd" clip-rule="evenodd" d="M11.98,0h-8c-0.55,0-1,0.45-1,1v18c0,0.55,0.45,1,1,1h13c0.55,0,1-0.45,1-1V6
                L11.98,0z M15.98,18h-11V2h6v5h5V18z"/>
        </svg>`;
    nameCell.innerHTML = `<span class="icon">${icon}</span><span class="${isDirectory ? 'directory' : 'file'}">${item.name}</span>`;
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
    return row;
};
//# sourceMappingURL=FileRow.js.map