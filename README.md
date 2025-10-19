# GeoStud API

Web API для сбора и анализа данных студентов с поддержкой аутентификации, версионирования и аналитики.

## 🚀 Быстрый запуск

### 🏠 Локальная разработка (SQLite)

```bash
dotnet run --project GeoStud.Api.csproj -- --sqlite
```

### 🏭 Production (SQL Server)

```bash
dotnet run --project GeoStud.Api.csproj --environment Production
```

### 🎯 Режимы работы

**SQLite режим (локальная разработка):**
- База данных: `GeoStudLocal.db` (файл)
- Быстрый старт, не требует SQL Server
- CORS: разрешены все источники

**SQL Server режим (production):**
- База данных: SQL Server
- Полнофункциональная база данных
- CORS: ограниченные источники

### 📊 Доступ к API

- **Swagger UI**: `https://localhost:5001`
- **API**: `https://localhost:5001/api/v1/`
- **Health Check**: `https://localhost:5001/api/v1/health`

### 🔐 Тестовые данные

**Пользователь:**
- Username: `admin`
- Password: `Admin123!`

**Сервисные клиенты:**
- Client ID: `mobile-app`, Secret: `MobileAppSecret123!`
- Client ID: `web-app`, Secret: `WebAppSecret123!`

### 📝 Примеры запросов

**1. Аутентификация:**
```bash
curl -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}'
```

**2. Отправка опроса:**
```bash
curl -X POST https://localhost:5001/api/v1/survey/submit \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "ageRange": "17-22",
    "isStudent": true,
    "gender": "Male",
    "isLocal": true,
    "interests": ["Учеба", "Спорт"],
    "budget": "500",
    "activityTime": "Evening",
    "socialPreference": "Group"
  }'
```

**3. Получение аналитики:**
```bash
curl -X GET https://localhost:5001/api/v1/analytics/comprehensive \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## 📋 Основные API Endpoints

### Аутентификация
- `POST /api/v1/auth/login` - Вход пользователя
- `POST /api/v1/auth/service-login` - Вход сервиса

### Опросы студентов
- `POST /api/v1/survey/submit` - Отправить данные опроса
- `GET /api/v1/survey/{id}` - Получить данные опроса

### Аналитика
- `GET /api/v1/analytics/comprehensive` - Полная аналитика
- `GET /api/v1/analytics/demographics` - Демографическая аналитика
- `GET /api/v1/analytics/interests` - Аналитика интересов
- `GET /api/v1/analytics/behavior` - Аналитика поведения

## 🛠️ Разработка

### Требования
- .NET 9.0 SDK
- SQL Server LocalDB (для production режима)

### Установка
```bash
dotnet restore
```

### Тестирование
```bash
dotnet test
```

## 📁 Структура проекта

```
src/
├── Controllers/          # API контроллеры
├── Models/              # Модели данных
├── Data/                # Контекст базы данных
├── Services/            # Бизнес-логика
├── DTOs/                # Объекты передачи данных
├── Middleware/          # Пользовательские middleware
└── Examples/            # Примеры использования
```

## 🔧 Настройка

### Переменные окружения
- `ASPNETCORE_ENVIRONMENT` - среда выполнения
- `FORCE_SQLITE` - принудительное использование SQLite

### Конфигурация
- `src/appsettings.json` - основная конфигурация

## 📚 Дополнительная информация

- **Swagger UI**: Автоматически доступен в режиме разработки
- **Логирование**: Настроено для всех HTTP запросов
- **CORS**: Настроен для локальной разработки и production
- **База данных**: Автоматическое создание при первом запуске