using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;

namespace Cert4Update.Core
{
    /// <summary>
    /// Сервис для сбора и фильтрации сертификатов электронной подписи из указанной директории.
    /// </summary>
    public class CollectorService
    {
        /// <summary>
        /// Асинхронно собирает сертификаты, срок действия которых истекает в ближайшие дни,
        /// группирует их по владельцу и оставляет только самый "свежий" сертификат для каждого владельца.
        /// </summary>
        /// <param name="collectParams">Параметры сбора (путь, прогресс, порог дней до окончания).</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Список отфильтрованных сертификатов.</returns>
        public async Task<List<CertData>> CollectCertificatesForUpdate(CollectParams collectParams,
            CancellationToken cancellationToken)
        {
            // Проверяем, существует ли исходная директория
            if (!Directory.Exists(collectParams.SourceDir))
                throw new DirectoryNotFoundException(collectParams.SourceDir);

            var prg = collectParams.progress;

            // Сообщаем о начале поиска файлов
            prg.Report(new($@"Ищем файлы сертификатов ЭП в каталоге {collectParams.SourceDir}"));
            // Рекурсивно ищем все файлы с расширением .cer
            var fileNames = Directory.GetFiles(collectParams.SourceDir, @"*.cer",
                new EnumerationOptions() { RecurseSubdirectories = true });
            prg.Report(new($@"Найдено {fileNames.Length} файлов сертификатов ЭП", ConsoleColor.Blue, ConsoleColor.White));

            var allCerts = new List<CertData?>();
            prg.Report(new($@"Выбираем сертификаты ЭП с датой окончания более текущей", ConsoleColor.Yellow, ConsoleColor.Black));

            // Список задач для параллельной обработки каждого файла
            var taskList = new List<Task>();
            foreach (var file in fileNames)
            {
                // Запускаем обработку каждого файла в отдельной задаче (параллельно)
                taskList.Add(new TaskFactory().StartNew(() =>
                {
                    // Проверяем отмену операции
                    cancellationToken.ThrowIfCancellationRequested();

                    // Извлекаем данные сертификата из файла
                    var certData = GetCertDataFromFile(collectParams, file);

                    // Если сертификат валиден и ещё не истёк
                    if (certData != null && certData.EndDate > DateTime.Now)
                    {
                        // Выводим информацию в прогресс с цветом в зависимости от наличия ОГРН
                        prg.Report(new(certData.ToString(), certData.Ogrn == null ? ConsoleColor.Green : ConsoleColor.Yellow));
                        allCerts.Add(certData);
                    }
                }));
            }
            // Ожидаем завершения всех задач
            await Task.WhenAll(taskList);

            // Формируем итоговый результат с группировкой и фильтрацией
            return BuidResult(allCerts, collectParams);
        }

        /// <summary>
        /// Формирует итоговый список сертификатов:
        /// - Группирует по уникальному владельцу (ФИО + ОГРН)
        /// - Для каждой группы оставляет сертификат с самой поздней датой окончания
        /// - Отбирает только те, срок которых истекает в пределах MaxDaysToCertEnd (в будущем)
        /// - Сортирует по дате окончания (по возрастанию)
        /// </summary>
        private List<CertData> BuidResult(IEnumerable<CertData?> allCerts, CollectParams collectParams)
        {
            // Отфильтровываем null-значения и группируем по владельцу
            var vDict = allCerts.Where(x => x != null).OfType<CertData>().GroupBy(x => x.FioOgrn).ToList();
            var result = new List<CertData>();
            foreach (var vGroup in vDict)
            {
                // Берём сертификат с самой поздней датой окончания в группе
                var vCert = vGroup.OfType<CertData>().OrderByDescending(x => x.EndDate).First();
                // Если срок окончания попадает в интервал [сегодня, сегодня + MaxDaysToCertEnd]
                if (vCert.EndDate <= DateTime.Now.AddDays(collectParams.MaxDaysToCertEnd) && vCert.EndDate >= DateTime.Now)
                    result.Add(vCert);
            }
            // Сортируем по дате окончания (от ранней к поздней)
            result.Sort((x, y) => x.EndDate.CompareTo(y.EndDate));
            return result;
        }

        /// <summary>
        /// Извлекает данные из файла сертификата (.cer) с помощью регулярных выражений.
        /// </summary>
        /// <param name="collectParams">Параметры (не используются непосредственно, но передаются для единообразия).</param>
        /// <param name="certFileName">Путь к файлу сертификата.</param>
        /// <returns>Объект CertData или null, если не удалось распознать ФИО.</returns>
        private CertData? GetCertDataFromFile(CollectParams collectParams, string certFileName)
        {
            CertData? result = null;
            // Загружаем сертификат из файла
            using var cert = X509CertificateLoader.LoadCertificateFromFile(certFileName);
            var certText = cert.Subject; // Тема сертификата (строка с атрибутами)

            // Регулярные выражения для извлечения:
            // SN=Фамилия
            var famMatch = Regex.Match(certText, @"SN=(\w+)");
            // G=Имя Отчество (может содержать пробел)
            var imOtMatch = Regex.Match(certText, @"G=(\w+( \w+))");
            // ОГРН=цифры (если есть)
            var ogrnMatch = Regex.Match(certText, @"ОГРН=(\d+)");

            // Если удалось извлечь фамилию и имя/отчество
            if (famMatch.Success && imOtMatch.Success)
            {
                result = new CertData()
                {
                    Fio = famMatch.Groups[1].Value + " " + imOtMatch.Groups[1].Value,
                    BegDate = cert.NotBefore,
                    EndDate = cert.NotAfter,
                    Ogrn = ogrnMatch.Success ? ogrnMatch.Groups[1].Value : null
                };
            }

            return result;
        }
    }
}