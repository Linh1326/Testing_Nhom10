const initialPoles = [
  { id: "PO001", editId: "PP0019", name: "Pole 1", code: "<<<<<>>>>>>", manufacturer: "NCC1", model: "Model A", station: "Station 1", connectors: ["Connector 1", "Connector 2", "Connector 3"], date: "2025-07-29", time: "10:45 AM", createLabel: "10:15 AM Fri, 13 July, 2025", active: true },
  { id: "PO002", editId: "PP0020", name: "Pole 2", code: "<<<<<>>>>>>", manufacturer: "MCC2", model: "Model B", station: "Station 2", connectors: ["Connector 2", "Connector 4"], date: "2025-07-29", time: "10:45 AM", createLabel: "10:15 AM Fri, 13 July, 2025", active: false },
  { id: "PO003", editId: "PP0021", name: "Pole 3", code: "<<<<<>>>>>>", manufacturer: "NCC3", model: "Model C", station: "Station 3", connectors: ["Connector 1", "Connector 3"], date: "2025-07-29", time: "10:45 AM", createLabel: "10:15 AM Fri, 13 July, 2025", active: true },
  { id: "PO004", editId: "PP0022", name: "Pole 4", code: "<<<<<>>>>>>", manufacturer: "NCC4", model: "Model D", station: "Station 4", connectors: ["Connector 2", "Connector 5"], date: "2025-07-29", time: "10:45 AM", createLabel: "10:15 AM Fri, 13 July, 2025", active: true },
  { id: "PO005", editId: "PP0023", name: "Pole 5", code: "<<<<<>>>>>>", manufacturer: "NCC5", model: "Model E", station: "Station 1", connectors: ["Connector 1"], date: "2025-07-29", time: "10:45 AM", createLabel: "10:15 AM Fri, 13 July, 2025", active: false },
  { id: "PO006", editId: "PP0024", name: "Pole 6", code: "<<<<<>>>>>>", manufacturer: "NCC6", model: "Model F", station: "Station 2", connectors: ["Connector 1", "Connector 2", "Connector 3"], date: "2025-07-29", time: "10:45 AM", createLabel: "10:15 AM Fri, 13 July, 2025", active: true }
];

// Dữ liệu cảnh báo mẫu (khớp với nội dung trong alerts.html)
const DEFAULT_ALERTS = [
  { id: 1, type: "Quá nhiệt bộ điều khiển", statusGroup: "pending" },
  { id: 2, type: "Mất kết nối cổng sạc", statusGroup: "processing" },
  { id: 3, type: "Lỗi xác thực thiết bị", statusGroup: "resolved" },
  // Bạn có thể thêm nhiều lỗi 'pending' ở đây để test số lượng tăng lên
  { id: 4, type: "Lỗi dòng điện", statusGroup: "pending" },
  { id: 5, type: "Nắp trạm bị mở trái phép", statusGroup: "pending" }
];

let alerts = [...DEFAULT_ALERTS];

let poles = [...initialPoles];
let currentPole = null;
let isCreateMode = false;
let filterState = { code: "", manufacturer: "", installedAt: "", status: "" };

const poleRows = document.getElementById("poleRows");
const poleCards = document.getElementById("poleCards");
const connectorTags = document.getElementById("connectorTags");
const emptyState = document.getElementById("emptyState");

const searchInput = document.getElementById("searchInput");
const searchClearBtn = document.getElementById("searchClearBtn");

