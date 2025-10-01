// FIXED VERSION - Giải quyết vấn đề ảnh phụ không submit được
// ========================================
// GLOBAL VARIABLES
// ========================================
let additionalFilesArray = [];
let sortableInstance = null;
let dragCounterMain = 0;
let dragCounterAdditional = 0;
let isProcessingDrop = false;
let lastDropTime = 0; // ← THÊM DÒNG NÀY

// ========================================
// INITIALIZE
// ========================================
document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Initializing Product Form...');
    initMainImage();
    initAdditionalImages();
    initFormValidation();
    addCustomStyles();
});

// ========================================
// PHẦN 1: MAIN IMAGE (ẢNH CHÍNH)
// ========================================
function initMainImage() {
    console.log('📸 Init Main Image');
    const input = document.getElementById('MainImageFile');
    const placeholder = document.getElementById('mainImagePlaceholder');

    if (!input || !placeholder) {
        console.error('❌ Main image elements not found');
        return;
    }

    input.addEventListener('change', handleMainImageSelect);
    setupMainImageDragDrop(placeholder, input);
}

function handleMainImageSelect(e) {
    const file = e.target.files[0];
    console.log('📸 Main image selected:', file?.name);

    if (!file) {
        resetMainPreview();
        return;
    }

    if (!validateImage(file)) {
        e.target.value = '';
        resetMainPreview();
        return;
    }

    showMainPreview(file);
}

function setupMainImageDragDrop(zone, input) {
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(evt => {
        zone.addEventListener(evt, e => {
            e.preventDefault();
            e.stopPropagation();
        });
    });

    zone.addEventListener('dragenter', () => {
        dragCounterMain++;
        zone.classList.add('drag-active-main');
    });

    zone.addEventListener('dragleave', () => {
        dragCounterMain--;
        if (dragCounterMain === 0) {
            zone.classList.remove('drag-active-main');
        }
    });

    zone.addEventListener('drop', (e) => {
        dragCounterMain = 0;
        zone.classList.remove('drag-active-main');

        const files = e.dataTransfer.files;
        if (files.length > 0) {
            const file = files[0];
            if (validateImage(file)) {
                const dt = new DataTransfer();
                dt.items.add(file);
                input.files = dt.files;
                showMainPreview(file);
                showSuccessFlash(zone);
            }
        }
    });
}

function showMainPreview(file) {
    const preview = document.getElementById('mainImagePreview');
    const placeholder = document.getElementById('mainImagePlaceholder');
    const badge = document.getElementById('mainImageStatus');

    const reader = new FileReader();
    reader.onload = (e) => {
        preview.src = e.target.result;
        preview.style.display = 'block';
        placeholder.style.display = 'none';

        if (badge) {
            badge.textContent = 'ĐÃ CHỌN';
            badge.className = 'badge bg-success';
        }
    };
    reader.readAsDataURL(file);
}

function resetMainPreview() {
    const preview = document.getElementById('mainImagePreview');
    const placeholder = document.getElementById('mainImagePlaceholder');
    const badge = document.getElementById('mainImageStatus');

    if (preview) {
        preview.style.display = 'none';
        preview.src = '#';
    }
    if (placeholder) {
        placeholder.style.display = 'flex';
    }
    if (badge) {
        badge.textContent = 'CHƯA CHỌN';
        badge.className = 'badge bg-secondary';
    }
}

// ========================================
// PHẦN 2: ADDITIONAL IMAGES (ẢNH PHỤ)
// ========================================
function initAdditionalImages() {
    console.log('🖼️ Init Additional Images');
    const input = document.getElementById('AdditionalImageFiles');
    const container = document.getElementById('additionalImagesPreview');

    if (!input || !container) {
        console.error('❌ Additional images elements not found');
        return;
    }

    createAdditionalDropZone(container);
    input.addEventListener('change', handleAdditionalSelect);
    setupAdditionalDragDrop();
}

