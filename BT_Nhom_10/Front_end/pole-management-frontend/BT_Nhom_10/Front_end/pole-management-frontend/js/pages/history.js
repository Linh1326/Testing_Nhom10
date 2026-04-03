const STORAGE_KEY = "evcStations.v1";

const DEFAULT_STATIONS = [
  { id: "ST001", name: "Station 1" },
  { id: "ST002", name: "Station 2" },
  { id: "ST003", name: "Station 3" }
];

const HISTORY_SESSIONS = [
  { id: "HS001", stationId: "ST001", stationName: "Station 1", connector: "Port 1 - CCS2", status: "completed", start: "2026-03-29T08:10:00", end: "2026-03-29T09:05:00", kwh: 34.2, duration: 55, cost: 18.4, note: "Charging session completed normally." },
  { id: "HS002", stationId: "ST001", stationName: "Station 1", connector: "Port 2 - Type 2", status: "interrupted", start: "2026-03-29T13:20:00", end: "2026-03-29T13:54:00", kwh: 12.6, duration: 34, cost: 6.8, note: "The user stopped the session before completion." },
  { id: "HS003", stationId: "ST002", stationName: "Station 2", connector: "Port 1 - CCS2", status: "failed", start: "2026-03-28T10:12:00", end: "2026-03-28T10:19:00", kwh: 1.3, duration: 7, cost: 0.7, note: "Handshake error detected between station and vehicle." },
  { id: "HS004", stationId: "ST002", stationName: "Station 2", connector: "Port 2 - CHAdeMO", status: "completed", start: "2026-03-27T09:40:00", end: "2026-03-27T10:35:00", kwh: 29.8, duration: 55, cost: 16.1, note: "Stable charging performance during the entire session." },
  { id: "HS005", stationId: "ST001", stationName: "Station 1", connector: "Port 1 - CCS2", status: "completed", start: "2026-03-26T18:15:00", end: "2026-03-26T19:10:00", kwh: 31.5, duration: 55, cost: 17.3, note: "Energy usage matched expected historical behavior." },
  { id: "HS006", stationId: "ST003", stationName: "Station 3", connector: "Port 1 - CCS2", status: "interrupted", start: "2026-03-24T07:50:00", end: "2026-03-24T08:22:00", kwh: 10.1, duration: 32, cost: 5.5, note: "Maintenance found a loose connector during inspection." },
  { id: "HS007", stationId: "ST002", stationName: "Station 2", connector: "Port 3 - Type 2", status: "completed", start: "2026-03-22T15:05:00", end: "2026-03-22T16:00:00", kwh: 28.4, duration: 55, cost: 15.2, note: "Session completed successfully with no abnormalities." },
  { id: "HS008", stationId: "ST001", stationName: "Station 1", connector: "Port 2 - Type 2", status: "completed", start: "2026-03-20T11:25:00", end: "2026-03-20T12:08:00", kwh: 19.6, duration: 43, cost: 10.4, note: "Medium load and duration below the warning threshold." },
  { id: "HS009", stationId: "ST002", stationName: "Station 2", connector: "Port 2 - CHAdeMO", status: "completed", start: "2026-03-17T20:10:00", end: "2026-03-17T21:12:00", kwh: 36.1, duration: 62, cost: 19.6, note: "High load observed near the end of the session." },
  { id: "HS010", stationId: "ST003", stationName: "Station 3", connector: "Port 1 - CCS2", status: "failed", start: "2026-03-15T06:30:00", end: "2026-03-15T06:38:00", kwh: 0.8, duration: 8, cost: 0.4, note: "The station stopped due to a temperature sensor alert." }
];

function loadStationsFromStorage() {
  try {
    const raw = window.localStorage.getItem(STORAGE_KEY);
    if (!raw) return [...DEFAULT_STATIONS];
    const parsed = JSON.parse(raw);
    if (!Array.isArray(parsed)) return [...DEFAULT_STATIONS];
    return parsed;
  } catch {
    return [...DEFAULT_STATIONS];
  }
}