const fieldPoleName = document.getElementById("fieldPoleName");
const fieldManufacturer = document.getElementById("fieldManufacturer");
const fieldModel = document.getElementById("fieldModel");
const fieldStation = document.getElementById("fieldStation");
const fieldCode = document.getElementById("fieldCode");
const fieldInstalledDate = document.getElementById("fieldInstalledDate");
const fieldConnectors = document.getElementById("fieldConnectors");
const drawerActions = document.getElementById("drawerActions");
const createStationBox = document.getElementById("createStationBox");
const createQrButton = document.getElementById("createQrButton");
const createConnectorBox = document.getElementById("createConnectorBox");
const deleteButton = document.getElementById("deleteButton");
const saveUpdateButton = document.getElementById("saveUpdateButton");
const drawerFooter = document.querySelector(".drawer-footer");
const listView = document.getElementById("listView");
const detailView = document.getElementById("detailView");
const pageTitleSuffix = document.getElementById("pageTitleSuffix");
const detailCloseBtn = document.getElementById("detailCloseBtn");
const detailDeleteBtn = document.getElementById("detailDeleteBtn");
const detailPoleName = document.getElementById("detailPoleName");
const detailPoleId = document.getElementById("detailPoleId");
const detailPoleManufacturer = document.getElementById("detailPoleManufacturer");
const detailPoleModel = document.getElementById("detailPoleModel");
const detailPoleStatus = document.getElementById("detailPoleStatus");
const detailPoleStation = document.getElementById("detailPoleStation");
const detailPoleInstalledAt = document.getElementById("detailPoleInstalledAt");
const detailPoleCode = document.getElementById("detailPoleCode");
const detailPoleConnectors = document.getElementById("detailPoleConnectors");
const stationDropdownToggle = document.getElementById("stationDropdownToggle");
const stationDropdownMenu = document.getElementById("stationDropdownMenu");
const stationDropdownLabel = document.getElementById("stationDropdownLabel");
const deactivatePoleOverlay = document.getElementById("deactivatePoleOverlay");
const deactivatePoleCancelBtn = document.getElementById("deactivatePoleCancelBtn");
const deactivatePoleConfirmBtn = document.getElementById("deactivatePoleConfirmBtn");
const drawer = document.getElementById("drawer");

function escapeHtml(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#39;");
}

function editActionIcon() {
  return `
    <svg viewBox="0 0 24 24" aria-hidden="true" class="pole-edit-icon">
      <path d="M3 21h6l11-11a2.2 2.2 0 0 0-6-3.2L3 17.8V21Z" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round"></path>
      <path d="M13.5 7.5l3 3" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"></path>
      <path d="M3 21h14" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"></path>
    </svg>
  `;
}

function statusBadgeClasses(active) {
  return active ? "border-[#2c34ff] text-[#2c34ff]" : "border-[#d9a210] text-[#d9a210]";
}

function formatDate(dateString) {
  const [year, month, day] = dateString.split("-");
  return `${day}/${month}/${year}`;
}

function getConnectorArray(value) {
  return value
    .split(",")
    .map((item) => item.trim())
    .filter(Boolean);
}

function filteredPoles() {
  const query = searchInput.value.trim().toLowerCase();
  return poles.filter((pole) => {
    const matchesSearch =
      !query ||
      [pole.id, pole.name, pole.code, pole.manufacturer, pole.station, pole.model]
        .join(" ")
        .toLowerCase()
        .includes(query);

    const matchesCode = !filterState.code || pole.code === filterState.code;
    const matchesManufacturer = !filterState.manufacturer || pole.manufacturer === filterState.manufacturer;
    const matchesInstalledAt = !filterState.installedAt || pole.date === filterState.installedAt;
    const matchesStatus =
      !filterState.status ||
      (filterState.status === "active" && pole.active) ||
      (filterState.status === "inactive" && !pole.active);

    return matchesSearch && matchesCode && matchesManufacturer && matchesInstalledAt && matchesStatus;
  });
}

function updateFilterButtons() {
  document.querySelectorAll(".sort-trigger").forEach((button) => {
    const key = button.dataset.filterKey;
    const isActive = !!filterState[key];
    button.classList.toggle("text-brand-500", isActive);
    button.classList.toggle("text-[#222]", !isActive);
    button.classList.toggle("filter-trigger-active", isActive);
  });
}

function closeAllFilterMenus() {
  document.querySelectorAll(".filter-menu").forEach((menu) => {
    menu.classList.add("hidden");
  });
}

