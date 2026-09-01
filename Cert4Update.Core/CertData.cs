namespace Cert4Update.Core
{
    /// <summary>
    /// Представляет данные сертификата электронной подписи, извлечённые из файла.
    /// Используется для хранения информации о владельце, сроке действия и ОГРН (для юрлиц).
    /// </summary>
    public class CertData
    {
        /// <summary>
        /// Полное имя владельца сертификата (Фамилия + Имя/Отчество).
        /// Обязательное поле.
        /// </summary>
        public required string Fio { get; set; }

        /// <summary>
        /// Дата начала действия сертификата.
        /// Обязательное поле.
        /// </summary>
        public required DateTime BegDate { get; set; }

        /// <summary>
        /// Дата окончания действия сертификата.
        /// Обязательное поле.
        /// </summary>
        public required DateTime EndDate { get; set; }

        /// <summary>
        /// ОГРН организации (если сертификат принадлежит юридическому лицу).
        /// Может быть null для физических лиц.
        /// </summary>
        public string? Ogrn { get; set; }

        /// <summary>
        /// Строковое представление владельца с добавлением ОГРН, если он указан.
        /// Используется для группировки сертификатов по уникальному владельцу.
        /// </summary>
        public string FioOgrn => Fio + (Ogrn == null ? "" : " юрлицо ОГРН " + Ogrn);

        /// <summary>
        /// Возвращает текстовое описание сертификата для вывода в лог/прогресс.
        /// </summary>
        public override string ToString()
        {
            return $@"{Fio} {BegDate} {EndDate}{(Ogrn == null ? "" : " юрлицо " + Ogrn)}";
        }
    }
}