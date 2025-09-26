// Enhanced Product List Management - JavaScript File with Custom Modal

// Custom Modal System
class CustomModal {
    constructor() {
        this.overlay = null;
        this.modal = null;
        this.isOpen = false;
        this.createModal();
    }

    createModal() {
        // Create overlay
        this.overlay = document.createElement('div');
        this.overlay.className = 'custom-modal-overlay';
        this.overlay.innerHTML = `
            <div class="custom-modal">
                <div class="custom-modal-header">
                    <h3 class="custom-modal-title">
                        <div class="custom-modal-icon">
                            <i class="fas fa-question-circle"></i>
                        </div>
                        <span class="custom-modal-title-text">Xác nhận</span>
                    </h3>
                </div>
                <div class="custom-modal-body">
                    <p class="custom-modal-message">Bạn có chắc chắn muốn thực hiện hành động này?</p>
                </div>
                <div class="custom-modal-footer">
                    <button type="button" class="custom-modal-btn btn-secondary" data-action="cancel">
                        Hủy
                    </button>
                    <button type="button" class="custom-modal-btn btn-danger" data-action="confirm">
                        <i class="fas fa-check"></i>
                        Xác nhận
                    </button>
                </div>
            </div>
        `;

        // Add CSS styles
        this.addStyles();

        // Add to body
        document.body.appendChild(this.overlay);

        // Add event listeners
        this.setupEventListeners();
    }

    addStyles() {
        if (document.getElementById('custom-modal-styles')) return;

        const style = document.createElement('style');
        style.id = 'custom-modal-styles';
        style.textContent = `
            /* Custom Modal Styles */
            .custom-modal-overlay {
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0, 0, 0, 0.6);
                display: none;
                justify-content: center;
                align-items: center;
                z-index: 10000;
                backdrop-filter: blur(5px);
                animation: fadeIn 0.3s ease-out;
            }

            .custom-modal-overlay.show {
                display: flex;
            }

            .custom-modal {
                background: white;
                border-radius: 16px;
                box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
                max-width: 450px;
                width: 90%;
                max-height: 90vh;
                overflow: hidden;
                transform: scale(0.8);
                animation: modalSlideIn 0.3s ease-out forwards;
                position: relative;
            }

            .custom-modal-header {
                padding: 24px 24px 16px;
                border-bottom: 1px solid #f0f0f0;
            }

            .custom-modal-title {
                margin: 0;
                font-size: 20px;
                font-weight: 600;
                color: #2c3e50;
                display: flex;
                align-items: center;
                gap: 12px;
            }

            .custom-modal-icon {
                width: 40px;
                height: 40px;
                border-radius: 50%;
                display: flex;
                align-items: center;
                justify-content: center;
                font-size: 18px;
                flex-shrink: 0;
            }

            .custom-modal-icon.danger {
                background: linear-gradient(135deg, #ff6b6b, #ee5a52);
                color: white;
            }

            .custom-modal-icon.warning {
                background: linear-gradient(135deg, #feca57, #ff9ff3);
                color: white;
            }

            .custom-modal-icon.info {
                background: linear-gradient(135deg, #74b9ff, #0984e3);
                color: white;
            }

            .custom-modal-icon.success {
                background: linear-gradient(135deg, #00b894, #00a085);
                color: white;
            }

            .custom-modal-body {
                padding: 20px 24px;
            }

            .custom-modal-message {
                font-size: 16px;
                line-height: 1.5;
                color: #555;
                margin: 0;
            }

            .custom-modal-footer {
                padding: 16px 24px 24px;
                display: flex;
                gap: 12px;
                justify-content: flex-end;
            }

            .custom-modal-btn {
                padding: 12px 24px;
                border: none;
                border-radius: 8px;
                font-size: 14px;
                font-weight: 500;
                cursor: pointer;
                transition: all 0.2s ease;
                min-width: 80px;
                position: relative;
                overflow: hidden;
            }

            .custom-modal-btn:hover {
                transform: translateY(-1px);
                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
            }

            .custom-modal-btn:active {
                transform: translateY(0);
            }

            .custom-modal-btn.btn-danger {
                background: linear-gradient(135deg, #ff6b6b, #ee5a52);
                color: white;
            }

            .custom-modal-btn.btn-danger:hover {
                background: linear-gradient(135deg, #ee5a52, #dd4b39);
            }

            .custom-modal-btn.btn-secondary {
                background: #f8f9fa;
                color: #6c757d;
                border: 1px solid #dee2e6;
            }

            .custom-modal-btn.btn-secondary:hover {
                background: #e9ecef;
                color: #5a6169;
            }

            .custom-modal-btn.btn-warning {
                background: linear-gradient(135deg, #feca57, #ff9ff3);
                color: white;
            }

            .custom-modal-btn.btn-success {
                background: linear-gradient(135deg, #00b894, #00a085);
                color: white;
            }

            .custom-modal-btn.btn-primary {
                background: linear-gradient(135deg, #74b9ff, #0984e3);
                color: white;
            }

            .custom-modal-btn.btn-info {
                background: linear-gradient(135deg, #74b9ff, #0984e3);
                color: white;
            }

            .custom-modal-spinner {
                width: 20px;
                height: 20px;
                border: 2px solid transparent;
                border-top: 2px solid currentColor;
                border-radius: 50%;
                animation: spin 1s linear infinite;
                margin-right: 8px;
            }

            @keyframes spin {
                0% { transform: rotate(0deg); }
                100% { transform: rotate(360deg); }
            }

            @keyframes fadeIn {
                from { opacity: 0; }
                to { opacity: 1; }
            }

            @keyframes modalSlideIn {
                from {
                    transform: scale(0.8) translateY(-20px);
                    opacity: 0;
                }
                to {
                    transform: scale(1) translateY(0);
                    opacity: 1;
                }
            }

            @keyframes modalSlideOut {
                from {
                    transform: scale(1) translateY(0);
                    opacity: 1;
                }
                to {
                    transform: scale(0.8) translateY(-20px);
                    opacity: 0;
                }
            }

            .custom-modal.closing {
                animation: modalSlideOut 0.3s ease-out forwards;
            }

            @media (max-width: 480px) {
                .custom-modal {
                    max-width: 95%;
                    margin: 20px;
                }
                
                .custom-modal-footer {
                    flex-direction: column-reverse;
                }
                
                .custom-modal-btn {
                    width: 100%;
                }
            }
        `;
        document.head.appendChild(style);
    }