function dayKeyFromDate(date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

const stations = loadStationsFromStorage();
let filteredHistorySessions = [];
let currentHistorySession = null;

const historyDatePickerState = {
  view: new Date(2026, 2, 1),
  working: "",
  activeField: ""
};

const els = {
  historyDateFrom: document.getElementById("historyDateFrom"),
  historyDateTo: document.getElementById("historyDateTo"),
  historyDateFromTrigger: document.getElementById("historyDateFromTrigger"),
  historyDateFromText: document.getElementById("historyDateFromText"),
  historyDateToTrigger: document.getElementById("historyDateToTrigger"),
  historyDateToText: document.getElementById("historyDateToText"),
  historyStationFilter: document.getElementById("historyStationFilter"),
  historyConnectorFilter: document.getElementById("historyConnectorFilter"),
  historyStatusFilter: document.getElementById("historyStatusFilter"),
  historyChartMode: document.getElementById("historyChartMode"),
  historyChart: document.getElementById("historyChart"),
  historyTableBody: document.getElementById("historyTableBody"),
  historyEmptyState: document.getElementById("historyEmptyState"),
  historyResultCount: document.getElementById("historyResultCount"),
  historyMetricSessions: document.getElementById("historyMetricSessions"),
  historyMetricEnergy: document.getElementById("historyMetricEnergy"),
  historyMetricDuration: document.getElementById("historyMetricDuration"),
  historyMetricRevenue: document.getElementById("historyMetricRevenue"),
  historyExportFeedback: document.getElementById("historyExportFeedback"),
  historyExportMenu: document.getElementById("historyExportMenu"),
  historyExportTrigger: document.getElementById("historyExportTrigger"),
  historyDatePopover: document.getElementById("historyDatePopover"),
  historyDateTitle: document.getElementById("historyDateTitle"),
  historyDateGrid: document.getElementById("historyDateGrid"),
  historyDatePrevMonth: document.getElementById("historyDatePrevMonth"),
  historyDateNextMonth: document.getElementById("historyDateNextMonth"),
  historyDateCancel: document.getElementById("historyDateCancel"),
  historyDateDone: document.getElementById("historyDateDone"),
  historyDetailOverlay: document.getElementById("historyDetailOverlay"),
  historyDetailCloseBtn: document.getElementById("historyDetailCloseBtn"),
  historyDetailSessionId: document.getElementById("historyDetailSessionId"),
  historyDetailStation: document.getElementById("historyDetailStation"),
  historyDetailConnector: document.getElementById("historyDetailConnector"),
  historyDetailStatus: document.getElementById("historyDetailStatus"),
  historyDetailStart: document.getElementById("historyDetailStart"),
  historyDetailEnd: document.getElementById("historyDetailEnd"),
  historyDetailEnergy: document.getElementById("historyDetailEnergy"),
  historyDetailDuration: document.getElementById("historyDetailDuration"),
  historyDetailCost: document.getElementById("historyDetailCost")
};

function formatHistoryFilterDate(value) {
  if (!value) return "Select date";
  const date = new Date(`${value}T00:00:00`);
  return date.toLocaleDateString("en-US", {
    month: "short",
    day: "2-digit",
    year: "numeric"
  });
}

function syncHistoryDateTriggers() {
  if (els.historyDateFromText) els.historyDateFromText.textContent = formatHistoryFilterDate(els.historyDateFrom ? els.historyDateFrom.value : "");
  if (els.historyDateToText) els.historyDateToText.textContent = formatHistoryFilterDate(els.historyDateTo ? els.historyDateTo.value : "");
}

function isHistoryDatePopoverOpen() {
  return !!(els.historyDatePopover && els.historyDatePopover.classList.contains("is-open"));
}

function positionHistoryDatePopover(anchor) {
  if (!els.historyDatePopover || !anchor) return;
  const rect = anchor.getBoundingClientRect();
  const width = els.historyDatePopover.offsetWidth || 360;
  const height = els.historyDatePopover.offsetHeight || 320;
  let left = rect.left;
  let top = rect.bottom + 8;

  if (left + width > window.innerWidth - 8) left = window.innerWidth - width - 8;
  if (left < 8) left = 8;
  if (top + height > window.innerHeight - 8) top = Math.max(8, rect.top - height - 8);

  els.historyDatePopover.style.left = `${left}px`;
  els.historyDatePopover.style.top = `${top}px`;
}

function renderHistoryDatePicker() {
  if (!els.historyDateGrid || !els.historyDateTitle) return;
  const view = historyDatePickerState.view;
  const year = view.getFullYear();
  const month = view.getMonth();
  const first = new Date(year, month, 1);
  const lead = first.getDay();
  const daysInMonth = new Date(year, month + 1, 0).getDate();
  const prevMonthLast = new Date(year, month, 0).getDate();
  const cells = [];

  els.historyDateTitle.textContent = view.toLocaleDateString("en-US", { month: "long", year: "numeric" });

  for (let i = 0; i < lead; i += 1) cells.push({ d: new Date(year, month - 1, prevMonthLast - lead + i + 1), inMonth: false });
  for (let day = 1; day <= daysInMonth; day += 1) cells.push({ d: new Date(year, month, day), inMonth: true });
  const tail = (7 - (cells.length % 7)) % 7;
  for (let day = 1; day <= tail; day += 1) cells.push({ d: new Date(year, month + 1, day), inMonth: false });

  els.historyDateGrid.innerHTML = cells.map((cell) => {
    const key = dayKeyFromDate(cell.d);
    const outClass = cell.inMonth ? "" : " coord-cal__cell--out";
    const selectedClass = historyDatePickerState.working === key ? " coord-cal__cell--selected coord-cal__cell--solo" : "";
    const pill = historyDatePickerState.working === key ? '<span class="coord-cal__pill" aria-hidden="true"></span>' : "";
    return `<div class="coord-cal__cell${outClass}${selectedClass}">${pill}<button type="button" class="coord-cal__cell-btn" data-history-day="${key}">${cell.d.getDate()}</button></div>`;
  }).join("");
}

function openHistoryDatePopover(field) {
  if (!els.historyDatePopover) return;
  historyDatePickerState.activeField = field;
  historyDatePickerState.working = field === "from" ? (els.historyDateFrom ? els.historyDateFrom.value : "") : (els.historyDateTo ? els.historyDateTo.value : "");
  const seed = historyDatePickerState.working ? new Date(`${historyDatePickerState.working}T00:00:00`) : new Date("2026-04-01T00:00:00");
  historyDatePickerState.view = new Date(seed.getFullYear(), seed.getMonth(), 1);
  renderHistoryDatePicker();
  els.historyDatePopover.classList.add("is-open");
  els.historyDatePopover.setAttribute("aria-hidden", "false");

  if (els.historyDateFromTrigger) {
    els.historyDateFromTrigger.classList.toggle("is-active", field === "from");
    els.historyDateFromTrigger.setAttribute("aria-expanded", field === "from" ? "true" : "false");
  }
  if (els.historyDateToTrigger) {
    els.historyDateToTrigger.classList.toggle("is-active", field === "to");
    els.historyDateToTrigger.setAttribute("aria-expanded", field === "to" ? "true" : "false");
  }

  const anchor = field === "from" ? els.historyDateFromTrigger : els.historyDateToTrigger;
  requestAnimationFrame(() => positionHistoryDatePopover(anchor));
}

function closeHistoryDatePopover() {
  if (!els.historyDatePopover) return;
  els.historyDatePopover.classList.remove("is-open");
  els.historyDatePopover.setAttribute("aria-hidden", "true");
  if (els.historyDateFromTrigger) {
    els.historyDateFromTrigger.classList.remove("is-active");
    els.historyDateFromTrigger.setAttribute("aria-expanded", "false");
  }
  if (els.historyDateToTrigger) {
    els.historyDateToTrigger.classList.remove("is-active");
    els.historyDateToTrigger.setAttribute("aria-expanded", "false");
  }
  historyDatePickerState.activeField = "";
}

function commitHistoryDatePopover() {
  if (!historyDatePickerState.working) {
    closeHistoryDatePopover();
    return;
  }

  if (historyDatePickerState.activeField === "from" && els.historyDateFrom) {
    els.historyDateFrom.value = historyDatePickerState.working;
    if (els.historyDateTo && els.historyDateTo.value && els.historyDateFrom.value > els.historyDateTo.value) {
      els.historyDateTo.value = els.historyDateFrom.value;
    }
  }

  if (historyDatePickerState.activeField === "to" && els.historyDateTo) {
    els.historyDateTo.value = historyDatePickerState.working;
    if (els.historyDateFrom && els.historyDateFrom.value && els.historyDateTo.value < els.historyDateFrom.value) {
      els.historyDateFrom.value = els.historyDateTo.value;
    }
  }

  syncHistoryDateTriggers();
  closeHistoryDatePopover();
  renderHistoryView();
}

function formatHistoryStatus(status) {
  if (status === "completed") return "Completed";
  if (status === "interrupted") return "Interrupted";
  if (status === "failed") return "Failed";
  return status;
}

function formatHistoryDateTime(value) {
  const date = new Date(value);
  return date.toLocaleString("en-GB", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit"
  });
}

