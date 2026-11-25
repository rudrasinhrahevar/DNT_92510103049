const apiUrl = "/api/students";
const rows = document.querySelector("#student-rows");
const searchInput = document.querySelector("#student-search");
const dialog = document.querySelector("#student-dialog");
const form = document.querySelector("#student-form");
const formError = document.querySelector("#dialog-error");
const notice = document.querySelector("#notice");
const saveButton = document.querySelector("#save-student");
let editingId = null;
let searchTimer;

const avatarColors = [
  ["#f0edff", "#6b59d2"],
  ["#e8f4ff", "#397bb5"],
  ["#eaf7ef", "#36895f"],
  ["#fff1e7", "#ba7042"],
  ["#fdeefa", "#a14f94"]
];

function escapeHtml(value) {
  return String(value).replace(/[&<>"']/g, (character) => ({
    "&": "&amp;",
    "<": "&lt;",
    ">": "&gt;",
    '"': "&quot;",
    "'": "&#39;"
  })[character]);
}

function initials(student) {
  return `${student.firstName[0] ?? ""}${student.lastName[0] ?? ""}`.toUpperCase();
}

function formatDate(value) {
  const date = new Date(`${value}T00:00:00`);
  return Number.isNaN(date.valueOf())
    ? escapeHtml(value)
    : new Intl.DateTimeFormat(undefined, { month: "short", day: "numeric", year: "numeric" }).format(date);
}

function showNotice(message) {
  notice.textContent = message;
  notice.hidden = false;
}

function hideNotice() {
  notice.textContent = "";
  notice.hidden = true;
}

async function getErrorMessage(response) {
  const problem = await response.json().catch(() => null);
  if (problem?.detail) return problem.detail;
  if (problem?.title) return problem.title;
  if (problem?.errors) {
    return Object.values(problem.errors).flat().join(" ");
  }
  return `Request failed (${response.status}). Please try again.`;
}

function renderStudents(students) {
  document.querySelector("#total-students").textContent = students.length;
  document.querySelector("#nav-count").textContent = students.length;
  document.querySelector("#total-departments").textContent =
    new Set(students.map((student) => student.department.toLowerCase())).size;
  document.querySelector("#api-status").innerHTML = `<span class="status-dot"></span> API connected`;

  document.querySelector("#results-label").textContent =
    `${students.length} ${students.length === 1 ? "student" : "students"} found`;

  if (students.length === 0) {
    rows.innerHTML = `<tr><td class="empty-cell" colspan="5">No students found. Add a student to get started.</td></tr>`;
    return;
  }

  rows.innerHTML = students.map((student, index) => {
    const [background, foreground] = avatarColors[index % avatarColors.length];
    return `
      <tr>
        <td>
          <div class="student-cell">
            <span class="student-avatar" style="--avatar-bg:${background};--avatar-color:${foreground}">${escapeHtml(initials(student))}</span>
            <span>
              <span class="student-name">${escapeHtml(student.firstName)} ${escapeHtml(student.lastName)}</span>
              <span class="student-email">${escapeHtml(student.email)}</span>
            </span>
          </div>
        </td>
        <td><span class="student-id">${escapeHtml(student.studentNumber)}</span></td>
        <td><span class="department-pill">${escapeHtml(student.department)}</span></td>
        <td>${formatDate(student.dateOfBirth)}</td>
        <td>
          <div class="row-actions">
            <button class="row-action" type="button" data-action="edit" data-id="${student.id}">Edit</button>
            <button class="row-action delete" type="button" data-action="delete" data-id="${student.id}">Delete</button>
          </div>
        </td>
      </tr>`;
  }).join("");
}

async function loadStudents(search = "") {
  hideNotice();
  rows.innerHTML = `<tr><td class="loading-cell" colspan="5"><span class="spinner"></span> Loading students...</td></tr>`;

  try {
    const url = new URL(apiUrl, window.location.origin);
    if (search.trim()) url.searchParams.set("search", search.trim());
    const response = await fetch(url);
    if (!response.ok) throw new Error(await getErrorMessage(response));
    renderStudents(await response.json());
  } catch (error) {
    rows.innerHTML = `<tr><td class="empty-cell" colspan="5">Student records could not be loaded.</td></tr>`;
    document.querySelector("#results-label").textContent = "Unable to load records";
    document.querySelector("#api-status").innerHTML = `<span class="status-dot status-offline"></span> API unavailable`;
    showNotice(error instanceof Error ? error.message : "The API could not be reached.");
  }
}

function openCreateDialog() {
  editingId = null;
  form.reset();
  formError.hidden = true;
  document.querySelector("#dialog-title").textContent = "Add a student";
  saveButton.textContent = "Add student";
  dialog.showModal();
  form.elements.studentNumber.focus();
}

async function openEditDialog(id) {
  form.reset();
  formError.hidden = true;
  try {
    const response = await fetch(`${apiUrl}/${id}`);
    if (!response.ok) throw new Error(await getErrorMessage(response));
    const student = await response.json();
    editingId = student.id;
    for (const [name, value] of Object.entries(student)) {
      if (form.elements.namedItem(name)) form.elements[name].value = value;
    }
    document.querySelector("#dialog-title").textContent = "Edit student";
    saveButton.textContent = "Save changes";
    dialog.showModal();
    form.elements.firstName.focus();
  } catch (error) {
    showNotice(error instanceof Error ? error.message : "Unable to load this student.");
  }
}

async function submitStudent(event) {
  event.preventDefault();
  formError.hidden = true;
  saveButton.disabled = true;
  saveButton.textContent = "Saving...";

  const student = Object.fromEntries(new FormData(form).entries());
  const isEditing = editingId !== null;

  try {
    const response = await fetch(isEditing ? `${apiUrl}/${editingId}` : apiUrl, {
      method: isEditing ? "PUT" : "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(student)
    });
    if (!response.ok) throw new Error(await getErrorMessage(response));
    dialog.close();
    await loadStudents(searchInput.value);
  } catch (error) {
    formError.textContent = error instanceof Error ? error.message : "Unable to save this student.";
    formError.hidden = false;
  } finally {
    saveButton.disabled = false;
    saveButton.textContent = editingId === null ? "Add student" : "Save changes";
  }
}

async function deleteStudent(id) {
  if (!window.confirm("Delete this student record? This action cannot be undone.")) return;
  hideNotice();
  try {
    const response = await fetch(`${apiUrl}/${id}`, { method: "DELETE" });
    if (!response.ok) throw new Error(await getErrorMessage(response));
    await loadStudents(searchInput.value);
  } catch (error) {
    showNotice(error instanceof Error ? error.message : "Unable to delete this student.");
  }
}

document.querySelector("#add-student").addEventListener("click", openCreateDialog);
form.addEventListener("submit", submitStudent);
document.querySelectorAll(".close-dialog").forEach((button) => {
  button.addEventListener("click", () => dialog.close());
});
searchInput.addEventListener("input", () => {
  clearTimeout(searchTimer);
  searchTimer = setTimeout(() => loadStudents(searchInput.value), 180);
});
rows.addEventListener("click", (event) => {
  const button = event.target.closest("button[data-action]");
  if (!button) return;
  if (button.dataset.action === "edit") openEditDialog(button.dataset.id);
  if (button.dataset.action === "delete") deleteStudent(button.dataset.id);
});
document.addEventListener("keydown", (event) => {
  if (event.key === "/" && document.activeElement !== searchInput && !dialog.open) {
    event.preventDefault();
    searchInput.focus();
  }
});

loadStudents();