function renderFilterMenus() {
  const configs = {
    code: {
      title: "All Active Codes",
      options: [...new Set(poles.map((pole) => pole.code))].map((value) => ({ value, label: value }))
    },
    manufacturer: {
      title: "All Manufacturers",
      options: [...new Set(poles.map((pole) => pole.manufacturer))].map((value) => ({ value, label: value }))
    },
    installedAt: {
      title: "All Create Dates",
      options: [...new Set(poles.map((pole) => pole.date))].map((value) => ({ value, label: formatDate(value) }))
    },
    status: {
      title: "All Status",
      options: [
        { value: "active", label: "Active" },
        { value: "inactive", label: "Inactive" }
      ]
    }
  };

  Object.entries(configs).forEach(([key, config]) => {
    const menu = document.getElementById(`filterMenu-${key}`);
    if (!menu) return;

    const allActiveClass = filterState[key] === "" ? "filter-menu__item--active" : "";
    menu.innerHTML = `
      <button type="button" class="filter-menu__item ${allActiveClass}" onclick="setFilter('${key}', '')">
        <span class="filter-menu__dot"></span>
        <span>${config.title}</span>
      </button>
      ${config.options
        .map((option) => {
          const activeClass = filterState[key] === option.value ? "filter-menu__item--active" : "";
          return `
            <button type="button" class="filter-menu__item ${activeClass}" onclick="setFilter('${key}', '${option.value}')">
              <span class="filter-menu__dot"></span>
              <span>${option.label}</span>
            </button>
          `;
        })
        .join("")}
    `;
  });
}

function toggleFilterMenu(key) {
  const target = document.getElementById(`filterMenu-${key}`);
  if (!target) return;

  const willOpen = target.classList.contains("hidden");
  closeAllFilterMenus();

  if (willOpen) {
    target.classList.remove("hidden");
  }
}

function setFilter(key, value) {
  filterState[key] = value;
  renderFilterMenus();
  updateFilterButtons();
  closeAllFilterMenus();
  renderPoles();
}

function renderConnectorTags(connectors) {
  connectorTags.innerHTML = connectors
    .map(
      (connector) => `
        <div class="inline-flex items-center gap-3 rounded-xl bg-brand-100 px-4 py-2 text-[15px] font-medium text-[#41515f]">
          <span>${connector}</span>
          <i class="fa-solid fa-xmark text-brand-400"></i>
        </div>
      `
    )
    .join("");
}

function renderPoles() {
  const data = filteredPoles();

  emptyState.classList.toggle("hidden", data.length !== 0);
  poleRows.innerHTML = data
    .map(
      (pole) => `
        <div data-action="detail" data-pole-id="${pole.id}" class="grid cursor-pointer grid-cols-[1fr_1.1fr_1.3fr_1.2fr_1.1fr_0.8fr] items-center border-b border-[#d9d9d9] px-5 py-6 text-[16px] hover:bg-[#fbfbfb]">
          <div class="px-4 text-[18px] font-extrabold text-brand-500">${pole.id}</div>
          <div class="px-4">${pole.name}</div>
          <div class="px-4">${pole.code}</div>
          <div class="px-4">${pole.manufacturer}</div>
          <div class="px-4 leading-7">
            <div>${formatDate(pole.date)}</div>
            <div>${pole.time}</div>
          </div>
          <div class="px-4">
            <div class="flex items-center justify-between gap-3">
              <span class="${statusBadgeClasses(pole.active)} inline-flex min-w-[92px] justify-center rounded-xl border bg-white px-3 py-1.5 text-[14px]">
                ${pole.active ? "Active" : "Inactive"}
              </span>
              <button type="button" class="pole-edit-action" aria-label="Edit ${escapeHtml(pole.name)}" data-action="edit" data-pole-id="${pole.id}">
                ${editActionIcon()}
              </button>
            </div>
          </div>
        </div>
      `
    )
    .join("");

  poleCards.innerHTML = data
    .map(
      (pole) => `
        <div data-action="detail" data-pole-id="${pole.id}" class="cursor-pointer rounded-2xl border border-[#dcdcdc] bg-white p-4 shadow-sm">
          <div class="mb-3 flex items-start justify-between gap-4">
            <div>
              <div class="text-[18px] font-extrabold text-brand-500">${pole.id}</div>
              <div class="mt-1 text-[17px] font-semibold text-[#2f2f2f]">${pole.name}</div>
            </div>
            <div class="flex items-center gap-3">
              <span class="${statusBadgeClasses(pole.active)} inline-flex min-w-[92px] justify-center rounded-xl border bg-white px-3 py-1.5 text-[14px]">
                ${pole.active ? "Active" : "Inactive"}
              </span>
              <button type="button" class="pole-edit-action" aria-label="Edit ${escapeHtml(pole.name)}" data-action="edit" data-pole-id="${pole.id}">
                ${editActionIcon()}
              </button>
            </div>
          </div>
          <div class="grid grid-cols-2 gap-3 text-[14px] text-[#555]">
            <div>
              <div class="text-[#909090]">Code</div>
              <div class="mt-1 text-[#2f2f2f]">${pole.code}</div>
            </div>
            <div>
              <div class="text-[#909090]">Manufacturer</div>
              <div class="mt-1 text-[#2f2f2f]">${pole.manufacturer}</div>
            </div>
            <div>
              <div class="text-[#909090]">Installed</div>
              <div class="mt-1 text-[#2f2f2f]">${formatDate(pole.date)}</div>
            </div>
            <div>
              <div class="text-[#909090]">Station</div>
              <div class="mt-1 text-[#2f2f2f]">${pole.station}</div>
            </div>
          </div>
        </div>
      `
    )
    .join("");

  updateFilterButtons();
}

