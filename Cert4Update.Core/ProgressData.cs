namespace Cert4Update.Core
{
    /// <summary>
    /// Данные для отображения прогресса (текст и цвета консоли).
    /// </summary>
    /// <param name="Text">Текст сообщения.</param>
    /// <param name="Color">Цвет текста (по умолчанию серый).</param>
    /// <param name="BackColor">Цвет фона (по умолчанию чёрный).</param>
    public record ProgressData(string Text, ConsoleColor Color = ConsoleColor.Gray, ConsoleColor BackColor = ConsoleColor.Black);
}