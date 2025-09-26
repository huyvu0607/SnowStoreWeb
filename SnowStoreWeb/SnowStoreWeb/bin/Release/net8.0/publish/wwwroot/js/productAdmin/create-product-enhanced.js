// Enhanced Create Product - Fixed Sortable Images

// Global variables
let selectedFiles = [];
let sortableInstance;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    initializeImagePreview();
    initializeFormValidation();
    initializeDragDrop();
    initializeAdditionalImages(); // Make sure this is called
});

// Main Image Preview Handler
function initializeImagePreview() {
    const mainImageInput = document.getElementById('MainImageFile');
    if (!mainImageInput) return;

    mainImageInput.addEventListener('change', function (e) {
        handleMainImageChange(e);
    });
}

function handleMainImageChange(e) {
    const file = e.target.files[0];
    const preview = document.getElementById('mainImagePreview');
    const placeholder = document.getElementById('mainImagePlaceholder');
    const statusBadge = document.getElementById('mainImageStatus');

    if (file) {
        // Validate file size (5MB)
        if (file.size > 5 * 1024 * 1024) {
            showAlert('Ảnh không được lớn hơn 5MB!', 'danger');
            e.target.value = '';
            resetMainImagePreview();
            return;
        }

        // Validate file type
        if (!file.type.startsWith('image/')) {
            showAlert('Vui lòng chọn file hình ảnh!', 'danger');
            e.target.value = '';
            resetMainImagePreview();
            return;
        }

        const reader = new FileReader();
        reader.onload = function (e) {
            preview.src = e.target.result;
            preview.style.display = 'block';
            placeholder.style.display = 'none';

            // Update badge to success state with animation
            statusBadge.style.transform = 'scale(0.8)';
            setTimeout(() => {
                statusBadge.textContent = 'ĐÃ CHỌN';
                statusBadge.className = 'badge bg-success';
                statusBadge.style.transform = 'scale(1.1)';
                setTimeout(() => {
                    statusBadge.style.transform = 'scale(1)';
                }, 200);
            }, 100);

            // Add animation to preview
            preview.style.opacity = '0';
            preview.style.transform = 'scale(0.9)';
            setTimeout(() => {
                preview.style.transition = 'all 0.3s ease';
                preview.style.opacity = '1';
                preview.style.transform = 'scale(1)';
            }, 10);
        };
        reader.readAsDataURL(file);
    } else {
        resetMainImagePreview();
    }
}

function resetMainImagePreview() {
    const preview = document.getElementById('mainImagePreview');
    const placeholder = document.getElementById('mainImagePlaceholder');
    const statusBadge = document.getElementById('mainImageStatus');

    if (preview) {
        preview.style.display = 'none';
        preview.src = '#';
    }
    if (placeholder) {
        placeholder.style.display = 'flex';
    }
    if (statusBadge) {
        // Animate badge reset
        statusBadge.style.transform = 'scale(0.8)';
        setTimeout(() => {
            statusBadge.textContent = 'CHƯA CHỌN';
            statusBadge.className = 'badge bg-secondary';
            statusBadge.style.transform = 'scale(1)';
        }, 100);
    }
}

// Additional Images Handler
function initializeAdditionalImages() {
    const additionalInput = document.getElementById('AdditionalImageFiles');
    if (!additionalInput) return;

    additionalInput.addEventListener('change', function (e) {
        handleAdditionalImagesChange(e);
    });
}