function showDetail(id) {
  const pole = poles.find((item) => item.id === id);
  if (!pole) return;

  currentPole = pole;
  if (detailPoleName) detailPoleName.textContent = pole.name;
  if (detailPoleId) detailPoleId.textContent = pole.id;
  if (detailPoleManufacturer) detailPoleManufacturer.textContent = pole.manufacturer;
  if (detailPoleModel) detailPoleModel.textContent = pole.model;
  if (detailPoleStatus) {
    detailPoleStatus.innerHTML = `
      <span class="${statusBadgeClasses(pole.active)} inline-flex min-w-[108px] justify-center rounded-[18px] border bg-white px-4 py-2 text-[14px] font-medium">
        ${pole.active ? "Active" : "Inactive"}
      </span>
    `;
  }
  if (detailPoleStation) detailPoleStation.textContent = pole.station;
  if (detailPoleInstalledAt) detailPoleInstalledAt.textContent = `${formatDate(pole.date)}, ${pole.time}`;
  if (detailPoleCode) detailPoleCode.textContent = pole.code;
  if (detailPoleConnectors) {
    detailPoleConnectors.innerHTML = pole.connectors
      .map((connector) => `<li>• ${escapeHtml(connector)}: Available</li>`)
      .join("");
  }

  if (listView) {
    listView.classList.add("hidden");
    listView.style.display = "none";
  }
  if (detailView) {
    detailView.classList.remove("hidden");
    detailView.style.display = "block";
  }

  if (pageTitleSuffix) {
    pageTitleSuffix.innerHTML = `<span class="mx-2 sm:mx-3 text-[#202020]">›</span><span class="text-[#111]">${pole.id}</span>`;
    pageTitleSuffix.classList.remove("hidden");
  }
}

function showList() {
  currentPole = null;
  if (detailView) {
    detailView.classList.add("hidden");
    detailView.style.display = "none";
  }
  if (listView) {
    listView.classList.remove("hidden");
    listView.style.display = "";
  }

  if (pageTitleSuffix) {
    pageTitleSuffix.innerHTML = "";
    pageTitleSuffix.classList.add("hidden");
  }
}

function closeStationDropdown() {
  stationDropdownMenu?.classList.add("hidden");
}

function selectStationOption(value) {
  if (stationDropdownLabel) stationDropdownLabel.textContent = value;
  if (fieldStation) fieldStation.value = value;
  closeStationDropdown();
}

function syncDrawer(pole) {
  currentPole = pole;
  document.getElementById("drawerId").innerText = pole.editId;
  document.getElementById("drawerCreateTime").innerText = pole.createLabel;
  fieldPoleName.value = pole.name;
  fieldManufacturer.value = pole.manufacturer;
  fieldModel.value = pole.model;
  fieldStation.value = pole.station;
  fieldCode.value = pole.code;
  fieldInstalledDate.value = pole.date;
  fieldConnectors.value = pole.connectors.join(", ");
  document.getElementById("activeLabel").innerText = pole.active ? "Active" : "Inactive";
  document.getElementById("activeToggle").className = `relative flex h-[48px] w-[142px] items-center rounded-full px-5 text-[17px] font-semibold text-white transition ${pole.active ? "bg-[#3f5ee8] justify-start" : "bg-[#c89c1f] justify-end"}`;
  document.getElementById("activeKnob").className = `absolute h-[40px] w-[40px] rounded-full bg-white shadow-md transition ${pole.active ? "right-[4px]" : "left-[4px]"}`;
  renderConnectorTags(pole.connectors);
}

