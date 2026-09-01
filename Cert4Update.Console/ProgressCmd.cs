using Cert4Update.Core;
using System;

namespace Cert4Update.Console
{
    /// <summary>
    /// Реализация интерфейса IProgress&lt;ProgressData&gt; для вывода отчётов о прогрессе
    /// в консольное приложение с цветовым форматированием.
    /// </summary>
    public class ProgressCmd : IProgress<ProgressData>
    {
        public ProgressCmd() { }

        /// <summary>
        /// Обрабатывает сообщение о прогрессе, выводя его в консоль
        /// с указанными цветами текста и фона.
        /// </summary>
        /// <param name="value">Данные прогресса (текст и цвета).</param>
        public void Report(ProgressData value)
        {
            // Устанавливаем цвета консоли
            System.Console.ForegroundColor = value.Color;
            System.Console.BackgroundColor = value.BackColor;
            // Выводим сообщение с временной меткой
            System.Console.WriteLine(DateTime.Now.ToString() + " " + value.Text);
            // Сбрасываем цвета (необязательно, но рекомендуется)
            System.Console.ResetColor();
        }
    }
}
