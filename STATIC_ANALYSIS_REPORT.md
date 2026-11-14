# Static Analysis Report

Ось детальний опис інструментів, які ви використовували під час побудови системи статичного аналізу та тестування, відформатований у синтаксисі **Markdown** (з таблицею та списками), щоб його можна було одразу додати до файлу документації (`README.md` або звіту):

---

## Використані інструменти та їх призначення

| Інструмент | Призначення / Для чого використовувався |
| --- | --- |
| **Roslyn Analyzers** (`AnalysisLevel latest`) | Вбудований у .NET компіляторний аналізатор, який перевіряє код на наявність архітектурних проблем, порушень шаблонів проектування, антипатернів (наприклад, асинхронні проблеми, поганий витік ресурсів) та генерує попередження (`Warnings`) на етапі збірки. |
| **SonarAnalyzer.CSharp** | Розширений пакет статичного аналізу коду (розроблений SonarSource), який глибоко сканує проєкт на наявність «запахів коду» (`Code Smells`), багів, вразливостей безпеки та проблем із продуктивністю (наприклад, правила S3260, S1066, S6580). |
| **`.editorconfig`** | Конфігураційний файл для забезпечення єдиного стилю написання коду в команді (відступи, пробіли, правила nullability, переведення рядків `LF/CRLF`) для всіх розробників та середовищ розробки. |
| **`dotnet format`** | Утиліта командного рядка для автоматичного форматування коду та перевірки його відповідності правилам `.editorconfig`.
| **PowerShell Scripting** (`run-static-analysis.ps1`) | Скрипт для локальної оркестрації пайплайну: очищення проєкту, форматування, збірки з аналізаторами (`/p:ReportAnalyzer=true`), розрахунку метрик якості (Score) та формування звіту в терміналі. |
| **GitHub Actions** (`ci.yml`) | Налаштовано для автоматичного запуску кросплатформного білду (на Linux та Windows), перевірки стилю коду та прогону всіх юніт-тестів при кожному `push` або `pull_request`. |

---

## Код перед виправленнями

Було виявлено 0 помилок та 126 попереджень, таких як:

#### 1. Порушення патерну Dispose (S3881)