function createAdditionalDropZone(container) {
    const zone = document.createElement('div');
    zone.className = 'col-12 mb-3';
    zone.innerHTML = `
        <div id="additionalDropZone" 
             class="border border-2 border-dashed rounded bg-light additional-zone"
             style="min-height: 120px; cursor: pointer; padding: 20px;">
            <div class="text-center text-muted">
                <i class="fas fa-images fa-3x mb-2 text-info"></i>
                <p class="mb-1 fw-bold">ẢNH PHỤ - Kéo thả hoặc click</p>
                <small>Tối đa 5 ảnh (JPG, PNG, GIF, WEBP - Max 5MB)</small>
            </div>
        </div>
    `;

    container.insertBefore(zone, container.firstChild);

    const dropZone = document.getElementById('additionalDropZone');
    dropZone.addEventListener('click', () => {
        document.getElementById('AdditionalImageFiles').click();
    });
}

// ========================================
// QUAN TRỌNG: Xử lý Drag & Drop cho ảnh phụ
// ========================================
function setupAdditionalDragDrop() {
    const zone = document.getElementById('additionalDropZone');
    const input = document.getElementById('AdditionalImageFiles');
    if (!zone || !input) return;

    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(evt => {
        zone.addEventListener(evt, e => {
            e.preventDefault();
            e.stopPropagation();
        });
    });

    zone.addEventListener('dragenter', () => {
        dragCounterAdditional++;
        zone.classList.add('drag-active-additional');
    });

    zone.addEventListener('dragleave', () => {
        dragCounterAdditional--;
        if (dragCounterAdditional === 0) {
            zone.classList.remove('drag-active-additional');
        }
    });

    zone.addEventListener('drop', (e) => {
        console.log('💧 DROP EVENT triggered');
        dragCounterAdditional = 0;
        zone.classList.remove('drag-active-additional');

        const files = Array.from(e.dataTransfer.files);

        if (files.length > 0) {
            console.log('📥 Processing drop with', files.length, 'files');

            // Lưu timestamp và set flag NGAY
            lastDropTime = Date.now();
            isProcessingDrop = true;
            console.log('🔒 Drop processing LOCKED at', lastDropTime);

            // Clear input value NGAY và force blur
            input.value = '';
            input.blur();

            // Xử lý files ngay lập tức
            addAdditionalImages(files);
            showSuccessFlash(zone);

            // Unlock sau 1.5 giây
            setTimeout(() => {
                isProcessingDrop = false;
                console.log('🔓 Drop processing UNLOCKED');
            }, 300);
        }
    });
}

// ========================================
// QUAN TRỌNG: Xử lý khi chọn file từ dialog
// ========================================
function handleAdditionalSelect(e) {
    const now = Date.now();
    console.log('📂 CHANGE EVENT triggered at', now);
    console.log('🔍 isProcessingDrop:', isProcessingDrop);
    console.log('⏱️ Time since last drop:', now - lastDropTime, 'ms');

    // Nếu đang xử lý drop HOẶC change xảy ra trong vòng 1.5s sau drop → BỎ QUA
    if (isProcessingDrop) {
        console.log('⛔ Change event BLOCKED - drop is processing');
        e.preventDefault();
        e.stopPropagation();
        e.target.value = '';
        e.target.blur();
        return false;
    }

    const files = Array.from(e.target.files);
    console.log('🖼️ Files selected from dialog:', files.length);
    console.log('📱 Device info:', {
        userAgent: navigator.userAgent,
        platform: navigator.platform
    });

    if (files.length === 0) {
        console.log('⚠️ No files selected');
        return;
    }

    // Log thông tin chi tiết từng file (debug cho mobile)
    files.forEach((f, i) => {
        console.log(`File ${i}:`, {
            name: f.name,
            size: f.size,
            type: f.type,
            lastModified: f.lastModified
        });
    });

    const remaining = 5 - additionalFilesArray.length;

    if (remaining <= 0) {
        showAlert('Đã đủ 5 ảnh rồi!', 'warning');
        e.target.value = '';
        syncAdditionalInput();
        return;
    }

    // Lấy số lượng file tối đa có thể thêm
    let filesToAdd = files.slice(0, Math.min(files.length, remaining));

    let added = 0;
    let skipped = 0;

    filesToAdd.forEach(file => {
        // Kiểm tra file có hợp lệ không
        const isValid = validateImage(file);
        console.log(`Validating ${file.name}:`, isValid);

        if (!isValid) {
            console.log(`❌ File không hợp lệ: ${file.name}`);
            skipped++;
            return;
        }

        // Tìm file trùng - CHỈ dùng name + size (bỏ lastModified vì không ổn định trên mobile)
        const dupeIndex = additionalFilesArray.findIndex(f =>
            f.name === file.name && f.size === file.size
        );

        if (dupeIndex !== -1) {
            // Xóa file cũ
            additionalFilesArray.splice(dupeIndex, 1);
            console.log(`🔄 Replaced old file: ${file.name}`);
        }

        // Thêm file mới vào cuối
        additionalFilesArray.push(file);
        added++;
        console.log(`✅ Added: ${file.name}`);
    });

    // Clear input để có thể chọn lại
    e.target.value = '';

    if (added > 0) {
        // Sync lại input với array đầy đủ
        syncAdditionalInput();
        renderAllPreviews();
        updateBadge();
        showAlert(`✅ Đã thêm ${added} ảnh${skipped > 0 ? ` (bỏ qua ${skipped} ảnh trùng/lỗi)` : ''}`, 'success');
    } else if (skipped > 0) {
        showAlert('Tất cả ảnh đã tồn tại hoặc không hợp lệ', 'warning');
    }

    console.log('📊 Total images now:', additionalFilesArray.length);
}

