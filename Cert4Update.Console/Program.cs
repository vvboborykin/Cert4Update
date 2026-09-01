using Cert4Update.Core;
using Microsoft.Extensions.Configuration;

namespace Cert4Update.Console
{
    /// <summary>
    /// Главный класс консольного приложения.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Точка входа в приложение.
        /// Выполняет сбор сертификатов и формирует Excel-отчёт.
        /// </summary>
        static async Task Main(string[] args)
        {
            // Создаём экземпляр сервиса сбора сертификатов
            var vService = new CollectorService();

            // Создаём параметры сбора из конфигурации
            CollectParams collectParams = CreateParams(args);

            // Асинхронно собираем сертификаты (без отмены)
            var result = await vService.CollectCertificatesForUpdate(collectParams, CancellationToken.None);

            // Строим Excel-отчёт по шаблону и открываем его
            new ExcelReportBuilder().Build(Path.Combine("Templates", "report.xlsx"), result, collectParams.MaxDaysToCertEnd);
        }

        /// <summary>
        /// Создаёт параметры сбора сертификатов на основе конфигурации.
        /// Источники конфигурации (в порядке приоритета):
        /// 1. appsettings.json
        /// 2. Аргументы командной строки
        /// 3. Секреты пользователя (User Secrets)
        /// </summary>
        /// <param name="args">Аргументы командной строки.</param>
        /// <returns>Объект CollectParams с настроенными параметрами.</returns>
        private static CollectParams CreateParams(string[] args)
        {
            // Строим конфигурацию из нескольких источников
            var config = new ConfigurationBuilder()
                .AddJsonFile(@"appsettings.json")          // Базовый файл конфигурации
                .AddCommandLine(args)                     // Переопределение через командную строку
                .AddUserSecrets<Program>()                // Переопределение через секреты (для разработки)
                .Build();

            // Определяем директорию с сертификатами:
            // - из конфигурации CertDir
            // - или по умолчанию: родительская директория относительно запуска
            var vCertDir = config.GetValue<string?>(@"CertDir") ??
                Path.Combine("..", Path.DirectorySeparatorChar.ToString());

            // Создаём объект для вывода прогресса в консоль
            ProgressCmd progress = new();

            // Определяем максимальное количество дней до окончания срока (по умолчанию 60)
            int vMaxEndDays = config.GetValue<int?>(@"MaxDaysToCertEnd") ?? 60;

            // Возвращаем собранные параметры
            return new(vCertDir, progress, vMaxEndDays);
        }
    }
}