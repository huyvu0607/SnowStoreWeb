/*
Admin Panel JavaScript
File: wwwroot/js/admin.js
*/

document.addEventListener('DOMContentLoaded', function () {
    // Initialize admin panel
    initSidebar();
    initTooltips();
    initConfirmDialogs();
    initTableFeatures();
    initFormValidation();
});

// Sidebar functionality
function initSidebar() {
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('adminSidebar');
    const overlay = document.getElementById('sidebarOverlay');
    const body = document.body;

    if (sidebarToggle && sidebar && overlay) {
        // Toggle sidebar
        sidebarToggle.addEventListener('click', function () {
            toggleSidebar();
        });

        // Close sidebar when clicking overlay
        overlay.addEventListener('click', function () {
            closeSidebar();
        });

        // Handle window resize
        window.addEventListener('resize', function () {
            if (window.innerWidth >= 992) {
                closeSidebar();
            }
        });

        // Handle escape key
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && sidebar.classList.contains('show')) {
                closeSidebar();
            }
        });
    }

    function toggleSidebar() {
        sidebar.classList.toggle('show');
        overlay.classList.toggle('show');
        body.classList.toggle('sidebar-open');
    }

    function closeSidebar() {
        sidebar.classList.remove('show');
        overlay.classList.remove('show');
        body.classList.remove('sidebar-open');
    }

    // Auto-close sidebar on mobile when clicking nav links
    const navLinks = sidebar.querySelectorAll('.nav-link');
    navLinks.forEach(link => {
        link.addEventListener('click', function () {
            if (window.innerWidth < 992) {
                setTimeout(closeSidebar, 150);
            }
        });
    });
}

// Initialize tooltips
function initTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Confirmation dialogs
/*function initConfirmDialogs() {
    const deleteButtons = document.querySelectorAll('.btn-delete, .delete-btn');

    deleteButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();

            const message = this.dataset.message || 'Bạn có chắc chắn muốn xóa?';
            const title = this.dataset.title || 'Xác nhận xóa';

            showConfirmDialog(title, message, () => {
                // If it's a form, submit it
                const form = this.closest('form');
                if (form) {
                    form.submit();
                } else if (this.href) {
                    // If it's a link, navigate to it
                    window.location.href = this.href;
                }
            });
        });
    });
}*/

// Custom confirm dialog
function showConfirmDialog(title, message, onConfirm, onCancel = null) {
    const modal = document.createElement('div');
    modal.className = 'modal fade';
    modal.innerHTML = `
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">${title}</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <p>${message}</p>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
                    <button type="button" class="btn btn-danger" id="confirmBtn">Xác nhận</button>
                </div>
            </div>
        </div>
    `;

    document.body.appendChild(modal);
    const bootstrapModal = new bootstrap.Modal(modal);

    modal.querySelector('#confirmBtn').addEventListener('click', function () {
        onConfirm();
        bootstrapModal.hide();
    });

    modal.addEventListener('hidden.bs.modal', function () {
        document.body.removeChild(modal);
        if (onCancel) onCancel();
    });

    bootstrapModal.show();
}

// Table features
function initTableFeatures() {
    // Row selection
    initRowSelection();

    // Search functionality
    initTableSearch();

    // Sorting
    initTableSort();
}

// Row selection for batch operations
function initRowSelection() {
    const selectAllCheckbox = document.querySelector('#selectAll');
    const rowCheckboxes = document.querySelectorAll('.row-checkbox');
    const batchActions = document.querySelector('.batch-actions');

    if (selectAllCheckbox && rowCheckboxes.length > 0) {
        selectAllCheckbox.addEventListener('change', function () {
            rowCheckboxes.forEach(checkbox => {
                checkbox.checked = this.checked;
            });
            toggleBatchActions();
        });

        rowCheckboxes.forEach(checkbox => {
            checkbox.addEventListener('change', function () {
                const checkedCount = document.querySelectorAll('.row-checkbox:checked').length;
                selectAllCheckbox.indeterminate = checkedCount > 0 && checkedCount < rowCheckboxes.length;
                selectAllCheckbox.checked = checkedCount === rowCheckboxes.length;
                toggleBatchActions();
            });
        });
    }

    function toggleBatchActions() {
        const checkedCount = document.querySelectorAll('.row-checkbox:checked').length;
        if (batchActions) {
            batchActions.style.display = checkedCount > 0 ? 'block' : 'none';
        }
    }
}

// Simple table search
function initTableSearch() {
    const searchInput = document.querySelector('.table-search');
    const table = document.querySelector('.searchable-table');

    if (searchInput && table) {
        searchInput.addEventListener('input', function () {
            const searchTerm = this.value.toLowerCase();
            const rows = table.querySelectorAll('tbody tr');

            rows.forEach(row => {
                const text = row.textContent.toLowerCase();
                row.style.display = text.includes(searchTerm) ? '' : 'none';
            });

            updateTableInfo();
        });
    }
}