function populateHistoryFilters() {
  if (els.historyStationFilter) {
    const stationOptions = ['<option value="all">All Stations</option>']
      .concat(stations.map((station) => `<option value="${station.id}">${station.id} - ${station.name}</option>`));
    els.historyStationFilter.innerHTML = stationOptions.join("");
  }

  if (els.historyConnectorFilter) {
    const connectors = [...new Set(HISTORY_SESSIONS.map((item) => item.connector))];
    els.historyConnectorFilter.innerHTML = ['<option value="all">All Connectors</option>']
      .concat(connectors.map((connector) => `<option value="${connector}">${connector}</option>`))
      .join("");
  }
}

function getFilteredHistorySessions() {
  const dateFrom = els.historyDateFrom ? els.historyDateFrom.value : "";
  const dateTo = els.historyDateTo ? els.historyDateTo.value : "";
  const stationId = els.historyStationFilter ? els.historyStationFilter.value : "all";
  const connector = els.historyConnectorFilter ? els.historyConnectorFilter.value : "all";
  const status = els.historyStatusFilter ? els.historyStatusFilter.value : "all";

  return HISTORY_SESSIONS.filter((session) => {
    const start = new Date(session.start);
    const startDateOnly = new Date(start.getFullYear(), start.getMonth(), start.getDate());
    const fromDate = dateFrom ? new Date(`${dateFrom}T00:00:00`) : null;
    const toDate = dateTo ? new Date(`${dateTo}T23:59:59`) : null;
    const fromMatch = !fromDate || startDateOnly >= fromDate;
    const toMatch = !toDate || start <= toDate;
    const stationMatch = stationId === "all" || session.stationId === stationId;
    const connectorMatch = connector === "all" || session.connector === connector;
    const statusMatch = status === "all" || session.status === status;
    return fromMatch && toMatch && stationMatch && connectorMatch && statusMatch;
  }).sort((a, b) => new Date(b.start) - new Date(a.start));
}

