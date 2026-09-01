using Cert4Update.Core;
using ClosedXML.Report;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Cert4Update.Console
{
    /// <summary>
    /// Класс для построения отчёта в формате Excel на основе шаблона
    /// и списка сертификатов.
    /// </summary>
    public class ExcelReportBuilder
    {
        /// <summary>
        /// Строит Excel-отчёт по шаблону, заполняя его данными сертификатов,
        /// и автоматически открывает созданный файл.
        /// </summary>
        /// <param name="templateFileName">Путь к файлу шаблона Excel (.xlsx).</param>
        /// <param name="certDataList">Список сертификатов для отчёта.</param>
        /// <param name="days">Количество дней до окончания срока (используется в отчёте).</param>
        public void Build(string templateFileName, List<CertData> certDataList, int days)
        {
            // Формируем имя выходного файла в папке "Мои документы" с временной меткой
            string outputFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                $@"cert4update{DateTime.Now.ToString(@"yyyyMMddhhmmss")}.xlsx");

            // Загружаем шаблон отчёта
            var template = new XLTemplate(templateFileName);

            // Передаём переменные в шаблон
            template.AddVariable("CertList", certDataList); // Список сертификатов
            template.AddVariable("Days", days);             // Количество дней
            template.AddVariable("Now", DateTime.Now.ToShortDateString()); // Текущая дата

            // Генерируем отчёт и сохраняем
            template.Generate();
            template.SaveAs(outputFile);

            // Открываем созданный файл в программе по умолчанию для .xlsx
            Process.Start(new ProcessStartInfo(outputFile) { UseShellExecute = true });
        }
    }
}