    setupEventListeners() {
        // Click outside to close
        this.overlay.addEventListener('click', (e) => {
            if (e.target === this.overlay) {
                this.close(false);
            }
        });

        // Button clicks
        this.overlay.addEventListener('click', (e) => {
            if (e.target.hasAttribute('data-action')) {
                const action = e.target.getAttribute('data-action');
                this.close(action === 'confirm');
            }
        });

        // Escape key to close
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isOpen) {
                this.close(false);
            }
        });
    }

    show(options = {}) {
        return new Promise((resolve) => {
            this.resolve = resolve;

            // Set default options
            const config = {
                title: 'Xác nhận',
                message: 'Bạn có chắc chắn muốn thực hiện hành động này?',
                type: 'danger',
                confirmText: 'Xác nhận',
                cancelText: 'Hủy',
                confirmIcon: 'fas fa-check',
                showCancel: true,
                ...options
            };

            // Update content
            this.updateContent(config);

            // Show modal
            this.overlay.classList.add('show');
            this.isOpen = true;

            // Focus on confirm button
            setTimeout(() => {
                const confirmBtn = this.overlay.querySelector('[data-action="confirm"]');
                if (confirmBtn) {
                    confirmBtn.focus();
                }
            }, 300);
        });
    }

    updateContent(config) {
        // Update icon
        const iconElement = this.overlay.querySelector('.custom-modal-icon');
        const iconClass = this.getIconClass(config.type);
        iconElement.className = `custom-modal-icon ${config.type}`;
        iconElement.querySelector('i').className = iconClass;

        // Update title
        const titleElement = this.overlay.querySelector('.custom-modal-title-text');
        titleElement.textContent = config.title;

        // Update message
        const messageElement = this.overlay.querySelector('.custom-modal-message');
        messageElement.textContent = config.message;

        // Update buttons
        const confirmBtn = this.overlay.querySelector('[data-action="confirm"]');
        const cancelBtn = this.overlay.querySelector('[data-action="cancel"]');

        confirmBtn.innerHTML = `<i class="${config.confirmIcon}"></i> ${config.confirmText}`;
        confirmBtn.className = `custom-modal-btn btn-${config.type}`;

        cancelBtn.textContent = config.cancelText;
        cancelBtn.style.display = config.showCancel ? 'inline-block' : 'none';
    }

    getIconClass(type) {
        const icons = {
            danger: 'fas fa-exclamation-triangle',
            warning: 'fas fa-exclamation-circle',
            info: 'fas fa-info-circle',
            success: 'fas fa-check-circle',
            question: 'fas fa-question-circle'
        };
        return icons[type] || icons.question;
    }

    close(confirmed) {
        if (!this.isOpen) return;

        const modal = this.overlay.querySelector('.custom-modal');
        modal.classList.add('closing');

        setTimeout(() => {
            this.overlay.classList.remove('show');
            modal.classList.remove('closing');
            this.isOpen = false;

            if (this.resolve) {
                this.resolve(confirmed);
                this.resolve = null;
            }
        }, 300);
    }

    // Static methods for easy use
    static async confirm(message, title = 'Xác nhận') {
        const modal = window.customModalInstance || new CustomModal();
        if (!window.customModalInstance) {
            window.customModalInstance = modal;
        }

        return modal.show({
            title,
            message,
            type: 'danger',
            confirmText: 'Xác nhận',
            confirmIcon: 'fas fa-check'
        });
    }

    static async warning(message, title = 'Cảnh báo') {
        const modal = window.customModalInstance || new CustomModal();
        if (!window.customModalInstance) {
            window.customModalInstance = modal;
        }

        return modal.show({
            title,
            message,
            type: 'warning',
            confirmText: 'Tiếp tục',
            confirmIcon: 'fas fa-exclamation-triangle'
        });
    }

    static async info(message, title = 'Thông tin') {
        const modal = window.customModalInstance || new CustomModal();
        if (!window.customModalInstance) {
            window.customModalInstance = modal;
        }

        return modal.show({
            title,
            message,
            type: 'info',
            confirmText: 'Đồng ý',
            confirmIcon: 'fas fa-check',
            showCancel: false
        });
    }

    static async success(message, title = 'Thành công') {
        const modal = window.customModalInstance || new CustomModal();
        if (!window.customModalInstance) {
            window.customModalInstance = modal;
        }

        return modal.show({
            title,
            message,
            type: 'success',
            confirmText: 'OK',
            confirmIcon: 'fas fa-check',
            showCancel: false
        });
    }

    static async delete(itemName = 'mục này') {
        const modal = window.customModalInstance || new CustomModal();
        if (!window.customModalInstance) {
            window.customModalInstance = modal;
        }

        return modal.show({
            title: 'Xóa sản phẩm',
            message: `Bạn có chắc chắn muốn xóa ${itemName}? Hành động này không thể hoàn tác.`,
            type: 'danger',
            confirmText: 'Xóa',
            confirmIcon: 'fas fa-trash',
            cancelText: 'Hủy'
        });
    }

    static async bulkDelete(count) {
        const modal = window.customModalInstance || new CustomModal();
        if (!window.customModalInstance) {
            window.customModalInstance = modal;
        }

        return modal.show({
            title: 'Xóa nhiều sản phẩm',
            message: `Bạn có chắc chắn muốn xóa ${count} sản phẩm đã chọn? Hành động này không thể hoàn tác.`,
            type: 'danger',
            confirmText: 'Xóa tất cả',
            confirmIcon: 'fas fa-trash',
            cancelText: 'Hủy'
        });
    }

    static async bulkAction(action, count) {
        const modal = window.customModalInstance || new CustomModal();
        if (!window.customModalInstance) {
            window.customModalInstance = modal;
        }

        let title = 'Xác nhận';
        let message = `Bạn có chắc chắn muốn thực hiện hành động này với ${count} sản phẩm đã chọn?`;
        let confirmText = 'Xác nhận';
        let icon = 'fas fa-check';

        switch (action) {
            case 'sethot':
                title = 'Đặt sản phẩm Hot';
                message = `Đặt ${count} sản phẩm đã chọn là sản phẩm Hot?`;
                confirmText = 'Đặt Hot';
                icon = 'fas fa-fire';
                break;
            case 'setbestseller':
                title = 'Đặt sản phẩm Bán chạy';
                message = `Đặt ${count} sản phẩm đã chọn là sản phẩm Bán chạy?`;
                confirmText = 'Đặt Bán chạy';
                icon = 'fas fa-star';
                break;
        }

        return modal.show({
            title,
            message,
            type: 'warning',
            confirmText,
            confirmIcon: icon,
            cancelText: 'Hủy'
        });
    }
}

