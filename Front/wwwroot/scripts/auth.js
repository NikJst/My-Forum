console.log("auth.js загружен");
import { API_BASE_URL } from "./config.js";

function apiFetch(path, options) {
  return fetch(`${API_BASE_URL}${path}`, options);
}

// (Удалено дублирующееся определение showUserInfo)

function hideUserInfo() {
  document.getElementById("user-name").textContent = "";
  document.getElementById("user-info").style.display = "none";
  document.getElementById("auth-buttons").style.display = "flex";
  localStorage.removeItem("token");
  localStorage.removeItem("username");

  document.getElementById("username").style.display = "inline-block";
  document.getElementById("password").style.display = "inline-block";
}

// (Удалено дублирующееся определение submitLogin)

function submitRegister() {
  const username = document.getElementById("reg-username").value;
  const password = document.getElementById("reg-password").value;

  apiFetch("/api/auth/register", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password }),
  })
    .then((res) => res.json())
    .then((data) => {
      if (data.success || data.token) {
        alert("Регистрация успешна!");
        hideRegister();
      } else {
        alert("Ошибка регистрации");
      }
    })
    .catch((err) => {
      console.error("Ошибка регистрации:", err);
      alert("Ошибка регистрации");
    });
}

function hideLogin() {
  document.getElementById("login-form").style.display = "none";
}

function hideRegister() {
  document.getElementById("register-form").style.display = "none";
}

function hideAuthForms() {
  hideLogin();
  hideRegister();
}

document.title = "Форум в реальном времени";

// Функция для отображения блока с именем пользователя и скрытия кнопок входа/регистрации
function showUserInfo(username) {
  document.getElementById("user-name").textContent = username;
  document.getElementById("user-info").style.display = "flex";
  document.getElementById("auth-buttons").style.display = "none";

  // Скрываем поля логина и пароля в шапке
  document.getElementById("username").style.display = "none";
  document.getElementById("password").style.display = "none";
}

function submitLogin() {
  const username = document.getElementById("username").value;
  const password = document.getElementById("password").value;

  apiFetch("/api/auth/login", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password }),
  })
    .then((res) => res.json())
    .then((data) => {
      if (data.token) {
        localStorage.setItem("username", username);
        localStorage.setItem("token", data.token);
        showUserInfo(username);
        alert("Вы вошли!");
      } else {
        alert("Ошибка входа");
      }
    })
    .catch((err) => {
      console.error("Ошибка авторизации:", err);
      alert("Ошибка авторизации");
    });
}
// Показывает форму входа
// Эта функция вызывается, когда пользователь нажимает кнопку "Войти"
// (реализация login() уже выше)
// Скрывает форму входа

// Функция для отправки данных регистрации на сервер
// Она берёт значения из полей формы регистрации и отправляет их на сервер

//===
// (Обработчик для input теперь назначается внутри DOMContentLoaded)

// Этот код добавляет обработчики событий для кнопок "Войти" и "Регистрация"
// (Удалено: дублирующие обработчики регистрации/входа)

// Функции для обработки кликов по кнопкам "Войти" и "Регистрация"
// Они просто показывают окно с формой входа или регистрации

// (Удалено: функция register, не используется)
// Обработчик кнопки "Выйти" - скрывает информацию о пользователе и очищает сессию

// === Назначение обработчиков после загрузки DOM ===
document.addEventListener("DOMContentLoaded", () => {
  console.log("DOM полностью загружен");

  const loginButton = document.getElementById("login-button");
  console.log("Найден элемент login-button?", loginButton);
  if (loginButton) {
    loginButton.addEventListener("click", async () => {
      console.log("Нажата кнопка входа");

      const username = document.getElementById("username").value.trim();
      const password = document.getElementById("password").value;

      if (!username || !password) {
        alert("Введите имя пользователя и пароль.");
        return;
      }

      try {
        const response = await apiFetch("/api/auth/login", {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({ username, password }),
        });

        if (response.ok) {
          const data = await response.json();

          localStorage.setItem("token", data.token);
          localStorage.setItem("username", username);

          showUserInfo(username);
          alert("Вы вошли!");
        } else {
          const error = await response.text();
          alert("Ошибка входа: " + error);
        }
      } catch (err) {
        console.error("Ошибка запроса:", err);
        alert("Ошибка подключения к серверу.");
      }
    });
  }

  const registerButton = document.getElementById("register-button");
  if (registerButton) {
    registerButton.addEventListener("click", () => {
      window.location.href = "/register.html";
    });
  }

  const logoutButton = document.getElementById("logout-button");
  if (logoutButton) {
    logoutButton.addEventListener("click", () => {
      localStorage.removeItem("token");
      localStorage.removeItem("username");
      showAuthFields();
      alert("Вы вышли из системы");
    });
  }

  const input = document.getElementById("input");
});

function showAuthFields() {
  document.getElementById("auth-buttons").style.display = "flex";
  document.getElementById("user-info").style.display = "none";
  document.getElementById("username").style.display = "inline-block";
  document.getElementById("password").style.display = "inline-block";
}