// ========================================
// Thêm ảnh vào array
// ========================================
function addAdditionalImages(newFiles) {
    console.log('➕ Adding images. Current:', additionalFilesArray.length, 'New:', newFiles.length);

    const remaining = 5 - additionalFilesArray.length;

    if (remaining <= 0) {
        showAlert('Đã đủ 5 ảnh rồi!', 'warning');
        return;
    }

    if (newFiles.length > remaining) {
        showAlert(`Chỉ thêm được ${remaining} ảnh nữa`, 'warning');
        newFiles = newFiles.slice(0, remaining);
    }

    let added = 0;
    let skipped = 0;

    newFiles.forEach(file => {
        if (!validateImage(file)) {
            skipped++;
            return;
        }

        // Tìm file trùng
        const dupeIndex = additionalFilesArray.findIndex(f =>
            f.name === file.name && f.size === file.size
        );

        if (dupeIndex !== -1) {
            // Xóa file cũ
            additionalFilesArray.splice(dupeIndex, 1);
            console.log(`🔄 Replaced: ${file.name}`);
        }

        // Thêm file mới vào cuối
        additionalFilesArray.push(file);
        added++;
    });

    if (added > 0) {
        syncAdditionalInput();
        renderAllPreviews();
        updateBadge();
        showAlert(`✅ Đã thêm ${added} ảnh${skipped > 0 ? ` (${skipped} bỏ qua)` : ''}`, 'success');
    } else {
        showAlert('Không có ảnh hợp lệ', 'danger');
    }

    console.log('📊 Total now:', additionalFilesArray.length);
}

// ========================================
// QUAN TRỌNG: Sync input với array
// ========================================
function syncAdditionalInput() {
    const input = document.getElementById('AdditionalImageFiles');
    if (!input) return;

    console.log('🔄 Syncing input with', additionalFilesArray.length, 'files');

    const dt = new DataTransfer();
    additionalFilesArray.forEach(file => {
        dt.items.add(file);
    });

    input.files = dt.files;

    console.log('✅ Input synced. Files in input:', input.files.length);
}

// ========================================
// RENDER PREVIEWS
// ========================================
function renderAllPreviews() {
    const container = document.getElementById('additionalImagesPreview');
    if (!container) return;

    container.querySelectorAll('.preview-item').forEach(el => el.remove());

    const placeholder = document.getElementById('noImagesPlaceholder');
    if (placeholder) {
        placeholder.style.display = additionalFilesArray.length > 0 ? 'none' : 'block';
    }

    additionalFilesArray.forEach((file, idx) => {
        renderPreview(file, idx, container);
    });

    if (additionalFilesArray.length > 0) {
        setTimeout(() => initSortable(), 300);
    }
}

