const API_BASE_URL = "http://localhost:8000";

function apiFetch(path, options) {
  return fetch(`${API_BASE_URL}${path}`, options);
}

// Этот файл содержит логику для работы с ветками форума
// Он управляет загрузкой веток
document.addEventListener("DOMContentLoaded", () => {
  const savedUsername = localStorage.getItem("username");
  const savedToken = localStorage.getItem("token");
  if (savedUsername && savedToken) {
    showUserInfo(savedUsername); // здесь уже скрывается форма и показывается инфо
  }
  initThreads();
});

function joinThread(newThreadId) {
  if (threadId === newThreadId) return;

  connection.invoke("LeaveThread", threadId).catch(() => {});

  threadId = newThreadId;
  document.getElementById("chat-area").style.display = "block";
  document.getElementById("search-area").style.display = "flex";

  const messagesDiv = document.getElementById("messages");
  const currentThread = allThreads.find((t) => t.id.toString() === newThreadId);
  if (currentThread) {
    addToVisitedThreads(currentThread);
    renderVisitedThreads();
  }
  // Очищаем интерфейс сообщений перед загрузкой новой ветки
  messagesDiv.innerHTML = "";

  // Загружаем историю сообщений из API
  apiFetch(`/api/posts/thread/${newThreadId}`, {
    headers: {
      Authorization: "Bearer " + localStorage.getItem("token"),
    },
  })
    .then((res) => res.json())
    .then((posts) => {
      posts.forEach((post) => {
        const div = document.createElement("div");
        div.textContent = `${post.username}: ${post.content}`;
        div.style.color = "white";
        messagesDiv.appendChild(div);
      });
      messagesDiv.scrollTop = messagesDiv.scrollHeight;
    });

  connection.invoke("JoinThread", threadId).catch(console.error);
  localStorage.setItem("lastThreadId", newThreadId);
  const threadTitleDiv = document.getElementById("thread-title");
  threadTitleDiv.textContent = currentThread ? currentThread.title : "";
}

function loadThreads() {
  // Получить список всех веток и сохранить в allThreads
  const token = localStorage.getItem("token");
  apiFetch(`/api/threads`, {
    headers: {
      Authorization: "Bearer " + token,
    },
  })
    .then((res) => {
      if (!res.ok) throw new Error("Ошибка загрузки веток");
      return res.json();
    })
    .then((threads) => {
      allThreads = threads;
    })
    .catch((err) => {
      console.error(err);
      alert("Не удалось загрузить ветки");
    });
}

function showThreadDropdown(threads) {
  ensureThreadDropdown();
  threadDropdown.innerHTML = "";
  if (threads.length === 0) {
    const empty = document.createElement("div");
    empty.textContent = "Нет совпадений";
    empty.style.padding = "10px";
    threadDropdown.appendChild(empty);
  } else {
    threads.forEach((thread) => {
      const el = document.createElement("div");
      el.textContent = thread.title;
      el.style.padding = "10px";
      el.style.cursor = "pointer";
      el.addEventListener("click", () => {
        joinThread(thread.id.toString());
        hideThreadDropdown();
        document.getElementById("thread-search").value = "";
      });
      el.addEventListener("mouseover", () => {
        el.style.background = "#444";
      });
      el.addEventListener("mouseout", () => {
        el.style.background = "";
      });
      threadDropdown.appendChild(el);
    });
  }

  // Позиционируем под полем поиска
  const searchInput = document.getElementById("thread-search");
  const rect = searchInput.getBoundingClientRect();
  threadDropdown.style.top = window.scrollY + rect.bottom + "px";
  threadDropdown.style.left = window.scrollX + rect.left + "px";
  threadDropdown.style.width = rect.width + "px";
  threadDropdown.style.display = "block";
}

function hideThreadDropdown() {
  ensureThreadDropdown();
  threadDropdown.style.display = "none";
}

