using System;

namespace ConsoleApp1.Assets.Models
{
    /// <summary>
    /// �`���[�g�p���i���i�e�N�j�J���w�W�܂ށj
    /// </summary>
    public class ChartPrice : ICloneable
    {
        public virtual DateTime Date { get; internal set; }
        public virtual double Price { get; internal set; }
        public virtual double Volatility { get; internal set; }
        public virtual double RSIL { get; internal set; }
        public virtual double RSIS { get; internal set; }
        /// <summary>
        /// �P���ړ����ϒl�i25���j
        /// </summary>
        public virtual double SMA25 { get; internal set; }
        /// <summary>
        /// �P���ړ����ϒl�i75���j
        /// </summary>
        public virtual double SMA75 { get; internal set; }
        /// <summary>
        /// Moving Average Deviation�i���ψړ������j25��
        /// </summary>
        public virtual double MADS { get; internal set; }
        /// <summary>
        /// Moving Average Deviation�i���ψړ������j75��
        /// </summary>
        public virtual double MADL { get; internal set; }
        /// <summary>
        /// SMA25�ASMA75�̘����l
        /// </summary>
        public virtual double SMAdev { get; internal set; }

        public object Clone()
        {
            // �󂢃R�s�[�i�����o�[�R�s�[�j��Ԃ�
            return this.MemberwiseClone();
        }
        public virtual bool OverboughtIndicator()
        {
            bool result = false;

            // RSI��臒l�ȉ��̏ꍇ
            if (this.RSIL >= CommonUtils.Instance.ThresholdOfOverboughtRSI) result = true;
            if (this.RSIS >= CommonUtils.Instance.ThresholdOfOverboughtRSI) result = true;

            return result;
        }

        public virtual bool OversoldIndicator()
        {
            bool result = false;

            // RSI��臒l�ȉ��̏ꍇ
            if (this.RSIL <= CommonUtils.Instance.ThresholdOfOversoldRSI) result = true;
            if (this.RSIS <= CommonUtils.Instance.ThresholdOfOversoldRSI) result = true;

            return result;
        }

    }
}