function buildHistoryChartData(sessions) {
  const mode = els.historyChartMode ? els.historyChartMode.value : "day";
  const grouped = new Map();

  sessions.forEach((session) => {
    const date = new Date(session.start);
    let key = "";
    if (mode === "week") {
      const temp = new Date(date);
      temp.setHours(0, 0, 0, 0);
      temp.setDate(temp.getDate() - ((temp.getDay() + 6) % 7));
      key = `${String(temp.getDate()).padStart(2, "0")}/${String(temp.getMonth() + 1).padStart(2, "0")}`;
    } else {
      key = `${String(date.getDate()).padStart(2, "0")}/${String(date.getMonth() + 1).padStart(2, "0")}`;
    }
    grouped.set(key, (grouped.get(key) || 0) + 1);
  });

  return [...grouped.entries()].slice(-7);
}

function renderHistoryChart(sessions) {
  if (!els.historyChart) return;
  const data = buildHistoryChartData(sessions);

  if (!data.length) {
    els.historyChart.innerHTML = '<div class="muted">No chart data available.</div>';
    return;
  }

  const maxValue = Math.max(...data.map((item) => item[1]), 1);
  els.historyChart.innerHTML = data.map(([label, value]) => {
    const height = Math.max(18, Math.round((value / maxValue) * 180));
    return `
      <div class="history-chart__bar">
        <span class="history-chart__value">${value}</span>
        <div class="history-chart__column" style="height:${height}px"></div>
        <span class="history-chart__label">${label}</span>
      </div>
    `;
  }).join("");
}