// Global variables
let bulkMode = false;

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    initializeDescriptionModal();
    initializeToasts();
    initializeSearchBehavior();
    replaceDefaultConfirms();

    // Initialize custom modal
    if (!window.customModalInstance) {
        window.customModalInstance = new CustomModal();
    }
});

// Replace default confirm dialogs with custom modals
function replaceDefaultConfirms() {
    // Replace form onsubmit confirms
    const deleteFormsQuery = 'form[onsubmit*="confirm"]';
    const deleteForms = document.querySelectorAll(deleteFormsQuery);

    deleteForms.forEach(form => {
        form.removeAttribute('onsubmit');
        form.addEventListener('submit', async function (e) {
            e.preventDefault();

            const confirmed = await CustomModal.delete('sản phẩm này');
            if (confirmed) {
                // Remove event listener to avoid infinite loop
                form.removeEventListener('submit', arguments.callee);
                form.submit();
            }
        });
    });
}

// Initialize search behavior
function initializeSearchBehavior() {
    const searchTerm = document.querySelector('input[name="searchTerm"]');
    const productId = document.querySelector('input[name="productId"]');

    // Clear search term when typing Product ID
    if (productId) {
        productId.addEventListener('input', function () {
            if (this.value.trim() !== '' && searchTerm) {
                searchTerm.value = '';
            }
        });
    }

    // Clear Product ID when typing search term
    if (searchTerm) {
        searchTerm.addEventListener('input', function () {
            if (this.value.trim() !== '' && productId) {
                productId.value = '';
            }
        });
    }
}

