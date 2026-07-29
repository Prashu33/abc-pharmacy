// app.js - Main Application Coordinator
import * as api from './api.js';
import { renderMedicinesGrid } from './grid.js';

// DOM Elements
const searchInput = document.getElementById('search-input');
const inventoryBody = document.getElementById('inventory-body');
const saleMedicineSelect = document.getElementById('sale-medicine-select');
const saleQtyInput = document.getElementById('sale-qty');
const saleTotalAmount = document.getElementById('sale-total-amount');
const recordSaleForm = document.getElementById('record-sale-form');
const addMedicineForm = document.getElementById('add-medicine-form');
const salesHistoryBody = document.getElementById('sales-history-body');

// Detail Modal
const detailModal = document.getElementById('detail-modal');
const modalTitle = document.getElementById('modal-title');
const modalContent = document.getElementById('modal-content');
const closeModalBtns = document.querySelectorAll('.modal-close, .btn-modal-close');

// Tabs
const tabBtns = document.querySelectorAll('.tab-btn');
const tabContents = document.querySelectorAll('.tab-content');

// State
let selectedSalePrice = 0;
let searchTimeout = null;

// Initialization
document.addEventListener('DOMContentLoaded', () => {
  setupTabs();
  setupSearch();
  setupForms();
  setupModal();
  loadData();
});

// Load All Data
async function loadData() {
  await refreshInventory();
  await refreshSalesHistory();
}

// Refresh Inventory & Dropdowns
async function refreshInventory() {
  try {
    const query = searchInput.value;
    const medicines = await api.getMedicines(query);
    
    // Render Grid
    renderMedicinesGrid(inventoryBody, medicines, showMedicineDetails, startSaleFromGrid);

    // Update Sale Selection Dropdown
    const currentSelectVal = saleMedicineSelect.value;
    saleMedicineSelect.innerHTML = '<option value="">-- Select Medicine --</option>' +
      medicines.map(m => `<option value="${m.id}" data-price="${m.price}">${escapeHtml(m.fullName)} (Stock: ${m.quantity})</option>`).join('');
    
    if (currentSelectVal) {
      saleMedicineSelect.value = currentSelectVal;
    }

    // Update Quick Statistics from un-filtered dataset
    const allMedicines = await api.getMedicines();
    document.getElementById('stats-total-meds').textContent = allMedicines.length;
    document.getElementById('stats-expiry-alerts').textContent = allMedicines.filter(m => m.isNearExpiry).length;
    document.getElementById('stats-low-stock').textContent = allMedicines.filter(m => m.isLowStock).length;
  } catch (err) {
    showToast(err.message, 'danger');
  }
}

// Refresh Sales History
async function refreshSalesHistory() {
  try {
    const sales = await api.getSales();
    
    // Calculate total revenue
    const totalRev = sales.reduce((sum, s) => sum + s.totalAmount, 0);
    document.getElementById('stats-revenue').textContent = `₹${totalRev.toFixed(2)}`;

    if (sales.length === 0) {
      salesHistoryBody.innerHTML = `
        <tr>
          <td colspan="5" style="text-align: center; color: var(--text-muted); padding: 2rem;">
            No sales recorded yet.
          </td>
        </tr>
      `;
      return;
    }

    salesHistoryBody.innerHTML = sales.map(s => {
      const saleDate = new Date(s.saleDate).toLocaleString();
      return `
        <tr>
          <td style="font-weight: 600; color: #fff;">${escapeHtml(s.medicineName)}</td>
          <td class="mono">${s.quantitySold}</td>
          <td class="mono">₹${s.unitPriceAtSale.toFixed(2)}</td>
          <td class="mono" style="font-weight: 600; color: var(--primary);">₹${s.totalAmount.toFixed(2)}</td>
          <td class="mono">${saleDate}</td>
        </tr>
      `;
    }).join('');
  } catch (err) {
    showToast(err.message, 'danger');
  }
}

// Show Medicine Detail Modal
async function showMedicineDetails(id) {
  try {
    const m = await api.getMedicine(id);
    modalTitle.textContent = m.fullName;
    
    const expDate = new Date(m.expiryDate).toISOString().split('T')[0];

    modalContent.innerHTML = `
      <div style="display: grid; gap: 1rem;">
        <div>
          <label>Brand</label>
          <div style="font-weight: 600;">${escapeHtml(m.brand)}</div>
        </div>
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;">
          <div>
            <label>Price</label>
            <div class="mono" style="font-weight: 600; color: var(--primary);">₹${m.price.toFixed(2)}</div>
          </div>
          <div>
            <label>Stock Quantity</label>
            <div class="mono">${m.quantity} units</div>
          </div>
        </div>
        <div>
          <label>Expiry Date</label>
          <div class="mono">${expDate}</div>
        </div>
        <div>
          <label>Notes / Storage Instructions</label>
          <div style="background-color: var(--paper); padding: 0.75rem; border-radius: 6px; font-size: 0.9rem; border: 1px solid var(--line); white-space: pre-wrap;">${escapeHtml(m.notes) || 'No notes available.'}</div>
        </div>
      </div>
    `;
    
    detailModal.classList.add('show');
  } catch (err) {
    showToast(err.message, 'danger');
  }
}