function handleAdditionalImagesChange(e) {
    const files = Array.from(e.target.files);
    const previewContainer = document.getElementById('additionalImagesPreview');
    const noImagesPlaceholder = document.getElementById('noImagesPlaceholder');
    const countBadge = document.getElementById('additionalImageCount');

    if (!previewContainer || !countBadge) return;

    // Validate number of files
    if (files.length > 5) {
        showAlert('Chỉ được chọn tối đa 5 ảnh phụ!', 'warning');
        e.target.value = '';
        return;
    }

    // Clear previous previews
    previewContainer.innerHTML = '';
    selectedFiles = [];

    if (files.length === 0) {
        if (noImagesPlaceholder) {
            previewContainer.appendChild(noImagesPlaceholder);
        }
        countBadge.textContent = '0/5';
        countBadge.className = 'badge bg-secondary';
        return;
    }

    // Validate and process each file
    let validFiles = [];
    let hasError = false;

    files.forEach((file, index) => {
        // Validate file size
        if (file.size > 5 * 1024 * 1024) {
            showAlert(`File ${file.name} không được lớn hơn 5MB!`, 'danger');
            hasError = true;
            return;
        }

        // Validate file type
        if (!file.type.startsWith('image/')) {
            showAlert(`File ${file.name} không phải là hình ảnh!`, 'danger');
            hasError = true;
            return;
        }

        validFiles.push(file);
    });

    if (hasError || validFiles.length === 0) {
        e.target.value = '';
        if (noImagesPlaceholder) {
            previewContainer.appendChild(noImagesPlaceholder);
        }
        countBadge.textContent = '0/5';
        countBadge.className = 'badge bg-secondary';
        return;
    }

    selectedFiles = validFiles;
    countBadge.textContent = `${validFiles.length}/5`;
    countBadge.className = 'badge bg-info';

    // Create preview for each file
    validFiles.forEach((file, index) => {
        createImagePreview(file, index, previewContainer);
    });
}

function createImagePreview(file, index, container) {
    const reader = new FileReader();
    reader.onload = function (e) {
        const colDiv = document.createElement('div');
        colDiv.className = 'col-6 sortable-image';
        colDiv.dataset.fileIndex = index;
        colDiv.innerHTML = `
            <div class="card border-primary">
                <img src="${e.target.result}" class="card-img-top"
                     style="height: 80px; object-fit: cover;" alt="Preview ${index + 1}">
                <div class="card-body p-2">
                    <div class="d-flex justify-content-between align-items-center">
                        <small class="text-primary fw-bold">#${index + 1}</small>
                        <button type="button" class="btn btn-sm btn-danger remove-image-btn"
                                onclick="removePreviewImage(${index})" title="Xóa ảnh">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                    <small class="text-muted d-block text-truncate" title="${file.name}">${file.name}</small>
                </div>
            </div>
        `;

        // Add with animation
        colDiv.style.opacity = '0';
        colDiv.style.transform = 'translateY(20px)';
        container.appendChild(colDiv);

        setTimeout(() => {
            colDiv.style.transition = 'all 0.3s ease';
            colDiv.style.opacity = '1';
            colDiv.style.transform = 'translateY(0)';
        }, index * 100);

        // Initialize sortable after all images are loaded
        if (index === selectedFiles.length - 1) {
            setTimeout(() => initializeSortable(), 300);
        }
    };
    reader.readAsDataURL(file);
}

// Fixed Sortable functionality
function initializeSortable() {
    if (sortableInstance) {
        sortableInstance.destroy();
    }

    const container = document.getElementById('additionalImagesPreview');
    if (!container) return;

    sortableInstance = new Sortable(container, {
        animation: 200,
        ghostClass: 'sortable-ghost',
        chosenClass: 'sortable-chosen',
        dragClass: 'sortable-drag',
        onStart: function () {
            container.style.cursor = 'grabbing';
        },
        onEnd: function (evt) {
            container.style.cursor = '';
            // Reorder the selectedFiles array based on new DOM order
            reorderSelectedFiles();
            // Update image order display and handlers
            updateImageOrder();
            // Update file input with new order
            updateFileInput();
        }
    });
}

// NEW: Reorder selectedFiles array based on DOM order
function reorderSelectedFiles() {
    const imageElements = document.querySelectorAll('#additionalImagesPreview .sortable-image');
    const newOrderedFiles = [];

    imageElements.forEach((element) => {
        const oldIndex = parseInt(element.dataset.fileIndex);
        if (selectedFiles[oldIndex]) {
            newOrderedFiles.push(selectedFiles[oldIndex]);
        }
    });

    selectedFiles = newOrderedFiles;
}

function updateImageOrder() {
    const imageElements = document.querySelectorAll('#additionalImagesPreview .sortable-image');
    imageElements.forEach((element, newIndex) => {
        const orderSpan = element.querySelector('.text-primary');
        const removeBtn = element.querySelector('.remove-image-btn');

        if (orderSpan) {
            orderSpan.textContent = `#${newIndex + 1}`;
        }

        if (removeBtn) {
            removeBtn.setAttribute('onclick', `removePreviewImage(${newIndex})`);
        }

        element.dataset.fileIndex = newIndex;
    });
}