// Description Modal Functions
function initializeDescriptionModal() {
    // Create modal HTML
    const modalHTML = `
        <div id="descriptionModal" class="description-modal">
            <div class="description-modal-content">
                <div class="description-modal-header">
                    <h5 class="description-modal-title">Mô tả sản phẩm</h5>
                    <button type="button" class="description-modal-close" onclick="closeDescriptionModal()">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="description-modal-body" id="descriptionModalBody">
                    <!-- Content will be inserted here -->
                </div>
            </div>
        </div>
    `;

    // Add modal to body
    document.body.insertAdjacentHTML('beforeend', modalHTML);

    // Add click outside to close
    document.getElementById('descriptionModal').addEventListener('click', function (e) {
        if (e.target === this) {
            closeDescriptionModal();
        }
    });

    // Add ESC key to close
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeDescriptionModal();
        }
    });
}

function showDescriptionModal(title, description) {
    const modal = document.getElementById('descriptionModal');
    const modalTitle = document.querySelector('.description-modal-title');
    const modalBody = document.getElementById('descriptionModalBody');

    modalTitle.textContent = title ? `Mô tả: ${title}` : 'Mô tả sản phẩm';
    modalBody.innerHTML = description || 'Không có mô tả';

    modal.classList.add('show');
    document.body.style.overflow = 'hidden';
}

function closeDescriptionModal() {
    const modal = document.getElementById('descriptionModal');
    if (modal) {
        modal.classList.remove('show');
        document.body.style.overflow = '';
    }
}