function setDrawerMode(createMode) {
  drawer?.classList.toggle("create-mode", createMode);
  drawerActions.classList.toggle("justify-end", !createMode);
  drawerActions.classList.toggle("justify-start", createMode);
  drawerActions.classList.toggle("hidden", createMode);
  document.getElementById("activeToggle").classList.toggle("hidden", createMode);
  deleteButton.classList.toggle("hidden", createMode);
  drawerFooter?.classList.toggle("sm:grid-cols-2", createMode);
  drawerFooter?.classList.toggle("sm:grid-cols-3", !createMode);
  createStationBox.classList.toggle("hidden", !createMode);
  createStationBox.classList.toggle("flex", createMode);
  createQrButton.classList.toggle("hidden", !createMode);
  createConnectorBox.classList.toggle("hidden", !createMode);
  createConnectorBox.classList.toggle("flex", createMode);
  fieldStation.classList.toggle("hidden", createMode);
  fieldCode.classList.toggle("hidden", createMode);
  fieldConnectors.classList.toggle("hidden", createMode);
  fieldInstalledDate.parentElement.classList.toggle("hidden", createMode);
  connectorTags.classList.toggle("hidden", createMode);
  saveUpdateButton.textContent = createMode ? "Save" : "Update";
  document.getElementById("drawerTitle").textContent = createMode ? "Create new" : "Edit";
}

function openCreate() {
  isCreateMode = true;
  const nextNumber = poles.length + 1;
  const newPole = {
    id: `PO${String(nextNumber).padStart(3, "0")}`,
    editId: `PP${String(1000 + nextNumber).padStart(4, "0")}`,
    name: "",
    code: "",
    manufacturer: "",
    model: "",
    station: "",
    connectors: [],
    date: "2025-07-29",
    time: "10:45 AM",
    createLabel: "10:15 AM Fri, 13 July, 2025",
    active: true
  };
  setDrawerMode(true);
  document.getElementById("drawer").classList.remove("translate-x-full");
  document.getElementById("drawerBackdrop").classList.remove("pointer-events-none", "opacity-0");
  syncDrawer(newPole);
  if (stationDropdownLabel) stationDropdownLabel.textContent = "Please Select";
  closeStationDropdown();
}

function openEdit(id) {
  isCreateMode = false;
  const selectedPole = poles.find((pole) => pole.id === id) || poles[0];
  setDrawerMode(false);
  syncDrawer(selectedPole);
  document.getElementById("drawer").classList.remove("translate-x-full");
  document.getElementById("drawerBackdrop").classList.remove("pointer-events-none", "opacity-0");
}

function closeDrawer() {
  document.getElementById("drawer").classList.add("translate-x-full");
  document.getElementById("drawerBackdrop").classList.add("pointer-events-none", "opacity-0");
}

function isDeactivatePoleOverlayOpen() {
  return deactivatePoleOverlay?.classList.contains("is-open");
}

function openDeactivatePoleOverlay() {
  if (!deactivatePoleOverlay) return;
  deactivatePoleOverlay.classList.add("is-open");
  deactivatePoleOverlay.setAttribute("aria-hidden", "false");
}

function closeDeactivatePoleOverlay() {
  if (!deactivatePoleOverlay) return;
  deactivatePoleOverlay.classList.remove("is-open");
  deactivatePoleOverlay.setAttribute("aria-hidden", "true");
}

function applyPoleActiveState(isActive) {
  if (!currentPole) return;
  currentPole.active = isActive;
  syncDrawer(currentPole);
  if (!isCreateMode) {
    renderPoles();
  }
}

function toggleActive() {
  if (!currentPole) return;

  if (currentPole.active) {
    openDeactivatePoleOverlay();
    return;
  }

  applyPoleActiveState(true);
}


