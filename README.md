# Napominator

![Version](https://img.shields.io/badge/version-15.11.2025-blue)
![.NET](https://img.shields.io/badge/.NET-9.0-green)
![Language](https://img.shields.io/badge/language-C%23-green)
![Platform](https://img.shields.io/badge/platform-Windows-blue)

**Напоминатор** — это приложение на Windows для контроля и мониторинга использования компьютера. Позволяет настраивать напоминания, блокировать доступ к приложениям по расписанию и отслеживать активность пользователя.

**Napominator** is a Windows application designed to periodically remind users about tasks and control access to specific applications based on customizable allow/block lists and time schedules.

## ?? Ключевые возможности / Key Features

### Напоминания / Reminders
- ?? Периодические уведомления о задачах и расписании
- Настраиваемые интервалы напоминаний
- Поддержка пользовательских сообщений

### Контроль приложений / Application Control
- ?? Блокировка доступа к приложениям на основе черного/белого списков
- ? Контроль по расписанию (разрешенное/запрещенное время)
- ?? Мониторинг активного окна в реальном времени
- Поддержка исключений для определенных приложений

### Мониторинг шума / Noise Monitoring
- ?? Детектор шума через микрофон
- ?? Автоматические предупреждения при превышении уровня громкости
- ?? Снятие скриншотов при срабатывании сигнала
- ?? Автоматическая блокировка рабочей станции при необходимости

### Мониторинг активности / Activity Monitoring
- ?? Захват скриншотов по клику мыши или по расписанию
- ?? Интеграция с веб-камерой (через EmguCV)
- ?? Подробное логирование всех событий
- ?? Анализ и хранение истории действий

### Профили пользователей / User Profiles
- ??????????? Поддержка нескольких профилей (например: "Polina", "Mama", "Papa")
- Индивидуальные настройки и ограничения для каждого пользователя
- Разные правила контроля в зависимости от профиля

## ?? Быстрый старт / Quick Start

### Требования / Requirements
- Windows 10 / Windows 11
- .NET 9 Runtime
- Микрофон (для функции мониторинга шума)
- Веб-камера (для снимков при срабатывании сигнала)

### Установка / Installation

1. **Клонируйте репозиторий / Clone the repository**
   ```bash
   git clone https://github.com/dda-dream/Napominator.git
   cd Napominator
   ```

2. **Соберите проект / Build the project**
   ```bash
   dotnet build
   ```

3. **Запустите приложение / Run the application**
   ```bash
   dotnet run --project WinFormsApp1/Napominator.csproj
   ```

### Конфигурация / Configuration

1. Откройте директорию `SETTINGS`
2. Отредактируйте файл `Settings.txt` с необходимыми параметрами:
   - `[Shuminator_Enabled]` - включить мониторинг шума (0/1)
   - `[Shuminator_MicrophoneName]` - имя микрофона
   - `[BlockChrome]` - блокировать браузеры (0/1)
   - `[allowed time from]` / `[allowed time to]` - разрешенное время использования
   - `[Blocklist]` - список запрещенных приложений
   - `[ExcludeFromBlock]` - исключения из блокировки

3. Используйте PowerShell скрипт для кодировки файлов:
   ```powershell
   .\ConvertToUtf8WithBom.ps1
   ```

## ?? Типичные сценарии использования / Typical Use Cases

- ? Напомнить ребенку делать перерывы во время учебы
- ? Заблокировать доступ к браузерам и играм вне учебного времени
- ? Оповестить или заблокировать рабочую станцию при обнаружении шума ночью
- ? Контролировать время работы на компьютере
- ? Записывать скриншоты для мониторинга активности

## ??? Архитектура / Architecture

```
Napominator/
??? WinFormsApp1/
?   ??? MainForm.cs              # Основная форма приложения
?   ??? Program.cs               # Точка входа
?   ??? Message_To_Polina.cs     # Форма уведомлений
?   ??? IpInfo.cs                # Работа с IP информацией
?   ??? GlobalMouseHook.cs       # Глобальный перехват мыши
?   ??? LogController.cs         # Управление логированием
?   ??? Napominator.csproj       # Файл проекта
??? SETTINGS/
?   ??? Settings.txt             # Файл конфигурации
??? sounds/                      # Звуковые файлы для уведомлений
??? README.md                    # Этот файл
```

## ?? Технологический стек / Tech Stack

| Компонент | Версия | Назначение |
|-----------|--------|-----------|
| **.NET** | 9.0 | Платформа |
| **C#** | 13.0 | Язык программирования |
| **WinForms** | - | GUI фреймворк |
| **NAudio** | Latest | Работа с аудио и микрофоном |
| **EmguCV** | Latest | Интеграция с веб-камерой |
| **PowerShell** | - | Скрипты для конфигурации |

## ?? Файл конфигурации / Configuration File

Пример `Settings.txt`:
```ini
[Shuminator_Enabled]=1
[Shuminator_MicrophoneName]=Microphone
[BlockChrome]=1
[allowed time from]=09:00
[allowed time to]=18:00
[Blocklist]=discord,telegram,youtube
[ExcludeFromBlock]=visual studio
[ShuminatorPlaySoundWarning]=1
[LockWorkStation]=1
[Proxy_IP]=http://proxy:8888
```

## ?? Безопасность / Security

- Приложение требует прав администратора для блокировки рабочей станции
- Логи сохраняются локально
- Данные о профилях хранятся в конфигурационных файлах
- Используется Windows API для контроля окон

## ??? Разработка / Development

### Требования к разработке / Development Requirements
- Visual Studio 2022 или выше
- .NET 9 SDK
- Git

### Запуск в режиме отладки / Debug Mode
```bash
dotnet run --project WinFormsApp1/Napominator.csproj --configuration Debug
```

### Сборка Release версии / Build Release
```bash
dotnet build -c Release
```

## ?? Логирование / Logging

Приложение ведет подробные логи:
- `EventLog` - системные события (Windows Event Log)
- Текстовое поле логов в приложении
- Время, действие и результат каждого события

## ?? Горячие клавиши / Keyboard Shortcuts

| Комбинация | Действие |
|-----------|----------|
| `Ctrl + X` | Закрыть приложение (только для администратора) |

## ?? Дополнительно / Additional Notes

- Приложение использует глобальный хук мыши для захвата скриншотов
- Некоторые функции требуют прав администратора
- Микрофон должен быть подключен для функции мониторинга шума
- Поддерживает Cyrillic (русский) текст в уведомлениях

## ?? Вклад / Contributing

Если вы хотите внести вклад:
1. Форкните репозиторий
2. Создайте ветку для функции (`git checkout -b feature/amazing-feature`)
3. Коммитьте изменения (`git commit -m 'Add some amazing feature'`)
4. Запушьте в ветку (`git push origin feature/amazing-feature`)
5. Откройте Pull Request

## ?? Лицензия / License

Этот проект распространяется под лицензией MIT. Смотрите файл `LICENSE` для деталей.

## ????? Автор / Author

Разработано [dda-dream](https://github.com/dda-dream)

## ?? Контакты / Contacts

- GitHub Issues для сообщения об ошибках
- GitHub Discussions для вопросов и идей

---

**Напоследок / In summary:**

Napominator — это мощный инструмент для родителей и администраторов, которые хотят эффективно контролировать и управлять использованием компьютера. Приложение сочетает напоминания о задачах, контроль приложений и мониторинг активности в одном решении.

**Дата последнего обновления / Last Updated:** 15.11.2025