// Bulk Actions Functions
function toggleBulkActions() {
    bulkMode = !bulkMode;
    const checkboxes = document.querySelectorAll('.bulk-checkbox');
    const bulkPanel = document.getElementById('bulkActionsPanel');
    const toggleButton = document.getElementById('bulkToggle');

    if (bulkMode) {
        // Show bulk mode
        checkboxes.forEach(cb => cb.style.display = 'inline-block');
        bulkPanel.style.display = 'block';

        // Trigger animation
        setTimeout(() => {
            bulkPanel.classList.add('show');
        }, 10);

        toggleButton.innerHTML = '<i class="fas fa-times me-1"></i>Hủy chọn';
        toggleButton.className = 'btn btn-outline-danger btn-sm';

    } else {
        // Hide bulk mode
        bulkPanel.classList.remove('show');

        setTimeout(() => {
            bulkPanel.style.display = 'none';
            checkboxes.forEach(cb => {
                cb.style.display = 'none';
                cb.checked = false;
            });
        }, 300);

        toggleButton.innerHTML = '<i class="fas fa-check-square me-1"></i>Chọn nhiều';
        toggleButton.className = 'btn btn-outline-warning btn-sm';
        updateSelectedCount();
    }
}

function toggleSelectAll() {
    const selectAll = document.getElementById('selectAll');
    const productCheckboxes = document.querySelectorAll('.product-checkbox');

    productCheckboxes.forEach(cb => cb.checked = selectAll.checked);
    updateSelectedCount();
}

function updateSelectedCount() {
    const selectedCheckboxes = document.querySelectorAll('.product-checkbox:checked');
    const countElement = document.getElementById('selectedCount');
    const newCount = selectedCheckboxes.length;

    // Animate count change
    countElement.style.transform = 'scale(1.3)';
    countElement.textContent = newCount;

    setTimeout(() => {
        countElement.style.transform = 'scale(1)';
    }, 200);

    // Update select all checkbox
    const selectAll = document.getElementById('selectAll');
    const allCheckboxes = document.querySelectorAll('.product-checkbox');
    if (selectAll && allCheckboxes.length > 0) {
        selectAll.checked = selectedCheckboxes.length === allCheckboxes.length;
    }
}

async function bulkAction(action) {
    const selectedIds = Array.from(document.querySelectorAll('.product-checkbox:checked'))
        .map(cb => cb.value);

    if (selectedIds.length === 0) {
        // Shake animation for panel
        const panel = document.getElementById('bulkActionsPanel');
        panel.style.animation = 'shake 0.5s ease-in-out';
        setTimeout(() => {
            panel.style.animation = '';
        }, 500);

        await CustomModal.warning('Vui lòng chọn ít nhất một sản phẩm', 'Không có sản phẩm nào được chọn');
        return;
    }

    let confirmed = false;

    // Use custom modal based on action
    switch (action) {
        case 'delete':
            confirmed = await CustomModal.bulkDelete(selectedIds.length);
            break;
        case 'sethot':
        case 'setbestseller':
            confirmed = await CustomModal.bulkAction(action, selectedIds.length);
            break;
        default:
            confirmed = await CustomModal.confirm(
                `Bạn có chắc chắn muốn thực hiện hành động với ${selectedIds.length} sản phẩm đã chọn?`,
                'Xác nhận hành động'
            );
    }

    if (confirmed) {
        // Show loading state
        const actionButton = document.querySelector(`[onclick="bulkAction('${action}')"]`);
        const originalContent = actionButton.innerHTML;
        actionButton.innerHTML = '<span class="custom-modal-spinner"></span>Đang xử lý...';
        actionButton.disabled = true;

        // Create and submit form
        const form = document.createElement('form');
        form.method = 'POST';

        let actionUrl = '';
        switch (action) {
            case 'delete':
                actionUrl = '/AdminProducts/BulkDelete';
                break;
            case 'sethot':
            case 'setbestseller':
                actionUrl = '/AdminProducts/BulkUpdateStatus';
                break;
        }
        form.action = actionUrl;

        // Add anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (token) {
            const tokenInput = document.createElement('input');
            tokenInput.type = 'hidden';
            tokenInput.name = '__RequestVerificationToken';
            tokenInput.value = token.value;
            form.appendChild(tokenInput);
        }

        // Add selected IDs
        selectedIds.forEach(id => {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = 'selectedIds';
            input.value = id;
            form.appendChild(input);
        });

        // Add action if not delete
        if (action !== 'delete') {
            const actionInput = document.createElement('input');
            actionInput.type = 'hidden';
            actionInput.name = 'action';
            actionInput.value = action;
            form.appendChild(actionInput);
        }

        document.body.appendChild(form);
        form.submit();
    }
}