// Обработчик поиска по веткам
document.addEventListener("DOMContentLoaded", () => {
  ensureThreadDropdown();

  // Загрузить ветки при первом открытии поиска
  loadThreads();
  const searchInput = document.getElementById("thread-search");
  searchInput.addEventListener("input", (e) => {
    const query = e.target.value.trim().toLowerCase();
    if (!query) {
      hideThreadDropdown();
      return;
    }
    const filtered = allThreads.filter((t) =>
      t.title.toLowerCase().includes(query)
    );
    showThreadDropdown(filtered);
  });
  // Скрывать dropdown при клике вне
  document.addEventListener("click", (e) => {
    if (
      threadDropdown &&
      !threadDropdown.contains(e.target) &&
      e.target.id !== "thread-search"
    ) {
      hideThreadDropdown();
    }
  });
  // Показывать поиск снова при фокусе
  searchInput.addEventListener("focus", () => {
    if (searchInput.value.trim()) {
      const query = searchInput.value.trim().toLowerCase();
      const filtered = allThreads.filter((t) =>
        t.title.toLowerCase().includes(query)
      );
      showThreadDropdown(filtered);
    }
  });
});

//===> отрисовка
// Если есть параметр thread в URL, присоединяемся к этой ветке
const urlParams = new URLSearchParams(window.location.search);
threadId = urlParams.get("thread") || null;
if (!threadId) {
  threadId = localStorage.getItem("lastThreadId");
}
if (threadId) {
  joinThread(threadId);
}
renderVisitedThreads();

// Отключаем переходы по ссылкам веток и всегда используем threadId = "1"
// document.querySelectorAll('#sidebar ul li a').forEach(link => {
//   if (link.getAttribute('href') && link.getAttribute('href').startsWith('?thread=')) {
//     link.setAttribute('href', '#');
//   }
// });
let threadId = null;

// Глобальная переменная для всех веток
let allThreads = [];

function addToVisitedThreads(thread) {
  let visited = JSON.parse(localStorage.getItem("visitedThreads") || "[]");

  // Убираем дубли по id
  visited = visited.filter((t) => t.id !== thread.id);

  // Добавляем новую ветку в начало
  visited.unshift(thread);

  // Ограничиваем список, например, до 10
  if (visited.length > 10) visited.pop();

  localStorage.setItem("visitedThreads", JSON.stringify(visited));
}

function renderVisitedThreads() {
  const visitedList = document.getElementById("visited-list");
  visitedList.innerHTML = "";

  const visited = JSON.parse(localStorage.getItem("visitedThreads") || "[]");

  if (visited.length === 0) {
    visitedList.innerHTML = "<li>История пуста</li>";
    return;
  }

  visited.forEach((thread, index) => {
    const container = document.createElement("span");

    const titleSpan = document.createElement("span");
    titleSpan.textContent = thread.title;
    titleSpan.style.cursor = "pointer";
    titleSpan.style.marginRight = "4px";
    titleSpan.addEventListener("click", () => {
      joinThread(thread.id.toString());
    });
    titleSpan.addEventListener("mouseenter", () => {
      titleSpan.style.textDecoration = "underline";
    });
    titleSpan.addEventListener("mouseleave", () => {
      titleSpan.style.textDecoration = "none";
    });

    const deleteBtn = document.createElement("button");
    deleteBtn.textContent = "✕";
    deleteBtn.title = "Удалить из истории";
    deleteBtn.style.marginRight = "8px";
    deleteBtn.style.background = "transparent";
    deleteBtn.style.border = "none";
    deleteBtn.style.color = "#f44";
    deleteBtn.style.cursor = "pointer";
    deleteBtn.addEventListener("click", () => {
      visited.splice(index, 1);
      localStorage.setItem("visitedThreads", JSON.stringify(visited));
      renderVisitedThreads();
    });

    container.appendChild(titleSpan);
    container.appendChild(deleteBtn);

    visitedList.appendChild(container);

    if (index !== visited.length - 1) {
      visitedList.appendChild(document.createTextNode(", "));
    }
  });
}