function renderHistorySummary(sessions) {
  const totalSessions = sessions.length;
  const totalEnergy = sessions.reduce((sum, item) => sum + item.kwh, 0);
  const totalDuration = sessions.reduce((sum, item) => sum + item.duration, 0);
  const totalRevenue = sessions.reduce((sum, item) => sum + item.cost, 0);

  if (els.historyMetricSessions) els.historyMetricSessions.textContent = String(totalSessions);
  if (els.historyMetricEnergy) els.historyMetricEnergy.textContent = `${totalEnergy.toFixed(1)} kWh`;
  if (els.historyMetricDuration) els.historyMetricDuration.textContent = `${totalSessions ? Math.round(totalDuration / totalSessions) : 0} min`;
  if (els.historyMetricRevenue) els.historyMetricRevenue.textContent = `$${totalRevenue.toFixed(1)}`;
}

function renderHistoryTable(sessions) {
  if (!els.historyTableBody) return;
  if (els.historyResultCount) {
    els.historyResultCount.textContent = `${sessions.length} result${sessions.length === 1 ? "" : "s"}`;
  }
  if (els.historyEmptyState) {
    els.historyEmptyState.classList.toggle("is-visible", sessions.length === 0);
  }

  els.historyTableBody.innerHTML = sessions.map((session) => `
    <tr>
      <td>${session.id}</td>
      <td>${session.stationName}</td>
      <td>${session.connector}</td>
      <td>${formatHistoryDateTime(session.start)}</td>
      <td>${formatHistoryDateTime(session.end)}</td>
      <td>${session.kwh.toFixed(1)}</td>
      <td>${session.duration} min</td>
      <td>$${session.cost.toFixed(1)}</td>
      <td><span class="history-session-status history-session-status--${session.status}">${formatHistoryStatus(session.status)}</span></td>
      <td><button type="button" class="history-detail-btn" data-session-id="${session.id}">View</button></td>
    </tr>
  `).join("");
}

function renderHistoryView() {
  filteredHistorySessions = getFilteredHistorySessions();
  renderHistorySummary(filteredHistorySessions);
  renderHistoryChart(filteredHistorySessions);
  renderHistoryTable(filteredHistorySessions);
}