// Export to Excel Function
async function exportToExcel() {
    try {
        // Get current filter values
        const searchTerm = document.querySelector('input[name="searchTerm"]')?.value || '';
        const productId = document.querySelector('input[name="productId"]')?.value || '';
        const categoryId = document.querySelector('select[name="categoryId"]')?.value || '';
        const brandId = document.querySelector('select[name="brandId"]')?.value || '';
        const priceRange = document.querySelector('select[name="priceRange"]')?.value || '';
        const status = document.querySelector('select[name="status"]')?.value || '';

        // Build URL with filters
        let url = '/AdminProducts/ExportToExcel?';
        const params = [];

        if (searchTerm) params.push(`searchTerm=${encodeURIComponent(searchTerm)}`);
        if (productId) params.push(`productId=${productId}`);
        if (categoryId) params.push(`categoryId=${categoryId}`);
        if (brandId) params.push(`brandId=${brandId}`);
        if (priceRange) params.push(`priceRange=${priceRange}`);
        if (status) params.push(`status=${status}`);

        url += params.join('&');

        // Create temporary link and click it
        const link = document.createElement('a');
        link.href = url;
        link.download = `products_${new Date().getTime()}.csv`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        // Show success message
        setTimeout(async () => {
            await CustomModal.success('File Excel đã được tải xuống thành công!', 'Xuất Excel thành công');
        }, 1000);

    } catch (error) {
        console.error('Error exporting to Excel:', error);
        await CustomModal.warning('Có lỗi xảy ra khi xuất file Excel. Vui lòng thử lại.', 'Lỗi xuất Excel');
    }
}

// Toast Notifications
function initializeToasts() {
    const successToast = document.getElementById('successToast');
    const errorToast = document.getElementById('errorToast');

    if (successToast && window.bootstrap) {
        const bsSuccessToast = new bootstrap.Toast(successToast);
        // Check if there's a success message
        if (successToast.querySelector('.toast-body').textContent.trim()) {
            bsSuccessToast.show();
        }
    }

    if (errorToast && window.bootstrap) {
        const bsErrorToast = new bootstrap.Toast(errorToast);
        // Check if there's an error message
        if (errorToast.querySelector('.toast-body').textContent.trim()) {
            bsErrorToast.show();
        }
    }
}

// Utility Functions
function addShakeAnimation() {
    // Add shake animation keyframes if not already added
    if (!document.getElementById('shakeAnimation')) {
        const style = document.createElement('style');
        style.id = 'shakeAnimation';
        style.textContent = `
            @keyframes shake {
                0%, 20%, 40%, 60%, 80%, 100% { 
                    transform: translateX(0) translateY(0); 
                }
                10%, 30%, 50%, 70%, 90% { 
                    transform: translateX(-10px) translateY(-2px); 
                }
            }
        `;
        document.head.appendChild(style);
    }
}

// Form Helpers
function resetAllFilters() {
    const form = document.querySelector('form[method="get"]');
    if (form) {
        // Reset all form elements
        form.reset();
        // Submit the form to refresh with no filters
        form.submit();
    }
}

// Initialize animations
addShakeAnimation();

// Make functions globally available
window.CustomModal = CustomModal;
window.showDescriptionModal = showDescriptionModal;
window.closeDescriptionModal = closeDescriptionModal;
window.toggleBulkActions = toggleBulkActions;
window.toggleSelectAll = toggleSelectAll;
window.updateSelectedCount = updateSelectedCount;
window.bulkAction = bulkAction;
window.exportToExcel = exportToExcel;
window.resetAllFilters = resetAllFilters;