* **Причина:** Класи, що реалізують інтерфейс `IDisposable`, не дотримуються класичного паттерна вивільнення ресурсів із захищеним методом `Dispose(bool disposing)` та викликом `GC.SuppressFinalize(this)`.
* **Файли:**
* `MovieTracker.Tests/Services/RecommendationServiceTests.cs` (або пов'язані тестові фікстури).



#### 2. Проблеми з іменуванням та затіненням змінних / Неопечатані закриті класи (S3218, S3260)

* **Причина:** Вкладені або приватні класи не позначені як `sealed` (якщо вони не успадковуються), а також властивості локально затінюють поля зовнішнього класу.
* **Файли:**
* `src/MovieTracker.API/Services/RecommendationService.cs`
* `src/MovieTracker.API/Services/TasteProfileService.cs`
* `src/MovieTracker.UI/Services/TasteProfileInterpreter.cs`



#### 3. Використання Providers та парсинг без урахування локалі/формату (S6580)

* **Причина:** Парсинг дат і часу (`DateTime.Parse` тощо) виконується без передачі `IFormatProvider`, що може призводити до багів на різних локалях ОС.
* **Файли:**
* `src/MovieTracker.API/Services/TmdbService.cs`



#### 4. Неправильне використання маршрутизації та Null-Forgiving операторів (S6934, S8969)

* **Причина:** На рівнях контролерів пропущені явні атрибути `RouteAttribute` для дій, або використовуються зайві оператори `!` (null-forgiving), хоча компілятор і так знає про не-null значення.
* **Файли:**
* `src/MovieTracker.API/Controllers/CollectionsController.cs`
* `src/MovieTracker.API/Controllers/UsersController.cs`
* `MovieTracker.Tests/Services/AuthControllerTests.cs`
* `MovieTracker.Tests/Services/UsersControllerTests.cs`
* `MovieTracker.Tests/Services/TasteProfileControllerTests.cs`
* `MovieTracker.Tests/Controllers/ReccomendationControllerTests.cs`



#### 5. Архітектурні зауваження та асинхронні антипатерни у Program.cs / API (S1118, S6966)

* **Причина:** Статичні класи/точки входу не мають закритих конструкторів, а також використовуються синхронні виклики міграцій БД (`EnsureCreated`, `Migrate`) замість асинхронних аналогів із префіксом `Async`.
* **Файли:**
* `src/MovieTracker.API/Program.cs`
* `src/MovieTracker.UI/Platforms/iOS/Program.cs`
* `src/MovieTracker.UI/Platforms/MacCatalyst/Program.cs`



#### 6. Порожні блоки перехоплення винятків та структурні дублювання (S108, S1066)

* **Причина:** Залишені порожні блоки `catch` (або з однією точкою з запятою), а також вкладені блоки `if`, які можна об'єднати в один логічний вираз.
* **Файли:**
* `src/MovieTracker.UI/Services/AuthService.cs`
* `src/MovieTracker.UI/Views/MainPage.xaml.cs`



#### 7. Методи, які мають бути статичними, та хардкод шляхів (S2325, S1075)

* **Причина:** Методи сервісів не використовують стан екземпляра класу, але не позначені як `static`. Також у коді використовуються жорстко закодовані абсолютні шляхи чи URI.
* **Файли:**
* `src/MovieTracker.UI/Services/TasteProfileInterpreter.cs`
* `src/MovieTracker.UI/MauiProgram.cs`



#### 8. Стилістичні помилки форматування та закінчення рядків (ENDOFLINE / WHITESPACE / .editorconfig)

* **Причина:** Неузгодженість символів переведення рядків (CRLF замість LF у середовищі Windows/Linux) та порушення відступів, через що утиліта `dotnet format` фіксувала помилки.
* **Файли:**
* `MovieTracker.Tests/TestWebApplicationFactory.cs`
* `src/MovieTracker.API/Controllers/ReviewsController.cs`
* `src/MovieTracker.API/Program.cs`
* `src/MovieTracker.API/Services/TasteProfileService.cs`
* `src/MovieTracker.API/Services/TmdbService.cs`
* `src/MovieTracker.Core/Dtos/TasteProfileDto.cs`
* `src/MovieTracker.Core/Models/Genre.cs`
* `src/MovieTracker.Core/Models/TasteProfile.cs`
* `src/MovieTracker.UI/App.xaml.cs`
* `src/MovieTracker.UI/AppShell.xaml.cs`
* `src/MovieTracker.UI/MauiProgram.cs`
* `src/MovieTracker.UI/Platforms/Windows/App.xaml.cs`
* `src/MovieTracker.UI/Services/ApiService.cs`
* `src/MovieTracker.UI/ViewModels/MainViewModel.cs`

Після виправлень виявлено 0 помилок та 0 зауважень. Отже, виправлення були успішними.

---

## Інструкція з запуску статичного аналізу коду

### 1. Локальний запуск через PowerShell-скрипт (Рекомендовано)

Для швидкої локальної перевірки всього проєкту ви можете скористатися спеціально підготовленим скриптом автоматизації в корені репозиторію для Windows:

```
.\run-static-analysis.ps1

```

Цей скрипт послідовно виконує:

1. Очищення проєкту від попередніх збірок.
2. Перевірку та автоматичне форматування коду через `dotnet format`.
3. Повну збірку розв'язку разом із запуском Roslyn та SonarAnalyzer із генерацією звітом.

### 2. Ручний запуск кроків статичного аналізу через термінал

Якщо ви хочете виконати перевірки вручну покроково:

* **Перевірка відповідності стилю коду (.editorconfig):**
```bash
dotnet format --verify-no-changes --verbosity minimal

```

* **Збірка проєкту звітністю аналізаторів:**
```bash
dotnet build /p:ReportAnalyzer=true --configuration Release

```

### 3. Автоматичний статичний аналіз у CI/CD (GitHub Actions)

Статичний аналіз разом із перевіркою форматування автоматично виконується на кожному `push` або `pull_request` у гілку репозиторію через налаштований пайплайн (`ci.yml`).