// Simple table sorting
function initTableSort() {
    const sortableHeaders = document.querySelectorAll('.sortable');

    sortableHeaders.forEach(header => {
        header.addEventListener('click', function () {
            const table = this.closest('table');
            const tbody = table.querySelector('tbody');
            const rows = Array.from(tbody.querySelectorAll('tr'));
            const column = this.cellIndex;
            const currentSort = this.dataset.sort || 'none';

            // Reset other headers
            sortableHeaders.forEach(h => {
                if (h !== this) {
                    h.dataset.sort = 'none';
                    h.querySelector('.sort-icon')?.remove();
                }
            });

            // Determine new sort direction
            let newSort;
            if (currentSort === 'none' || currentSort === 'desc') {
                newSort = 'asc';
            } else {
                newSort = 'desc';
            }

            // Sort rows
            rows.sort((a, b) => {
                const aVal = a.cells[column].textContent.trim();
                const bVal = b.cells[column].textContent.trim();

                let comparison = 0;
                if (isNumeric(aVal) && isNumeric(bVal)) {
                    comparison = parseFloat(aVal) - parseFloat(bVal);
                } else {
                    comparison = aVal.localeCompare(bVal, 'vi');
                }

                return newSort === 'asc' ? comparison : -comparison;
            });

            // Re-append sorted rows
            rows.forEach(row => tbody.appendChild(row));

            // Update header
            this.dataset.sort = newSort;
            updateSortIcon(this, newSort);
        });
    });
}

function isNumeric(str) {
    return !isNaN(str) && !isNaN(parseFloat(str));
}

function updateSortIcon(header, direction) {
    // Remove existing icon
    header.querySelector('.sort-icon')?.remove();

    // Add new icon
    const icon = document.createElement('i');
    icon.className = `bi bi-arrow-${direction === 'asc' ? 'up' : 'down'} sort-icon ms-1`;
    header.appendChild(icon);
}

// Form validation enhancements
function initFormValidation() {
    const forms = document.querySelectorAll('.needs-validation');

    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            if (!form.checkValidity()) {
                e.preventDefault();
                e.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });

    // Auto-save drafts (optional)
    initAutoSave();
}

// Auto-save functionality
function initAutoSave() {
    const autoSaveForms = document.querySelectorAll('.auto-save');

    autoSaveForms.forEach(form => {
        const formId = form.id;
        if (!formId) return;

        // Load saved data
        loadFormData(form, formId);

        // Save on input
        const inputs = form.querySelectorAll('input, textarea, select');
        inputs.forEach(input => {
            input.addEventListener('input', debounce(() => {
                saveFormData(form, formId);
            }, 1000));
        });

        // Clear saved data on successful submit
        form.addEventListener('submit', function () {
            localStorage.removeItem(`form_${formId}`);
        });
    });
}

function saveFormData(form, formId) {
    const formData = new FormData(form);
    const data = {};

    for (let [key, value] of formData.entries()) {
        data[key] = value;
    }

    localStorage.setItem(`form_${formId}`, JSON.stringify(data));
    showAutoSaveIndicator();
}

function loadFormData(form, formId) {
    const savedData = localStorage.getItem(`form_${formId}`);
    if (!savedData) return;

    try {
        const data = JSON.parse(savedData);
        Object.keys(data).forEach(key => {
            const input = form.querySelector(`[name="${key}"]`);
            if (input && input.type !== 'file') {
                input.value = data[key];
            }
        });
    } catch (e) {
        console.error('Error loading form data:', e);
    }
}

function showAutoSaveIndicator() {
    // Simple auto-save indicator
    let indicator = document.querySelector('.auto-save-indicator');
    if (!indicator) {
        indicator = document.createElement('div');
        indicator.className = 'auto-save-indicator position-fixed bottom-0 end-0 m-3 alert alert-success alert-dismissible';
        indicator.innerHTML = '<i class="bi bi-check-circle me-2"></i>Đã tự động lưu';
        document.body.appendChild(indicator);
    }

    indicator.style.display = 'block';
    setTimeout(() => {
        indicator.style.display = 'none';
    }, 2000);
}

// Utility functions
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

function updateTableInfo() {
    const tableInfo = document.querySelector('.table-info');
    if (!tableInfo) return;

    const table = document.querySelector('.searchable-table');
    const visibleRows = table.querySelectorAll('tbody tr:not([style*="display: none"])');
    const totalRows = table.querySelectorAll('tbody tr');

    tableInfo.textContent = `Hiển thị ${visibleRows.length} / ${totalRows.length} bản ghi`;
}

// Show loading state
function showLoading(element) {
    element.classList.add('loading');
}

function hideLoading(element) {
    element.classList.remove('loading');
}

// Toast notifications
function showToast(message, type = 'success') {
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type} border-0`;
    toast.setAttribute('role', 'alert');
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;

    let container = document.querySelector('.toast-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(container);
    }

    container.appendChild(toast);
    const bootstrapToast = new bootstrap.Toast(toast);
    bootstrapToast.show();

    toast.addEventListener('hidden.bs.toast', function () {
        container.removeChild(toast);
    });
}