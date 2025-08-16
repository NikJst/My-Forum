const API_BASE_URL = "http://localhost:8000";

function apiFetch(path, options) {
  return fetch(`${API_BASE_URL}${path}`, options);
}

function sendMessage() {
  const content = document.getElementById("input").value; // Этот метод отправляет сообщение на сервер
  apiFetch(`/api/posts/thread/${threadId}`, {
    // Этот метод отправляет сообщение на сервер
    // fetch - прямо указывает путь на контроллер.
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: "Bearer " + localStorage.getItem("token"), //берем токен из localStorage
    },
    body: JSON.stringify({ content }),
  })
    .then((res) => {
      if (!res.ok) {
        alert("Ошибка отправки. Проверь токен и ID темы.");
        console.error("Ошибка POST запроса:", res);
      } else {
        document.getElementById("input").value = "";
      }
    })
    .catch((err) => {
      console.error("Ошибка при отправке запроса:", err);
    });
}

if (input) {
  input.addEventListener("keydown", function (event) {
    if (event.key === "Enter") {
      event.preventDefault();
      sendMessage();
    }
  }); // --> код чата
}

const chatArea = document.getElementById("chat-area");
if (chatArea) chatArea.style.display = "block"; // --> код чата

// document.getElementById("chat-area").style.display = "none"; // --> код чата