function openHistoryDetail(sessionId) {
  currentHistorySession = filteredHistorySessions.find((item) => item.id === sessionId) || null;
  if (!currentHistorySession || !els.historyDetailOverlay) return;

  if (els.historyDetailSessionId) els.historyDetailSessionId.textContent = currentHistorySession.id;
  if (els.historyDetailStation) els.historyDetailStation.textContent = `${currentHistorySession.stationId} - ${currentHistorySession.stationName}`;
  if (els.historyDetailConnector) els.historyDetailConnector.textContent = currentHistorySession.connector;
  if (els.historyDetailStatus) els.historyDetailStatus.textContent = formatHistoryStatus(currentHistorySession.status);
  if (els.historyDetailStart) els.historyDetailStart.textContent = formatHistoryDateTime(currentHistorySession.start);
  if (els.historyDetailEnd) els.historyDetailEnd.textContent = formatHistoryDateTime(currentHistorySession.end);
  if (els.historyDetailEnergy) els.historyDetailEnergy.textContent = `${currentHistorySession.kwh.toFixed(1)} kWh`;
  if (els.historyDetailDuration) els.historyDetailDuration.textContent = `${currentHistorySession.duration} min`;
  if (els.historyDetailCost) els.historyDetailCost.textContent = `$${currentHistorySession.cost.toFixed(1)} • ${currentHistorySession.note}`;

  els.historyDetailOverlay.classList.add("is-open");
  els.historyDetailOverlay.setAttribute("aria-hidden", "false");
}

function closeHistoryDetail() {
  if (!els.historyDetailOverlay) return;
  els.historyDetailOverlay.classList.remove("is-open");
  els.historyDetailOverlay.setAttribute("aria-hidden", "true");
}

function isHistoryExportMenuOpen() {
  return !!(els.historyExportMenu && els.historyExportMenu.classList.contains("is-open"));
}

function openHistoryExportMenu() {
  if (!els.historyExportMenu || !els.historyExportTrigger) return;
  els.historyExportMenu.classList.add("is-open");
  els.historyExportTrigger.setAttribute("aria-expanded", "true");
}

function closeHistoryExportMenu() {
  if (!els.historyExportMenu || !els.historyExportTrigger) return;
  els.historyExportMenu.classList.remove("is-open");
  els.historyExportTrigger.setAttribute("aria-expanded", "false");
}

function exportHistoryData(type) {
  try {
    const sessions = filteredHistorySessions.length ? filteredHistorySessions : getFilteredHistorySessions();
    const header = ["Session ID", "Station", "Connector", "Start", "End", "kWh", "Duration", "Cost", "Status"];
    const rows = sessions.map((item) => [
      item.id,
      `${item.stationId} - ${item.stationName}`,
      item.connector,
      formatHistoryDateTime(item.start),
      formatHistoryDateTime(item.end),
      item.kwh.toFixed(1),
      `${item.duration} min`,
      item.cost.toFixed(1),
      formatHistoryStatus(item.status)
    ]);
    const separator = type === "csv" ? "," : "\t";
    const content = [header, ...rows].map((row) => row.join(separator)).join("\n");
    const mime = type === "csv" ? "text/csv" : "text/plain";
    const blob = new Blob([content], { type: mime });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = `station-history.${type}`;
    link.click();

    if (els.historyExportFeedback) {
      els.historyExportFeedback.innerHTML = `Export completed successfully. <a href="${url}" download="station-history.${type}">Download file</a>`;
      els.historyExportFeedback.classList.add("is-visible");
    }
  } catch {
    if (els.historyExportFeedback) {
      els.historyExportFeedback.textContent = "Unable to export data. Please try again.";
      els.historyExportFeedback.classList.add("is-visible");
    }
  }
}

