namespace ConsoleApp1.Assets.Setups
{
    public class DefaultAssetSetupStrategy : IAssetSetupStrategy
    {
        public void UpdateExecutions(AssetInfo asset, List<ExecutionList.ListDetail> executionList)
        {
            asset.Executions = ExecutionList.GetExecutions(executionList, asset.Code).OrderBy(e => e.Date).ToList();
        }

        public void UpdateAveragePerPbr(AssetInfo asset, List<MasterList.AveragePerPbrDetails> masterList)
        {
            try
            {
                // �s��敪(�}�X�^, �O���T�C�g)
                var sectionTable = new Dictionary<string, string>
                {
                    { "�X�^���_�[�h�s��", "���؂r" },
                    { "�v���C���s��", "���؂o" },
                    { "�O���[�X�s��", "���؂f" },
                };

                // ���(�}�X�^, �O���T�C�g)
                var industryTable = new Dictionary<string, string>
                {
                    { "1 ���Y�E�_�ы�", "���Y�E�_�ы�" },
                    { "2 �z��", "�z��" },
                    { "3 ���݋�", "���݋�" },
                    { "4 �H���i", "�H���i" },
                    { "5 �@�ې��i", "�@�ې��i" },
                    { "6 �p���v�E��", "�p���v�E��" },
                    { "7 ���w", "���w" },
                    { "8 ���i", "���i" },
                    { "9 �Ζ��E�ΒY���i", "�Ζ��E�ΒY���i" },
                    { "10 �S�����i", "�S�����i" },
                    { "11 �K���X�E�y�ΐ��i", "�K���X�E�y��" },
                    { "12 �S�|", "�S�|" },
                    { "13 ��S����", "��S����" },
                    { "14 �������i", "�������i" },
                    { "15 �@�B", "�@�B" },
                    { "16 �d�C�@��", "�d�C�@��" },
                    { "17 �A���p�@��", "�A���p�@��" },
                    { "18 �����@��", "�����@��" },
                    { "19 ���̑����i", "���̑����i" },
                    { "20 �d�C�E�K�X��", "�d�C�E�K�X" },
                    { "21 ���^��", "���^��" },
                    { "22 �C�^��", "�C�^��" },
                    { "23 ��^��", "��^��" },
                    { "24 �q�ɁE�^�A�֘A��", "�q�ɁE�^�A" },
                    { "25 ���E�ʐM��", "���E�ʐM" },
                    { "26 ������", "������" },
                    { "27 ������", "������" },
                    { "28 ��s��", "��s��" },
                    { "29 �،��A���i�敨�����", "�،�" },
                    { "30 �ی���", "�ی�" },
                    { "31 ���̑����Z��", "���̑����Z" },
                    { "32 �s���Y��", "�s���Y" },
                    { "33 �T�[�r�X��", "�T�[�r�X" },
                };

                foreach (var details in masterList)
                {
                    bool sectionMatching = false;
                    bool industryMatching = false;

                    if (sectionTable.ContainsKey(details.Section))
                    {
                        if (!string.IsNullOrEmpty(asset.Section) && asset.Section.Contains(sectionTable[details.Section]))
                        {
                            sectionMatching = true;
                        }
                    }

                    if (industryTable.ContainsKey(details.Industry))
                    {
                        if (!string.IsNullOrEmpty(asset.Industry) && asset.Industry.Contains(industryTable[details.Industry]))
                        {
                            industryMatching = true;
                        }
                    }

                    if (sectionMatching && industryMatching)
                    {
                        asset.AveragePer = CommonUtils.Instance.GetDouble(details.AveragePer);
                        asset.AveragePbr = CommonUtils.Instance.GetDouble(details.AveragePbr);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}