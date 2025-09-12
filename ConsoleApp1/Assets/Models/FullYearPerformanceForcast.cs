using System;

namespace ConsoleApp1.Assets.Models
{
    /// <summary>
    /// �ʊ��Ɛї\�z�i����ї\�z�����j
    /// </summary>
    public class FullYearPerformanceForcast : ICloneable
    {
        /// <summary>
        /// ���Z��
        /// </summary>
        public string FiscalPeriod { get; internal set; }
        /// <summary>
        /// �C����
        /// </summary>
        public DateTime RevisionDate { get; internal set; }
        /// <summary>
        /// �敪
        /// </summary>
        public string Category { get; internal set; }
        /// <summary>
        /// �C������
        /// </summary>
        public string RevisionDirection { get; internal set; }
        /// <summary>
        /// ���㍂
        /// </summary>
        public string Revenue { get; internal set; }
        /// <summary>
        /// �c�Ɖv
        /// </summary>
        public string OperatingProfit { get; internal set; }
        /// <summary>
        /// �o��v
        /// </summary>
        public string OrdinaryProfit { get; internal set; }
        /// <summary>
        /// �ŏI�v
        /// </summary>
        public string NetProfit { get; internal set; }
        /// <summary>
        /// �C���z��
        /// </summary>
        public string RevisedDividend { get; internal set; }
        /// <summary>
        /// �\�z�T�v
        /// </summary>
        public string Summary { get; internal set; }
        /// <summary>
        /// �O��̗\�z
        /// </summary>
        public FullYearPerformanceForcast PreviousForcast { get; internal set; }

        public object Clone()
        {
            // �󂢃R�s�[�i�����o�[�R�s�[�j��Ԃ�
            return this.MemberwiseClone();
        }
        /// <summary>
        /// �����C�����܂ނ��H
        /// </summary>
        internal virtual bool HasDownwardRevision()
        {
            bool result = false;

            if (this.Category != CommonUtils.Instance.ForecastCategoryString.Initial
                && this.Category != CommonUtils.Instance.ForecastCategoryString.Final)
            {
                // ����
                if (CommonUtils.Instance.GetDouble(this.Revenue) < CommonUtils.Instance.GetDouble(this.PreviousForcast.Revenue)) result = true;
                // �o�험�v
                if (CommonUtils.Instance.GetDouble(this.OrdinaryProfit) < CommonUtils.Instance.GetDouble(this.PreviousForcast.OrdinaryProfit)) result = true;
                // �z��
                if (CommonUtils.Instance.GetDouble(this.RevisedDividend) < CommonUtils.Instance.GetDouble(this.PreviousForcast.RevisedDividend)) result = true;
            }

            return result;
        }

        /// <summary>
        /// ����C�����܂ނ��H
        /// </summary>
        internal virtual bool HasUpwardRevision()
        {
            bool result = false;

            if (this.PreviousForcast == null) return result;

            if (this.Category != CommonUtils.Instance.ForecastCategoryString.Initial)
            {
                // ����
                if (CommonUtils.Instance.GetDouble(this.Revenue) > CommonUtils.Instance.GetDouble(this.PreviousForcast.Revenue)) result = true;
                // �o�험�v
                if (CommonUtils.Instance.GetDouble(this.OrdinaryProfit) > CommonUtils.Instance.GetDouble(this.PreviousForcast.OrdinaryProfit)) result = true;
                // �z��
                if (CommonUtils.Instance.GetDouble(this.RevisedDividend) > CommonUtils.Instance.GetDouble(this.PreviousForcast.RevisedDividend)) result = true;
            }

            return result;
        }

    }
}