// Start Sale from Grid click
function startSaleFromGrid(id, name, price) {
  saleMedicineSelect.value = id;
  selectedSalePrice = price;
  saleQtyInput.value = 1;
  updateSaleTotal();
  
  // Scroll to record sale form
  document.getElementById('record-sale-card').scrollIntoView({ behavior: 'smooth' });
}

// Search debounce
function setupSearch() {
  searchInput.addEventListener('input', () => {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
      refreshInventory();
    }, 300);
  });
}

// Setup Forms (Add Medicine & Record Sale)
function setupForms() {
  // Sale select change price calculation
  saleMedicineSelect.addEventListener('change', (e) => {
    const selectedOption = e.target.options[e.target.selectedIndex];
    if (selectedOption && selectedOption.dataset.price) {
      selectedSalePrice = parseFloat(selectedOption.dataset.price);
    } else {
      selectedSalePrice = 0;
    }
    updateSaleTotal();
  });

  saleQtyInput.addEventListener('input', updateSaleTotal);

  // Form Submit: Add Medicine
  addMedicineForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    const formData = new FormData(addMedicineForm);
    const dto = {
      fullName: formData.get('fullName'),
      brand: formData.get('brand'),
      price: parseFloat(formData.get('price')),
      quantity: parseInt(formData.get('quantity'), 10),
      expiryDate: new Date(formData.get('expiryDate')).toISOString(),
      notes: formData.get('notes')
    };

    try {
      await api.addMedicine(dto);
      showToast('Medicine added successfully!', 'success');
      addMedicineForm.reset();
      await loadData();
    } catch (err) {
      showToast(err.message, 'danger');
    }
  });

  // Form Submit: Record Sale
  recordSaleForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    const dto = {
      medicineId: saleMedicineSelect.value,
      quantitySold: parseInt(saleQtyInput.value, 10)
    };

    if (!dto.medicineId) {
      showToast('Please select a medicine.', 'warning');
      return;
    }

    try {
      await api.recordSale(dto);
      showToast('Sale recorded successfully!', 'success');
      recordSaleForm.reset();
      selectedSalePrice = 0;
      updateSaleTotal();
      await loadData();
    } catch (err) {
      showToast(err.message, 'danger');
    }
  });
}

function updateSaleTotal() {
  const qty = parseInt(saleQtyInput.value, 10) || 0;
  const total = qty * selectedSalePrice;
  saleTotalAmount.textContent = `₹${total.toFixed(2)}`;
}

// Tabs setup
function setupTabs() {
  tabBtns.forEach(btn => {
    btn.addEventListener('click', () => {
      tabBtns.forEach(b => b.classList.remove('active'));
      tabContents.forEach(c => c.classList.remove('active'));
      
      btn.classList.add('active');
      const target = btn.dataset.tab;
      document.getElementById(target).classList.add('active');
    });
  });
}

// Modal closing setup
function setupModal() {
  closeModalBtns.forEach(btn => {
    btn.addEventListener('click', () => {
      detailModal.classList.remove('show');
    });
  });

  detailModal.addEventListener('click', (e) => {
    if (e.target === detailModal) {
      detailModal.classList.remove('show');
    }
  });
}

// Notification System
function showToast(message, type = 'success') {
  const toastContainer = document.getElementById('toast-container');
  const toast = document.createElement('div');
  toast.className = `toast ${type === 'danger' ? 'pill-danger' : type === 'warning' ? 'pill-warning' : 'pill-success'}`;
  toast.style.padding = '0.75rem 1.25rem';
  toast.style.border = '1px solid';
  toast.style.borderRadius = '8px';
  toast.style.boxShadow = 'var(--shadow-md)';
  toast.style.display = 'flex';
  toast.style.alignItems = 'center';
  toast.style.gap = '0.5rem';
  toast.style.color = '#fff';
  toast.style.background = type === 'danger' ? '#dc2626' : type === 'warning' ? '#d97706' : '#059669';
  
  toast.innerHTML = `
    <span>${escapeHtml(message)}</span>
  `;

  toastContainer.appendChild(toast);
  setTimeout(() => {
    toast.style.opacity = '0';
    toast.style.transition = 'opacity 0.5s ease';
    setTimeout(() => toast.remove(), 500);
  }, 4000);
}

function escapeHtml(str) {
  if (!str) return '';
  return str.replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
}