// Remove preview image
function removePreviewImage(fileIndex) {
    if (!confirm('Bạn có muốn xóa ảnh này?')) return;

    const previewContainer = document.getElementById('additionalImagesPreview');
    const countBadge = document.getElementById('additionalImageCount');
    const imageElement = document.querySelector(`[data-file-index="${fileIndex}"]`);

    if (!previewContainer || !countBadge) return;

    if (imageElement) {
        // Animate removal
        imageElement.style.transition = 'all 0.3s ease';
        imageElement.style.opacity = '0';
        imageElement.style.transform = 'translateY(-20px)';

        setTimeout(() => {
            imageElement.remove();

            // Remove from selectedFiles array
            selectedFiles.splice(fileIndex, 1);

            // Update file input
            updateFileInput();

            // Update count badge
            updateCountBadge();

            // Reindex remaining elements
            updateImageOrder();

            // Show placeholder if no images
            if (selectedFiles.length === 0) {
                showNoImagesPlaceholder();
                // Destroy sortable if no images left
                if (sortableInstance) {
                    sortableInstance.destroy();
                    sortableInstance = null;
                }
            }
        }, 300);
    }
}

// IMPROVED: Update file input with current order
function updateFileInput() {
    const fileInput = document.getElementById('AdditionalImageFiles');
    if (!fileInput) return;

    // Create new DataTransfer with files in current order
    const dt = new DataTransfer();
    selectedFiles.forEach(file => {
        dt.items.add(file);
    });

    fileInput.files = dt.files;

    // Trigger change event to notify any listeners
    fileInput.dispatchEvent(new Event('change', { bubbles: true }));
}

function updateCountBadge() {
    const countBadge = document.getElementById('additionalImageCount');
    if (!countBadge) return;

    const count = selectedFiles.length;

    // Add animation effect
    countBadge.style.transform = 'scale(0.8)';

    setTimeout(() => {
        countBadge.textContent = `${count}/5`;

        if (count === 0) {
            countBadge.className = 'badge bg-secondary';
        } else if (count >= 5) {
            countBadge.className = 'badge bg-success';
            countBadge.textContent = '5/5 - HOÀN TẤT';
        } else {
            countBadge.className = 'badge bg-info';
        }

        // Scale back with emphasis
        countBadge.style.transform = 'scale(1.1)';
        setTimeout(() => {
            countBadge.style.transform = 'scale(1)';
        }, 200);
    }, 100);
}

function showNoImagesPlaceholder() {
    const previewContainer = document.getElementById('additionalImagesPreview');

    if (previewContainer) {
        const noImagesHtml = `
            <div class="col-12 text-center text-muted" id="noImagesPlaceholder">
                <i class="fas fa-images fa-2x mb-2"></i>
                <p class="mb-0">Chưa có ảnh phụ</p>
                <small>Ảnh phụ sẽ hiển thị ở đây</small>
            </div>
        `;
        previewContainer.innerHTML = noImagesHtml;
    }
}

// Optimized Drag and Drop
function initializeDragDrop() {
    const placeholder = document.getElementById('mainImagePlaceholder');
    if (!placeholder) return;

    ['dragenter', 'dragover'].forEach(eventName => {
        placeholder.addEventListener(eventName, (e) => {
            e.preventDefault();
            placeholder.classList.add('drag-over');
        });
    });

    ['dragleave', 'drop'].forEach(eventName => {
        placeholder.addEventListener(eventName, (e) => {
            e.preventDefault();
            placeholder.classList.remove('drag-over');
        });
    });

    placeholder.addEventListener('drop', (e) => {
        const files = e.dataTransfer.files;
        if (files.length > 0 && files[0].type.startsWith('image/')) {
            const mainImageInput = document.getElementById('MainImageFile');
            if (mainImageInput) {
                const dt = new DataTransfer();
                dt.items.add(files[0]);
                mainImageInput.files = dt.files;
                mainImageInput.dispatchEvent(new Event('change'));
            }
        }
    });
}

// Form Validation
function initializeFormValidation() {
    const form = document.getElementById('createProductForm');
    if (!form) return;

    form.addEventListener('submit', function (e) {
        if (!validateForm()) {
            e.preventDefault();
            return false;
        }

        showLoadingState();
    });
}

