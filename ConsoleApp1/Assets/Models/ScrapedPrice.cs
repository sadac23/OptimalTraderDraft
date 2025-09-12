using System;

namespace ConsoleApp1.Assets.Models
{
    /// <summary>
    /// �������i���i���������j
    /// </summary>
    public class ScrapedPrice
    {
        /// <summary>
        /// ���t
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// ���t������
        /// </summary>
        public string DateYYYYMMDD { get; set; }
        /// <summary>
        /// �n�l
        /// </summary>
        public double Open { get; set; }
        /// <summary>
        /// ���l
        /// </summary>
        public double High { get; set; }
        /// <summary>
        /// ���l
        /// </summary>
        public double Low { get; set; }
        /// <summary>
        /// �I�l
        /// </summary>
        public double Close { get; set; }
        /// <summary>
        /// �o����
        /// </summary>
        public double Volume { get; set; }
        /// <summary>
        /// ������I�l
        /// </summary>
        public double AdjustedClose { get; internal set; }
    }
}