function renderPreview(file, idx, container) {
    const reader = new FileReader();
    reader.onload = (e) => {
        const col = document.createElement('div');
        col.className = 'col-6 preview-item';
        col.dataset.index = idx;
        col.innerHTML = `
            <div class="card border-primary h-100">
                <img src="${e.target.result}" class="card-img-top" 
                     style="height: 100px; object-fit: cover;">
                <div class="card-body p-2">
                    <div class="d-flex justify-content-between align-items-center">
                        <span class="badge bg-primary order-badge">#${idx + 1}</span>
                        <button type="button" class="btn btn-sm btn-danger" 
                                onclick="removeImage(${idx})">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                    <small class="text-muted d-block text-truncate mt-1" 
                           title="${file.name}">${file.name}</small>
                </div>
            </div>
        `;

        col.style.opacity = '0';
        container.appendChild(col);
        setTimeout(() => {
            col.style.transition = 'opacity 0.3s';
            col.style.opacity = '1';
        }, idx * 50);
    };
    reader.readAsDataURL(file);
}

// ========================================
// SORTABLE
// ========================================
function initSortable() {
    if (sortableInstance) {
        sortableInstance.destroy();
    }

    const container = document.getElementById('additionalImagesPreview');
    sortableInstance = new Sortable(container, {
        animation: 200,
        ghostClass: 'ghost',
        filter: '#additionalDropZone, #noImagesPlaceholder',
        onEnd: () => {
            reorderFiles();
            syncAdditionalInput();
            updateOrderLabels();
        }
    });
}

function reorderFiles() {
    const items = document.querySelectorAll('.preview-item');
    const newOrder = [];

    items.forEach(item => {
        const oldIdx = parseInt(item.dataset.index);
        if (additionalFilesArray[oldIdx]) {
            newOrder.push(additionalFilesArray[oldIdx]);
        }
    });

    additionalFilesArray = newOrder;
    console.log('🔄 Reordered:', additionalFilesArray.map(f => f.name));
}

function updateOrderLabels() {
    document.querySelectorAll('.preview-item').forEach((item, idx) => {
        item.dataset.index = idx;
        const badge = item.querySelector('.order-badge');
        const btn = item.querySelector('button');

        if (badge) badge.textContent = `#${idx + 1}`;
        if (btn) btn.setAttribute('onclick', `removeImage(${idx})`);
    });
}

// ========================================
// REMOVE IMAGE
// ========================================
function removeImage(idx) {
    if (!confirm('Xóa ảnh này?')) return;

    console.log('🗑️ Removing index:', idx);

    const item = document.querySelector(`.preview-item[data-index="${idx}"]`);
    if (item) {
        item.style.opacity = '0';
        setTimeout(() => {
            item.remove();
            additionalFilesArray.splice(idx, 1);
            syncAdditionalInput();
            updateOrderLabels();
            updateBadge();

            if (additionalFilesArray.length === 0) {
                showNoImagesPlaceholder();
                if (sortableInstance) {
                    sortableInstance.destroy();
                    sortableInstance = null;
                }
            }
        }, 300);
    }
}

// ========================================
// UPDATE BADGE
// ========================================
function updateBadge() {
    const badge = document.getElementById('additionalImageCount');
    if (!badge) return;

    const count = additionalFilesArray.length;
    badge.textContent = count === 5 ? '5/5 ĐẦY' : `${count}/5`;
    badge.className = count === 0 ? 'badge bg-secondary' :
        count === 5 ? 'badge bg-success' : 'badge bg-info';
}

function showNoImagesPlaceholder() {
    const ph = document.getElementById('noImagesPlaceholder');
    if (ph) ph.style.display = 'block';
}

// ========================================
// VALIDATION
// ========================================
function validateImage(file) {
    const maxSize = 5 * 1024 * 1024;
    const types = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];

    if (!types.includes(file.type)) {
        showAlert(`❌ "${file.name}" không đúng định dạng`, 'danger');
        return false;
    }

    if (file.size > maxSize) {
        showAlert(`❌ "${file.name}" quá lớn (>5MB)`, 'danger');
        return false;
    }

    return true;
}

// ========================================
// FORM VALIDATION & SUBMIT
// ========================================
function initFormValidation() {
    const form = document.getElementById('createProductForm');
    if (!form) return;

    form.addEventListener('submit', (e) => {
        console.log('📤 Form submitting...');
        console.log('Additional images:', additionalFilesArray.length);

        const input = document.getElementById('AdditionalImageFiles');
        console.log('Input files:', input.files.length);

        syncAdditionalInput();

        if (!validateForm()) {
            e.preventDefault();
            return false;
        }

        showLoading();
    });
}

