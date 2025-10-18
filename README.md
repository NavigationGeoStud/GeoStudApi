# GetStud API

Простой ASP.NET Core Web API проект с базовым health check и Aspire Dashboard.

## 🚀 Возможности

- ✅ **ASP.NET Core 9.0** Web API
- ✅ **Простой Health Check** - базовая проверка работоспособности
- ✅ **Aspire Dashboard** для мониторинга
- ✅ **OpenAPI/Swagger** документация
- ✅ **HTTPS** поддержка

## 🏥 Health Check

Проект включает простой health check:

| Health Check | Описание | Endpoint |
|-------------|----------|----------|
| **Basic** | Базовая проверка работоспособности приложения | `/health` |

## 🚀 Запуск проекта

### Вариант 1: Aspire Dashboard (Рекомендуется)

```bash
# Запуск с Aspire Dashboard
dotnet run --project GetStud.AppHost
```

**Что запустится:**
- 🌐 **GetStud API**: https://localhost:7281
- 📊 **Aspire Dashboard**: https://localhost:18888

### Вариант 2: Только API

```bash
# Запуск только API проекта
dotnet run --project GetStud.Api
```

## 📍 Endpoints

| Endpoint | Описание | URL |
|----------|----------|-----|
| **API** | Основной API | https://localhost:7281 |
| **Health Check** | JSON статус проверки | https://localhost:7281/health |
| **Swagger** | API документация | https://localhost:7281/swagger |
| **Aspire Dashboard** | Централизованный мониторинг | https://localhost:18888 |

## 📊 Мониторинг

### Aspire Dashboard
При запуске через AppHost предоставляет:
- 🏠 **Обзор сервисов** и их статус
- 📊 **Метрики производительности** в реальном времени
- 📝 **Логи** с фильтрацией и поиском
- 🔍 **Трассировка запросов** end-to-end

## 🏗️ Структура проекта

```
GetStud/
├── 📁 GetStud.Api/                 # Основной API проект
│   ├── 📁 Controllers/             # API контроллеры
│   │   └── WeatherForecastController.cs
│   ├── Program.cs                  # Конфигурация приложения
│   └── GetStud.Api.csproj         # Зависимости проекта
├── 📁 GetStud.AppHost/            # Aspire Host проект
│   ├── Program.cs                 # Конфигурация Aspire
│   └── GetStud.AppHost.csproj    # Aspire зависимости
└── GetStud.sln                    # Solution файл
```

## 🔧 Технические детали

### Зависимости
- **.NET 9.0** - последняя версия фреймворка
- **Microsoft.AspNetCore.OpenApi** - Swagger документация
- **Aspire.Hosting.AppHost** - Aspire Dashboard

### Конфигурация
- **Порты**: HTTP (5032), HTTPS (7281)
- **Окружение**: Development

## 🛠️ Разработка

### Рекомендуемый workflow:
1. **Запустите** проект через Aspire Dashboard
2. **Откройте** Aspire Dashboard для мониторинга
3. **Проверьте** health check по адресу `/health`

### Полезные команды:
```bash
# Восстановление пакетов
dotnet restore

# Сборка проекта
dotnet build

# Запуск тестов (если будут добавлены)
dotnet test

# Очистка
dotnet clean
```

## 📋 TODO / Планы развития

- [ ] Добавить базу данных (Entity Framework Core)
- [ ] Реализовать аутентификацию (JWT)
- [ ] Добавить валидацию моделей (FluentValidation)
- [ ] Создать unit тесты
- [ ] Добавить интеграционные тесты
- [ ] Настроить CI/CD pipeline
- [ ] Добавить Docker поддержку

## 🤝 Вклад в проект

1. Fork проекта
2. Создайте feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit изменения (`git commit -m 'Add some AmazingFeature'`)
4. Push в branch (`git push origin feature/AmazingFeature`)
5. Откройте Pull Request

## 📄 Лицензия

Этот проект использует MIT лицензию. См. файл `LICENSE` для деталей.

---

**Создано с ❤️ для мониторинга и здоровья вашего API**