function updateCurrentPole() {
  if (!currentPole) return;

  const payload = {
    ...currentPole,
    name: fieldPoleName.value.trim() || currentPole.name || "Untitled Pole",
    manufacturer: fieldManufacturer.value.trim() || currentPole.manufacturer || "Unknown",
    model: fieldModel.value.trim() || currentPole.model || "Unknown Model",
    station: fieldStation.value.trim() || currentPole.station || "Unknown Station",
    code: fieldCode.value.trim() || currentPole.code || "<<<<<>>>>>>",
    date: fieldInstalledDate.value || currentPole.date,
    connectors: getConnectorArray(fieldConnectors.value)
  };

  if (isCreateMode) {
    poles.unshift(payload);
    isCreateMode = false;
  } else {
    const index = poles.findIndex((pole) => pole.id === currentPole.id);
    if (index >= 0) {
      poles[index] = payload;
    }
  }

  currentPole = payload;
  renderPoles();
  syncDrawer(payload);
  closeDrawer();
}

function openDeleteModal() {
  const modal = document.getElementById("deleteModal");
  modal.classList.remove("pointer-events-none", "opacity-0");
}

function closeDeleteModal() {
  const modal = document.getElementById("deleteModal");
  modal.classList.add("pointer-events-none", "opacity-0");
}

function confirmDelete() {
  if (currentPole && !isCreateMode) {
    poles = poles.filter((pole) => pole.id !== currentPole.id);
    renderPoles();
    showList();
  }
  closeDeleteModal();
  closeDrawer();
}

function clearSearch() {
  searchInput.value = "";
  updateSearchClearVisibility();
  renderPoles();
}

function updateSearchClearVisibility() {
  if (!searchInput || !searchClearBtn) return;
  const hasValue = searchInput.value.trim().length > 0;
  searchClearBtn.classList.toggle("is-hidden", !hasValue);
}

[searchInput].forEach((element) => {
  element.addEventListener("input", () => {
    updateSearchClearVisibility();
    renderPoles();
  });
});

if (searchClearBtn) {
  searchClearBtn.addEventListener("click", () => {
    clearSearch();
    searchInput.focus();
  });
}

updateSearchClearVisibility();

fieldConnectors.addEventListener("input", () => {
  renderConnectorTags(getConnectorArray(fieldConnectors.value));
});

renderPoles();
renderFilterMenus();
updateFilterButtons();
setDrawerMode(false);

stationDropdownToggle?.addEventListener("click", (event) => {
  event.stopPropagation();
  stationDropdownMenu?.classList.toggle("hidden");
});

detailCloseBtn?.addEventListener("click", showList);
detailDeleteBtn?.addEventListener("click", () => {
  if (!currentPole) return;
  openDeleteModal();
});

function bindPoleListInteractions(container) {
  container?.addEventListener("click", (event) => {
    const editButton = event.target.closest('[data-action="edit"]');
    if (editButton) {
      const poleId = editButton.getAttribute("data-pole-id");
      if (poleId) openEdit(poleId);
      return;
    }

    const detailTrigger = event.target.closest('[data-action="detail"]');
    if (detailTrigger) {
      const poleId = detailTrigger.getAttribute("data-pole-id");
      if (poleId) showDetail(poleId);
    }
  });
}

bindPoleListInteractions(poleRows);
bindPoleListInteractions(poleCards);

deactivatePoleCancelBtn?.addEventListener("click", () => {
  closeDeactivatePoleOverlay();
});

deactivatePoleConfirmBtn?.addEventListener("click", () => {
  applyPoleActiveState(false);
  closeDeactivatePoleOverlay();
});

document.addEventListener("click", (event) => {
  if (!event.target.closest(".filter-menu") && !event.target.closest(".sort-trigger")) {
    closeAllFilterMenus();
  }
  if (!event.target.closest("#createStationBox")) {
    closeStationDropdown();
  }
});

deactivatePoleOverlay?.addEventListener("click", (event) => {
  if (event.target === deactivatePoleOverlay) {
    closeDeactivatePoleOverlay();
  }
});

window.showDetail = showDetail;
window.openEdit = openEdit;
window.openCreate = openCreate;
window.clearSearch = clearSearch;
window.toggleActive = toggleActive;
window.openDeleteModal = openDeleteModal;
window.closeDeleteModal = closeDeleteModal;
window.confirmDelete = confirmDelete;
window.closeDrawer = closeDrawer;
window.toggleFilterMenu = toggleFilterMenu;
window.selectStationOption = selectStationOption;

document.addEventListener("keydown", (event) => {
  if (event.key === "Escape" && isDeactivatePoleOverlayOpen()) {
    closeDeactivatePoleOverlay();
  }
});
