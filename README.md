# MovieTracker — Мультиплатформний сервіс трекінгу фільмів з аналізом смакового профілю

## Стек:
- **Frontend:** .NET MAUI (C# / XAML), шаблон розробки MVVM
- **Backend:** ASP.NET Core Web API
- **Database:** SQLite, Entity Framework Core
- **Зовнішні інтеграції:** TMDB (The Movie Database) API для автоматичного завантаження даних
- **Аутентифікація:** JWT (JSON Web Tokens) та ASP.NET Core Identity

## Вимоги для встановлення:
Перед початком переконайтеся, що на вашому комп'ютері встановлено:
1. .NET 10 SDK.
2. Для роботи з MAUI встановіть необхідні компоненти через термінал:
3. Для роботи з Android:
   - Встановіть компоненти **Android SDK**, **OpenJDK** та **Android Emulator** через Visual Studio Installer (у робочому навантаженні .NET MAUI) або за допомогою Android Studio.
   - Створіть принаймні один віртуальний пристрій (AVD) через **Android Device Manager** у Visual Studio (`Tools > Android > Android Device Manager`).
```
dotnet workload install maui
```

## Інструкція запуску:
Усі команди виконуються у терміналі MovieTracker

### 1. Клонування репозиторію
```
git clone https://github.com/htokira/MovieTracker.git
cd MovieTracker
```

### 2. Налаштування секретів
```
dotnet user-secrets set "Jwt:Key" "<будь-які 32 символи>" --project src/MovieTracker.API
dotnet user-secrets set "Tmdb:ApiKey" "<ваш API ключ від бази даних TMDB>" --project src/MovieTracker.API
```

### 3. Підтягування даних з БД
```
dotnet ef database update --project MovieTracker.Data --startup-project src/MovieTracker.API/MovieTracker.API.csproj
```

### 4. Запуск сервера
```
dotnet run --project src/MovieTracker.API
```
API слухає 'https://localhost:7226'

### 5. Запуск клієнта

* **Варіант А: Для Windows (Desktop)**
Виконуйте у новому терміналі:
```bash
dotnet build src/MovieTracker.UI/MovieTracker.UI.csproj -t:Run -f net10.0-windows10.0.19041.0

```

* **Варіант Б: Для Android (Емулятор)**
1. Запустіть Android-емулятор через Visual Studio (Android Device Manager) або скористайтеся інструментом командного рядка `emulator`.
2. Виконайте у новому терміналі команду для збирання та запуску під Android:
```bash
dotnet build src/MovieTracker.UI/MovieTracker.UI.csproj -t:Run -f net10.0-android

```