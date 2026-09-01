namespace Cert4Update.Core
{
    /// <summary>
    /// Параметры для сбора сертификатов.
    /// </summary>
    /// <param name="SourceDir">Директория, в которой рекурсивно искать файлы .cer.</param>
    /// <param name="progress">Объект для отчёта о прогрессе.</param>
    /// <param name="MaxDaysToCertEnd">Максимальное количество дней до окончания срока сертификата (включительно).</param>
    public record CollectParams(string SourceDir, IProgress<ProgressData> progress, int MaxDaysToCertEnd);
}