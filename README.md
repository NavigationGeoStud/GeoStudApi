# GetStud API

ASP.NET Core Web API проект с интегрированным мониторингом, health checks и Aspire Dashboard.

## 🚀 Возможности

- ✅ **ASP.NET Core 9.0** Web API
- ✅ **Health Checks** с красивым UI
- ✅ **Aspire Dashboard** для централизованного мониторинга
- ✅ **Serilog** структурированное логирование
- ✅ **Метрики производительности** в реальном времени
- ✅ **OpenAPI/Swagger** документация
- ✅ **HTTPS** поддержка

## 🏥 Health Checks

Проект включает следующие health checks:

| Health Check | Описание | Endpoint |
|-------------|----------|----------|
| **Basic** | Базовая проверка работоспособности приложения | `/health` |
| **Memory** | Мониторинг использования памяти (лимит: 1GB) | `/health` |
| **API** | Проверка отзывчивости API (лимит: 1 сек) | `/health` |
| **Dependencies** | Проверка внешних зависимостей (HTTP, файлы, env) | `/health` |

## 🚀 Запуск проекта

### Вариант 1: Aspire Dashboard (Рекомендуется)

```bash
# Запуск с Aspire Dashboard
dotnet run --project GetStud.AppHost
```

**Что запустится:**
- 🌐 **GetStud API**: https://localhost:7281
- 📊 **Aspire Dashboard**: https://localhost:18888
- 🏥 **Health Checks UI**: https://localhost:7281/health-ui

### Вариант 2: Только API

```bash
# Запуск только API проекта
dotnet run --project GetStud.Api
```

## 📍 Endpoints

| Endpoint | Описание | URL |
|----------|----------|-----|
| **API** | Основной API | https://localhost:7281 |
| **Health Check** | JSON статус всех проверок | https://localhost:7281/health |
| **Health UI** | Веб-интерфейс мониторинга | https://localhost:7281/health-ui |
| **Swagger** | API документация | https://localhost:7281/swagger |
| **Aspire Dashboard** | Централизованный мониторинг | https://localhost:18888 |

## 📊 Мониторинг

### Health Checks UI
Доступен по адресу `/health-ui` и предоставляет:
- 🟢 **Статус** всех health checks в реальном времени
- 📈 **История** проверок с графиками
- 🔍 **Детальная информация** по каждому check
- 🎨 **Красивый UI** с адаптивным дизайном

### Aspire Dashboard
При запуске через AppHost предоставляет:
- 🏠 **Обзор сервисов** и их статус
- 📊 **Метрики производительности** в реальном времени
- 📝 **Логи** с фильтрацией и поиском
- 🔍 **Трассировка запросов** end-to-end
- 📈 **Графики** использования ресурсов

## 📝 Логирование

**Serilog** настроен для записи логов в:
- 🖥️ **Консоль** - для разработки
- 📁 **Файлы** - в папке `logs/` с ротацией по дням

**Формат логов:**
```
[2024-01-15 10:30:45] [Information] API Health Check completed in 15.2ms
[2024-01-15 10:30:45] [Information] Dependencies Health Check: 3 healthy, 0 failed
```

## 🏗️ Структура проекта

```
GetStud/
├── 📁 GetStud.Api/                 # Основной API проект
│   ├── 📁 Controllers/             # API контроллеры
│   │   └── WeatherForecastController.cs
│   ├── 📁 HealthChecks/           # Health check реализации
│   │   ├── BasicHealthCheck.cs
│   │   ├── MemoryHealthCheck.cs
│   │   ├── ApiHealthCheck.cs
│   │   └── DependenciesHealthCheck.cs
│   ├── 📁 wwwroot/               # Статические файлы
│   ├── Program.cs                 # Конфигурация приложения
│   └── GetStud.Api.csproj        # Зависимости проекта
├── 📁 GetStud.AppHost/           # Aspire Host проект
│   ├── Program.cs                # Конфигурация Aspire
│   └── GetStud.AppHost.csproj   # Aspire зависимости
└── GetStud.sln                   # Solution файл
```

## 🔧 Технические детали

### Зависимости
- **.NET 9.0** - последняя версия фреймворка
- **Microsoft.AspNetCore.OpenApi** - Swagger документация
- **Microsoft.Extensions.Diagnostics.HealthChecks** - Health checks
- **AspNetCore.HealthChecks.UI** - Веб-интерфейс для health checks
- **Serilog.AspNetCore** - Структурированное логирование
- **Aspire.Hosting.AppHost** - Aspire Dashboard

### Конфигурация
- **Порты**: HTTP (5032), HTTPS (7281)
- **Окружение**: Development
- **Логирование**: Information level
- **Health Checks**: Каждые 15 секунд

## 🛠️ Разработка

### Рекомендуемый workflow:
1. **Запустите** проект через Aspire Dashboard
2. **Откройте** Aspire Dashboard для мониторинга
3. **Используйте** Health UI для проверки статуса
4. **Просматривайте** логи в реальном времени

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