function validateForm() {
    let valid = true;
    const required = [
        { id: 'CategoryId', name: 'Danh mục' },
        { id: 'BrandId', name: 'Thương hiệu' },
        { id: 'Name', name: 'Tên sản phẩm' },
        { id: 'Price', name: 'Giá' }
    ];

    document.querySelectorAll('.is-invalid').forEach(el => {
        el.classList.remove('is-invalid');
    });

    required.forEach(field => {
        const el = document.getElementById(field.id);
        if (!el?.value?.trim()) {
            el.classList.add('is-invalid');
            showAlert(`${field.name} là bắt buộc`, 'danger');
            valid = false;
        }
    });

    const price = document.getElementById('Price');
    if (price && parseFloat(price.value) <= 0) {
        price.classList.add('is-invalid');
        showAlert('Giá phải > 0', 'danger');
        valid = false;
    }

    return valid;
}

function showLoading() {
    const btn = document.getElementById('saveButton');
    if (btn) {
        btn.disabled = true;
        btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang lưu...';
    }
}

// ========================================
// RESET FORM
// ========================================
function resetForm() {
    document.getElementById('createProductForm')?.reset();
    resetMainPreview();
    resetAdditional();

    document.querySelectorAll('.is-invalid').forEach(el => {
        el.classList.remove('is-invalid');
    });

    const btn = document.getElementById('saveButton');
    if (btn) {
        btn.disabled = false;
        btn.innerHTML = '<i class="fas fa-save"></i> Lưu Sản Phẩm';
    }
}

function resetAdditional() {
    document.querySelectorAll('.preview-item').forEach(el => el.remove());
    additionalFilesArray = [];

    const input = document.getElementById('AdditionalImageFiles');
    if (input) input.value = '';

    updateBadge();
    showNoImagesPlaceholder();

    if (sortableInstance) {
        sortableInstance.destroy();
        sortableInstance = null;
    }
}

// ========================================
// UI HELPERS
// ========================================
function showAlert(msg, type = 'info') {
    document.querySelectorAll('.alert').forEach(a => {
        if (a.querySelector('.btn-close')) a.remove();
    });

    const icons = {
        success: 'check-circle',
        danger: 'exclamation-triangle',
        warning: 'exclamation-triangle',
        info: 'info-circle'
    };

    const html = `
        <div class="alert alert-${type} alert-dismissible fade show">
            <i class="fas fa-${icons[type]}"></i> ${msg}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    document.querySelector('.container-fluid')?.insertAdjacentHTML('afterbegin', html);

    setTimeout(() => {
        document.querySelector('.alert')?.remove();
    }, 5000);
}

function showSuccessFlash(el) {
    el.style.background = '#d4edda';
    el.style.borderColor = '#28a745';
    setTimeout(() => {
        el.style.background = '';
        el.style.borderColor = '';
    }, 800);
}

// ========================================
// STYLES
// ========================================
function addCustomStyles() {
    const css = `
    <style>
    .drag-active-main {
        border-color: #0d6efd !important;
        background: rgba(13, 110, 253, 0.1) !important;
        transform: scale(1.02);
        box-shadow: 0 0 20px rgba(13, 110, 253, 0.3);
    }

    .drag-active-additional {
        border-color: #17a2b8 !important;
        background: rgba(23, 162, 184, 0.1) !important;
        transform: scale(1.02);
        box-shadow: 0 0 20px rgba(23, 162, 184, 0.3);
    }

    .additional-zone {
        transition: all 0.3s ease;
    }

    .additional-zone:hover {
        border-color: #6c757d !important;
        background: #f8f9fa !important;
    }

    .preview-item .card {
        cursor: grab;
        transition: all 0.2s;
    }

    .preview-item .card:hover {
        transform: translateY(-3px);
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    }

    .ghost {
        opacity: 0.4;
    }

    .is-invalid {
        border-color: #dc3545 !important;
        box-shadow: 0 0 0 0.25rem rgba(220, 53, 69, 0.25) !important;
    }

    @media (max-width: 768px) {
        .additional-zone {
            min-height: 100px !important;
        }
    }
    </style>
    `;
    document.head.insertAdjacentHTML('beforeend', css);
}

// ========================================
// EXPOSE GLOBAL
// ========================================
window.removeImage = removeImage;
window.resetForm = resetForm;

console.log('✅ Product Form Loaded - FIXED VERSION');
