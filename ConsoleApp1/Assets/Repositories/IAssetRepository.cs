using ConsoleApp1.Assets.Models;

namespace ConsoleApp1.Assets.Repositories
{
    /// <summary>
    /// ���Y����DB�A�N�Z�X�p���|�W�g���C���^�[�t�F�[�X
    /// </summary>
    public interface IAssetRepository
    {
        Task<List<ScrapedPrice>> LoadHistoryAsync(string code);
        Task SaveHistoryAsync(string code, List<ScrapedPrice> prices);
        Task DeleteHistoryAsync(string code, DateTime targetDate);
        List<FullYearPerformanceForcast> GetPreviousForcasts(string code, string fiscalPeriod);

        // �ǉ�: �����e�[�u������ŐV���t���擾
        DateTime GetLastHistoryUpdateDay(string code);

        /// <summary>
        /// �`���[�g�p�����f�[�^���擾
        /// </summary>
        /// <param name="code">�����R�[�h</param>
        /// <param name="limit">�擾����</param>
        /// <returns>�����f�[�^�̃��X�g�i�e�s�̓J���������l��Dictionary�j</returns>
        List<Dictionary<string, object>> GetChartPriceRows(string code, int limit);

        Task RegisterHistoryAsync(string code, List<ScrapedPrice> prices);
        Task DeleteOldHistoryAsync(string code, DateTime beforeDate);
        Task RegisterForcastHistoryAsync(string code, List<FullYearPerformanceForcast> forcasts);
    }
}