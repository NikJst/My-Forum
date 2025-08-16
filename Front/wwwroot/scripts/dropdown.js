// Поиск и выпадающий список веток
let threadDropdown = null;
function ensureThreadDropdown() {
  if (!threadDropdown) {
    threadDropdown = document.createElement("div");
    threadDropdown.id = "thread-dropdown";
    threadDropdown.style.position = "absolute";
    threadDropdown.style.background = "#222";
    threadDropdown.style.color = "#fff";
    threadDropdown.style.border = "1px solid #444";
    threadDropdown.style.borderRadius = "8px";
    threadDropdown.style.zIndex = "1000";
    threadDropdown.style.width = "80%";
    threadDropdown.style.left = "10%";
    threadDropdown.style.top = "70px";
    threadDropdown.style.maxHeight = "250px";
    threadDropdown.style.overflowY = "auto";
    threadDropdown.style.display = "none";
    threadDropdown.style.boxShadow = "0 4px 16px rgba(0,0,0,0.4)";
    document.body.appendChild(threadDropdown);
  }
}
