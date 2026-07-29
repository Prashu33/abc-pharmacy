// grid.js - Grid rendering and visual rules

export function renderMedicinesGrid(container, medicines, onSelectMedicineDetail, onSelectMedicineSale) {
  if (medicines.length === 0) {
    container.innerHTML = `
      <tr>
        <td colspan="6" style="text-align: center; color: var(--text-muted); padding: 2rem;">
          No medicines found.
        </td>
      </tr>
    `;
    return;
  }

  container.innerHTML = medicines.map(m => {
    // Expiry date format
    const expDate = new Date(m.expiryDate);
    const dateStr = expDate.toISOString().split('T')[0];

    // CSS class selection based on the backend computed values
    let rowClass = '';
    let badges = '';
    
    if (m.isNearExpiry) {
      rowClass = 'row-expiring';
      badges += `<span class="pill pill-danger" style="margin-right: 0.25rem;">Near Expiry</span>`;
    } else if (m.isLowStock) {
      rowClass = 'row-lowstock';
      badges += `<span class="pill pill-warning" style="margin-right: 0.25rem;">Low Stock</span>`;
    }

    return `
      <tr class="${rowClass}">
        <td>
          <div style="font-weight: 600; color: var(--ink);">${escapeHtml(m.fullName)}</div>
          <div>${badges}</div>
        </td>
        <td>${escapeHtml(m.brand)}</td>
        <td class="mono">${dateStr}</td>
        <td class="mono">${m.quantity}</td>
        <td class="mono">₹${m.price.toFixed(2)}</td>
        <td>
          <div style="display: flex; gap: 0.5rem;">
            <button class="btn btn-secondary btn-sm btn-detail" data-id="${m.id}">Details</button>
            <button class="btn btn-primary btn-sm btn-sell" data-id="${m.id}" data-name="${escapeHtml(m.fullName)}" data-price="${m.price}">Sell</button>
          </div>
        </td>
      </tr>
    `;
  }).join('');

  // Wire up event listeners
  container.querySelectorAll('.btn-detail').forEach(btn => {
    btn.addEventListener('click', () => onSelectMedicineDetail(btn.dataset.id));
  });

  container.querySelectorAll('.btn-sell').forEach(btn => {
    btn.addEventListener('click', () => {
      onSelectMedicineSale(btn.dataset.id, btn.dataset.name, parseFloat(btn.dataset.price));
    });
  });
}

function escapeHtml(str) {
  if (!str) return '';
  return str.replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
}