function bindHistoryEvents() {
  populateHistoryFilters();
  syncHistoryDateTriggers();

  [els.historyDateFrom, els.historyDateTo, els.historyStationFilter, els.historyConnectorFilter, els.historyStatusFilter, els.historyChartMode]
    .filter(Boolean)
    .forEach((element) => element.addEventListener("change", renderHistoryView));

  if (els.historyDateFromTrigger) {
    els.historyDateFromTrigger.addEventListener("click", (event) => {
      event.stopPropagation();
      if (isHistoryDatePopoverOpen() && historyDatePickerState.activeField === "from") {
        closeHistoryDatePopover();
        return;
      }
      openHistoryDatePopover("from");
    });
  }

  if (els.historyDateToTrigger) {
    els.historyDateToTrigger.addEventListener("click", (event) => {
      event.stopPropagation();
      if (isHistoryDatePopoverOpen() && historyDatePickerState.activeField === "to") {
        closeHistoryDatePopover();
        return;
      }
      openHistoryDatePopover("to");
    });
  }

  if (els.historyDateGrid) {
    els.historyDateGrid.addEventListener("click", (event) => {
      const button = event.target.closest("[data-history-day]");
      if (!button) return;
      historyDatePickerState.working = button.dataset.historyDay;
      renderHistoryDatePicker();
    });
  }

  if (els.historyDatePrevMonth) {
    els.historyDatePrevMonth.addEventListener("click", () => {
      historyDatePickerState.view = new Date(historyDatePickerState.view.getFullYear(), historyDatePickerState.view.getMonth() - 1, 1);
      renderHistoryDatePicker();
    });
  }

  if (els.historyDateNextMonth) {
    els.historyDateNextMonth.addEventListener("click", () => {
      historyDatePickerState.view = new Date(historyDatePickerState.view.getFullYear(), historyDatePickerState.view.getMonth() + 1, 1);
      renderHistoryDatePicker();
    });
  }

  if (els.historyDateCancel) els.historyDateCancel.addEventListener("click", closeHistoryDatePopover);
  if (els.historyDateDone) els.historyDateDone.addEventListener("click", commitHistoryDatePopover);

  if (els.historyTableBody) {
    els.historyTableBody.addEventListener("click", (event) => {
      const button = event.target.closest("[data-session-id]");
      if (!button) return;
      openHistoryDetail(button.dataset.sessionId);
    });
  }

  if (els.historyExportTrigger) {
    els.historyExportTrigger.addEventListener("click", (event) => {
      event.stopPropagation();
      if (isHistoryExportMenuOpen()) closeHistoryExportMenu();
      else openHistoryExportMenu();
    });
  }

  document.querySelectorAll("[data-export-type]").forEach((button) => {
    button.addEventListener("click", () => {
      exportHistoryData(button.dataset.exportType);
      closeHistoryExportMenu();
    });
  });

  if (els.historyDetailCloseBtn) els.historyDetailCloseBtn.addEventListener("click", closeHistoryDetail);

  if (els.historyDetailOverlay) {
    els.historyDetailOverlay.addEventListener("click", (event) => {
      if (event.target === els.historyDetailOverlay) closeHistoryDetail();
    });
  }

  document.addEventListener("pointerdown", (event) => {
    if (
      isHistoryDatePopoverOpen() &&
      els.historyDatePopover &&
      !els.historyDatePopover.contains(event.target) &&
      !(els.historyDateFromTrigger && els.historyDateFromTrigger.contains(event.target)) &&
      !(els.historyDateToTrigger && els.historyDateToTrigger.contains(event.target))
    ) {
      closeHistoryDatePopover();
    }

    if (isHistoryExportMenuOpen() && els.historyExportMenu && !els.historyExportMenu.contains(event.target)) {
      closeHistoryExportMenu();
    }
  });

  document.addEventListener("keydown", (event) => {
    if (event.key !== "Escape") return;
    if (isHistoryDatePopoverOpen()) {
      closeHistoryDatePopover();
      return;
    }
    if (isHistoryExportMenuOpen()) {
      closeHistoryExportMenu();
      return;
    }
    if (els.historyDetailOverlay && els.historyDetailOverlay.classList.contains("is-open")) {
      closeHistoryDetail();
    }
  });

  window.addEventListener("resize", () => {
    if (!isHistoryDatePopoverOpen()) return;
    const anchor = historyDatePickerState.activeField === "from" ? els.historyDateFromTrigger : els.historyDateToTrigger;
    positionHistoryDatePopover(anchor);
  });
}

function init() {
  bindHistoryEvents();
  renderHistoryView();
}

init();