function validateForm() {
    let isValid = true;
    const requiredFields = [
        { id: 'CategoryId', name: 'Danh mục' },
        { id: 'BrandId', name: 'Thương hiệu' },
        { id: 'Name', name: 'Tên sản phẩm' },
        { id: 'Price', name: 'Giá' }
    ];

    // Clear previous errors
    document.querySelectorAll('.is-invalid').forEach(el => {
        el.classList.remove('is-invalid');
    });

    requiredFields.forEach(field => {
        const element = document.getElementById(field.id);
        if (element && (!element.value || element.value.trim() === '')) {
            element.classList.add('is-invalid');
            showAlert(`${field.name} là bắt buộc`, 'danger');
            isValid = false;
        }
    });

    // Validate price
    const priceElement = document.getElementById('Price');
    if (priceElement && priceElement.value) {
        const price = parseFloat(priceElement.value);
        if (price <= 0) {
            priceElement.classList.add('is-invalid');
            showAlert('Giá phải lớn hơn 0', 'danger');
            isValid = false;
        }
    }

    // Validate stock quantity
    const stockElement = document.getElementById('StockQuantity');
    if (stockElement && stockElement.value) {
        const stock = parseInt(stockElement.value);
        if (stock < 0) {
            stockElement.classList.add('is-invalid');
            showAlert('Số lượng tồn không được âm', 'danger');
            isValid = false;
        }
    }

    return isValid;
}

function showLoadingState() {
    const saveButton = document.getElementById('saveButton');
    if (saveButton) {
        saveButton.disabled = true;
        saveButton.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Đang lưu...';
    }
}

// Form Reset
function resetForm() {
    const form = document.getElementById('createProductForm');
    if (form) {
        form.reset();
    }

    // Reset image previews
    resetMainImagePreview();
    resetAdditionalImages();

    // Clear validation states
    document.querySelectorAll('.is-invalid').forEach(el => {
        el.classList.remove('is-invalid');
    });

    // Reset save button
    const saveButton = document.getElementById('saveButton');
    if (saveButton) {
        saveButton.disabled = false;
        saveButton.innerHTML = '<i class="fas fa-save me-1"></i>Lưu Sản Phẩm';
    }
}

function resetAdditionalImages() {
    const previewContainer = document.getElementById('additionalImagesPreview');
    const countBadge = document.getElementById('additionalImageCount');

    if (previewContainer) {
        previewContainer.innerHTML = `
            <div class="col-12 text-center text-muted" id="noImagesPlaceholder">
                <i class="fas fa-images fa-2x mb-2"></i>
                <p class="mb-0">Chưa có ảnh phụ</p>
                <small>Ảnh phụ sẽ hiển thị ở đây</small>
            </div>
        `;
    }

    if (countBadge) {
        countBadge.textContent = '0/5';
        countBadge.className = 'badge bg-secondary';
    }

    selectedFiles = [];

    if (sortableInstance) {
        sortableInstance.destroy();
        sortableInstance = null;
    }
}

// Alert System
function showAlert(message, type = 'info') {
    // Remove existing alerts
    document.querySelectorAll('.alert').forEach(alert => {
        if (!alert.querySelector('[data-bs-dismiss="alert"]')) return;
        alert.remove();
    });

    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            <i class="fas fa-${getAlertIcon(type)} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    const container = document.querySelector('.container-fluid');
    if (container) {
        container.insertAdjacentHTML('afterbegin', alertHtml);

        // Auto dismiss after 5 seconds
        setTimeout(() => {
            const alert = container.querySelector('.alert');
            if (alert && window.bootstrap) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }
        }, 5000);
    }
}

function getAlertIcon(type) {
    const icons = {
        success: 'check-circle',
        danger: 'exclamation-triangle',
        warning: 'exclamation-triangle',
        info: 'info-circle'
    };
    return icons[type] || 'info-circle';
}

// Utility Functions
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

// DEBUG: Log current file order (for testing)
function logCurrentOrder() {
    console.log('Current selectedFiles order:', selectedFiles.map(f => f.name));
    console.log('File input files:', Array.from(document.getElementById('AdditionalImageFiles').files || []).map(f => f.name));
}

// Make functions globally available
window.removePreviewImage = removePreviewImage;
window.resetForm = resetForm;
window.showAlert = showAlert;
window.logCurrentOrder = logCurrentOrder; // For debugging