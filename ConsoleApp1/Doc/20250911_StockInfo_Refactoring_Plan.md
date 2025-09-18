# StockInfo�n�N���X ���t�@�N�^�����O���j��

## �ړI

- ��剻����`StockInfo`�N���X�̐Ӗ��𕪊����A���ʉ���}��
- �h���N���X�i`IndexInfo`, `JapaneseETFInfo`���j�̎������j�𓝈ꂷ��
- ����̊g������e�X�g�e�Ր������߂�

---

## ���j�T�v

- �O�����擾��o�͏����̐Ӗ����AStrategy�p�^�[���ɂ��Ϗ��Ŏ�������
- ���ʃ��W�b�N��`StockInfo`�ɏW�񂵁A�ʃ��W�b�N�͈Ϗ���iStrategy�j�Ŏ�������
- �h���N���X�́u�Ϗ���̎����������ւ��邾���v�Ŋg���ł���\���Ƃ���

---

## �\���C���[�W

1. �O�����擾�E�o�͏����̂��߂̃C���^�[�t�F�[�X��p�ӂ��A�Ӗ��𖾊m�ɕ�������
2. `StockInfo`�͋��ʃv���p�e�B�E���W�b�N�������AUpdater/Formatter�iStrategy�j���Ϗ��ŕێ�����
3. �h���N���X�i��F`IndexInfo`, `JapaneseETFInfo`�j�́A�p�r�ɉ�����Updater/Formatter��n���ď���������
4. �V����������ʂ������Ă��AUpdater/Formatter��p�ӂ��邾���Ŋg���ł���

---

## �����b�g

- **�g����**�F�V����������ʂ�Strategy������ǉ����邾���őΉ��\
- **�e�X�g�e�Ր�**�FUpdater/Formatter�P�ʂ�Mock���E�P�̃e�X�g���e��
- **�ێ琫**�F���ʃ��W�b�N��StockInfo�A�ʃ��W�b�N��Strategy�ŕ�������邽�߁A�ێ炪���₷��

---

## �܂Ƃ�

- `StockInfo`�̔�剻��h���A�h���N���X�̎����𓝈�
- ����̊g����e�X�g���e�ՂȐ݌v�ɍ��V���邱�ƂŁA�ێ琫�E�i�����������

---

## AssetInfo�N���X ���t�@�N�^�����O��

### ��剻�̎�ȗv��

- �v���p�e�B�������ɑ����A�f�[�^�\���ƃ��W�b�N�����݂��Ă���
- �f�[�^�擾�E�v�Z�E����EDB�A�N�Z�X�E�o�͂ȂǈقȂ�Ӗ��̃��\�b�h��1�N���X�ɏW�񂳂�Ă���
- �����N���X�iScrapedPrice, FullYearPerformance���j�������A1�t�@�C���̍s�����c��
- Strategy�p�^�[���̓����͈ꕔ�iupdater/formatter�j�̂�

### ���t�@�N�^�����O���j

1. **�Ӗ����ƂɃN���X����**
   - AssetInfo�́u�f�[�^�ێ��v�Ɓu���W�b�N�Ϗ��v�̂ݒS��
   - �v�Z�E����EDB�A�N�Z�X���͐�p�N���X/�T�[�r�X�ɈϏ�

2. **Strategy�p�^�[���̓K�p�͈͊g��**
   - �v�Z�E���胍�W�b�N��Strategy�����A�����ւ��\��

3. **�����N���X�̊O�����E���ʉ�**
   - ScrapedPrice, FullYearPerformance���͓Ɨ��t�@�C�������A�ė��p�������߂�

4. **DB�A�N�Z�X�̃��|�W�g���p�^�[����**
   - DB�����Repository�N���X�ɏW�񂵁AAssetInfo����DB�A�N�Z�X��r��

5. **�h���N���X�̐Ӗ����m��**
   - �h���N���X��Strategy�̑g�ݍ��킹�w��݂̂Ƃ��A�ʃ��W�b�N��Strategy�������ɏW��

### �T���v���\���C���[�W

---

## AssetInfo���t�@�N�^�����O �i�K�I���{��

### �i�K�I���t�@�N�^�����O�X�e�b�v

1. **�f�[�^�N���X�̊O�����E����**
   - ScrapedPrice, FullYearPerformance�Ȃǂ̓����N���X��Ɨ��t�@�C����
   - AssetInfo�̃v���p�e�B��`�𐮗����A�f�[�^�\���̖��m��

   #### �O�����Ώۃf�[�^�N���X�ꗗ

   AssetInfo�N���X����O�������ׂ��f�[�^�N���X�͈ȉ��̒ʂ�ł��B������ `ConsoleApp1.Assets.Models` ���O��Ԕz���ɓƗ��t�@�C���Ƃ��Đ������܂��B

   | �N���X��                        | ��Ȗ����E���e                           |
   |----------------------------------|------------------------------------------|
   | ScrapedPrice                    | �������i���i���������j                 |
   | FullYearPerformance             | �ʊ��Ɛя��                             |
   | FullYearProfit                  | �ʊ����v���                             |
   | QuarterlyPerformance            | �l�����Ɛя��                           |
   | FullYearPerformanceForcast      | �ʊ��Ɛї\�z�i����ї\�z�����j           |
   | ChartPrice                      | �`���[�g�p���i���i�e�N�j�J���w�W�܂ށj |
   | Disclosure                      | �J�����                                 |

   **�z�u�E���O��ԗ�**

   - ConsoleApp1\Assets\Models\ScrapedPrice.cs
   - ConsoleApp1\Assets\Models\FullYearPerformance.cs
   - ConsoleApp1\Assets\Models\FullYearProfit.cs
   - ConsoleApp1\Assets\Models\QuarterlyPerformance.cs
   - ConsoleApp1\Assets\Models\FullYearPerformanceForcast.cs
   - ConsoleApp1\Assets\Models\ChartPrice.cs
   - ConsoleApp1\Assets\Models\Disclosure.cs

   ���ׂ� `namespace ConsoleApp1.Assets.Models` �œ��ꂵ�܂��B

2. **DB�A�N�Z�X�̕���**
   - DB�A�N�Z�X�����i�����擾�E�ۑ��Ȃǁj��IAssetRepository/AssetRepository�ֈړ�
   - AssetInfo����DB�A�N�Z�X�R�[�h���폜���ARepository�o�R�ɐ؂�ւ�

   #### AssetInfo�N���X����IAssetRepository�ɏ������ڏ����ׂ����\�b�h�ꗗ

   - `LoadHistoryAsync()`  
     �����f�[�^�̎擾�B����repository�o�R�ŌĂяo���ς݁B
   - `SaveHistoryAsync()`  
     �����f�[�^�̕ۑ��B����repository�o�R�ŌĂяo���ς݁B
   - `DeleteHistoryAsync(DateTime targetDate)`  
     �����f�[�^�̍폜�B����repository�o�R�ŌĂяo���ς݁B
   - `GetPreviousForcasts()`  
     �ʊ��Ɛї\�z�����̎擾�BDB�A�N�Z�X������repository�Ɉڏ����ׂ��B
   - `GetLastHistoryUpdateDay()`  
     �����e�[�u������ŐV���t���擾�BDB�A�N�Z�X������repository�Ɉڏ����ׂ��B
   - `SetupChartPrices()`  
     history�e�[�u������`���[�g�p�f�[�^���擾�BDB�A�N�Z�X������repository�Ɉڏ����ׂ��B

   ����L���\�b�h�́A����`SQLiteConnection`��`SQLiteCommand`����DB�A�N�Z�X���Ă���A�܂��̓��|�W�g���o�R�ɂ��ׂ��f�[�^�擾�E�ۑ��������܂݂܂��B

   - �����̃��\�b�h��DB�A�N�Z�X������IAssetRepository�C���^�[�t�F�[�X�Œ�`���AAssetRepository���̎����N���X�Ɉړ����Ă��������B
   - AssetInfo��IAssetRepository�o�R�ł̂�DB������s���悤�Ƀ��t�@�N�^�����O���܂��B

   #### ��̓I�Ȏ��{��

   - AssetInfo�N���X���ɑ��݂���DB�A�N�Z�X�֘A���\�b�h�i��F�����̎擾�E�ۑ��A�L���b�V���o�^�A�����폜�Ȃǁj�����ׂĒ��o���AIAssetRepository�C���^�[�t�F�[�X�Œ�`����B
   - DB�A�N�Z�X�̎�����AssetHistoryRepository�N���X�ɏW�񂵁ASQLite�⑼��DB�A�N�Z�X���W�b�N�����̃N���X�Ɉړ�����B
   - AssetInfo��IAssetRepository�^�̃t�B�[���h/�v���p�e�B�������A�K�v��DB�����Repository�o�R�ŌĂяo���B
   - ��F
     - `Task<List<ScrapedPrice>> LoadHistoryAsync(string code);`
     - `Task SaveHistoryAsync(string code, List<ScrapedPrice> prices);`
     - `Task DeleteHistoryAsync(string code, DateTime targetDate);`
   - �e�X�g�e�Ր�����̂��߁AIAssetRepository��Mock�������e�Ղɂł���悤�ɐ݌v����B
   - ������DB�A�N�Z�X�R�[�h�iSQLiteCommand���j��AssetInfo���犮�S�ɔr�����ARepository�N���X�ɂ̂ݑ��݂�����B
   - DI�i�ˑ��������j�����p���AAssetInfo�̗��p���Ń��|�W�g�������������ւ��\�ɂ���B

   **�����b�g**
   - DB�A�N�Z�X�̐Ӗ������m�ɂȂ�AAssetInfo�̔�剻��h�~
   - �e�X�g����DB�A�N�Z�X��Mock�ɍ����ւ��₷���Ȃ�
   - DB�A�N�Z�X�̋��ʉ��E�ė��p������
   - DB�A�N�Z�X�w�̕ύX�i��FSQLite��SQL Server���j��Repository�����̍����ւ��݂̂őΉ��\

   **�T���v���\���C���[�W**

   - ConsoleApp1\Assets\Repositories\IAssetRepository.cs
   - ConsoleApp1\Assets\Repositories\AssetRepository.cs

3. **�v�Z�E���胍�W�b�N��Strategy��**
   - PER�v�Z�◘��蔻��Ȃǂ�IAssetCalculator/StockCalculator���ɕ���
   - AssetInfo��Strategy�C���^�[�t�F�[�X�o�R�Ń��W�b�N���Ăяo���`�ɕύX

   #### ��̓I�Ȏ��{��

   - �v�Z�E���胍�W�b�N���ƂɃC���^�[�t�F�[�X���`���AStrategy�p�^�[���ō����ւ��\�ɂ���B
     - ��F`IAssetCalculator`�i���Y�S�ʂ̌v�Z�p�j�A`IStockCalculator`�i�����ŗL�̌v�Z�p�j�Ȃ�
   - ��\�I�Ȍv�Z�E���胍�W�b�N��
     - PER�i�������v���j�v�Z
     - PBR�i���������Y�{���j�v�Z
     - �z�������v�Z
     - ���z����
     - �Ɛѐ�������
     - �����g�����h����
   - �e���W�b�N�͌ʂ�Strategy�N���X�Ƃ��Ď������A�K�v�ɉ����đg�ݍ��킹�ė��p�ł���悤�ɂ���B
     - ��F`DefaultStockCalculator`�A`GrowthStockCalculator`�A`EtfCalculator`�Ȃ�
   - AssetInfo��h���N���X�́A�R���X�g���N�^���ŗ��p����Calculator�iStrategy�j���󂯎��A�v�Z�E���菈����Strategy�o�R�Ŏ��s����B
   - �e�X�g�e�Ր�����̂��߁A�eCalculator��Mock�����₷���݌v�Ƃ���B

   > **�⑫�FStrategy���̑Ώ۔͈͂ɂ���**
   >
   > Strategy�p�^�[���ŕ����E�����ւ��\�ɂ���̂́u�Z�o�l�i�v�Z�┻��ɂ���ē��o�����l�j�v�݂̂Ƃ���B
   > �O������擾�������i��F�����A�Ɛуf�[�^�A�z�����Ȃǂ̐��f�[�^�j��AssetInfo��֘A�f�[�^�N���X�����̂܂ܕێ����A
   > �����̒l�����ƂɎZ�o�E���肷�郍�W�b�N�����݂̂�Strategy�Ƃ��Ď�������B
   >
   > - **Strategy���̑Ώ�**�FPER�APBR�A�z�������A��������Ȃǁu�v�Z�E����ɂ���ē��o�����l�┻�茋�ʁv
   > - **Strategy���̑ΏۊO**�FAPI��DB�����璼�ڎ擾�����u���f�[�^�v�i�����A�ƐсA�z���Ȃǁj
   >
   > **Strategy���̑ΏۂƂȂ鏈���ꗗ�i�R�[�h��͌��ʁj**
   > 
   > - `IsPERUndervalued`�iPER��������j
   > - `IsPBRUndervalued`�iPBR��������j
   > - `IsROEAboveThreshold`�iROE�����j
   > - `IsAnnualProgressOnTrack`�i�ʊ��i����������j
   > - `IsHighYield`�i����荂����j
   > - `IsHighMarketCap`�i�������z������j
   > - `IsCloseToDividendRecordDate`�i�z�������m����ڋߔ���j
   > - `IsCloseToShareholderBenefitRecordDate`�i�D�Ҍ����m����ڋߔ���j
   > - `IsCloseToQuarterEnd`�i�l�������Z���ڋߔ���j
   > - `IsAfterQuarterEnd`�i�l�������Z���㔻��j
   > - `IsQuarterEnd`�i�l�������Z��������j
   > - `IsJustSold`�i���p���㔻��j
   > - `IsOwnedNow`�i���ݕۗL����j
   > - `IsGoldenCrossPossible`�i�S�[���f���N���X�����\������j
   > - `HasRecentStockSplitOccurred`�i����������������j
   > - `ShouldAverageDown`�i�i���s������j
   > - `IsGranvilleCase1Matched`�i�O�����r����1�@������j
   > - `IsGranvilleCase2Matched`�i�O�����r����2�@������j
   > - `HasDisclosure`�i�J�����L������j
   > - `IsRecordDate`�i�����m�����������j
   > - `IsAfterRecordDate`�i�����m������㔻��j
   > - `IsCloseToRecordDate`�i�����m������O����j
   > - `ExtractAndValidateDateWithinOneMonth`�i���t���o�E1�����ȓ�����j
   > - `UpdateProgress`�i�i�����E�c�Ɨ��v�����̎Z�o�j
   > - `UpdateDividendPayoutRatio`�i�z�������EDOE�Z�o�j
   > - `UpdateFullYearPerformanceForcastSummary`�i�ƐсE�z�������T�}���Z�o�j
   > - `SetupChartPrices`�i�`���[�g�p�w�W�Z�o�FSMA, RSI, ���������j
   > - ���̑��A`GetDividendPayoutRatio`�A`GetDOE`�A`GetIncreasedRate`�A`GetDividendPerShareIncreased` �Ȃǂ̎Z�o�nprivate���\�b�h

4. **�A���[�g�����Strategy��**
   - �A���[�g���胍�W�b�N��IAlertEvaluator/DefaultAlertEvaluator�ɕ���
   - AssetInfo�̓A���[�g�����Strategy�o�R�Ŏ��s

5. **�h���N���X�̐����E����**
   - JapaneseStockInfo, JapaneseETFInfo, IndexInfo���̔h���N���X��Strategy�̑g�ݍ��킹�w��݂̂ō\�z
   - �ʃ��W�b�N��Strategy�������ɏW��

6. **�e�X�g�E���t�@�N�^�����O�̊���**
   - �eStrategy/Repository�̒P�̃e�X�g���쐬
   - AssetInfo�̐Ӗ����u�f�[�^�ێ��{�Ϗ��v�݂̂ɂȂ��Ă��邱�Ƃ��m�F

---

### �i�ߕ��̃|�C���g

- �e�X�e�b�v���ƂɊ����̓���m�F�E�e�X�g��K�����{
- 1�X�e�b�v���ƂɃR�~�b�g���A��肪����΂����Ƀ��[���o�b�N�ł���悤�ɂ���
- �������p�ӏ��ւ̉e�����ŏ����ɗ}���A�i�K�I�Ɉڍs

---

### �܂Ƃ�

- �f�[�^�E���W�b�N�EDB�A�N�Z�X�E�o�́E����𖾊m�ɕ���
- Strategy/Repository�p�^�[����O�ꂵ�A�g�����E�e�X�g�e�Ր��E�ێ琫������
- �����N���X�⋤�ʃ��W�b�N�͓Ɨ��t�@�C�������ė��p�������߂�

---

## [2025/09/18] ��AssetInfo���VAssetInfo���t�@�N�^�����O�ڍs�󋵁E��������

### 1. ����E�v�Z���W�b�N�̈Ϗ��ƃJ�o�[��

- ��AssetInfo�̔���E�v�Z�n���\�b�h�i��FIsPERUndervalued, IsHighYield, UpdateFullYearPerformanceForcastSummary���j�́A�VAssetInfo��IAssetJudgementStrategy/IAssetCalculator����Strategy�p�^�[���ɈϏ�����Ă���B
- ����ɂ��A���W�b�N���̂͊O���N���X�Ɉړ����Ă��邽�߁AAssetInfo�{�̂ɂ͒��ړI�Ȏ������Ȃ��B
- �Q�Ɛ�N���X�iIAssetCalculator���j�̃C���^�[�t�F�[�X�ɂ͕K�v�ȃ��\�b�h����`����Ă��邪�A**�����N���X�ŋ�AssetInfo�̃��W�b�N�����m�ɍČ�����Ă��邩�͎����t�@�C���̊m�F���K�v**�B
  - ���Ɂu�������v���z�L���◦�v�Z�v�uSummary�̑g�ݗ��āv�ȂǁA���N���X�ׂ̍������W�b�N�������Ă��Ȃ����v���ӁB

### 2. DB����E���O�o��

- DB����i����o�^�E�폜�E�d���`�F�b�N�j�́A���N���X�ł͒���SQLite�𑀍삵�ACommonUtils.Instance.Logger.LogInformation�Ń��O�o�͂��Ă����B
- �V�N���X�ł�IAssetRepository�o�R�̔񓯊����\�b�h�Ɉڍs���ARegisterCacheAsync����RegisterHistoryAsync��RegisterForcastHistoryAsync�ADeleteOldHistoryAsync���Ăяo���Ă���B
- **���O�o�͂ɂ��ẮARepository��������CommonUtils.Instance.Logger���g���ďo�͂���݌v�ɂ���K�v������**�B
  - ���N���X���l�ADB�o�^�E�폜����LogInformation���Ă΂�Ă��邩�ARepository������K���m�F���邱�ƁB

### 3. FullYearPerformanceForcastSummary�v���p�e�B

- ���N���X�ɂ�FullYearPerformanceForcastSummary�v���p�e�B�����������A�V�N���X�ɂ͑��݂��Ȃ��B
- ���N���X��UpdateFullYearPerformanceForcastSummary�́A�eFullYearPerformanceForcast��Summary��g�ݗ��ĂĂ����B
- **�V�N���X�ł́ASummary�͊eForcast�̃v���p�e�B�Ƃ��ĕێ�����Ă��邽�߁A�N���X�S�̂�Summary�v���p�e�B�͕s�v**�B
  - �����u�S�̃T�}��������v���K�v�ȏꍇ�́AFullYearPerformancesForcasts.Select(f => f.Summary).Join(",")�̂悤�ɓs�x�g�ݗ��Ă�Ώ\���B

### 4. �s�v�ȃv���p�e�B

- FullYearPerformanceForcastSummary�͎g���Ă��Ȃ����ߕs�v�i�V�ݕs�v�j�B
- ���̑��̃v���p�e�B�͐V���łقڈ�v���Ă���A�s�v�Ȃ��̂͂Ȃ��B

### 5. �d���`�F�b�N�E�ʓo�^���W�b�N

- ���N���X��IsInHistory��IsInForcastHistory�̂悤�ȏd���`�F�b�N�́A�V�N���X�ł�Repository���ŒS�ۂ���Ă���B
- RegisterCacheAsync�̃R�����g�ɂ��u�d���`�F�b�N��Repository���Ŏ����v�Ɩ��L����Ă���B

### 6. �Q�Ɛ�N���X�̃J�o�[��

- IAssetCalculator, IAssetJudgementStrategy, IAssetRepository�̃C���^�[�t�F�[�X�͐V�N���X�Ő��������p����Ă���B
- �������A**�����N���X�i��FAssetCalculator, AssetRepository�Ȃǁj�ŋ��N���X�̃��W�b�N�����m�ɍČ�����Ă��邩�́A�����t�@�C���̊m�F���K�v**�B
  - ���ɁuDB���쎞�̃��O�o�́v�uSummary�g�ݗ��ă��W�b�N�v�u�d���`�F�b�N�v�ȂǁB

### 7. ���O�o�͗v��

- **DB���쎞�̃��O�o�͂͐V�݌v�ł��K�{**�B
- ���N���X���l�ACommonUtils.Instance.Logger.LogInformation��Repository�����̊eDB���상�\�b�h���ŕK���Ăяo�����ƁB

---

#### ���_�E�A�N�V����

- �@�\�I�Ȕ����͊�{�I�ɂȂ����ARepository/Calculator���̎�����
  - ���N���X�̃��W�b�N�i����Summary�g�ݗ��āADB���쎞�̃��O�o�́A�d���`�F�b�N�j�����m�ɍČ�����Ă��邩��K���m�F���邱�ƁB
- FullYearPerformanceForcastSummary�v���p�e�B�͕s�v�B
- DB���쎞�̃��O�o�͂�Repository�����ŕK���s�����ƁB

---

#### �⑫

- IAssetCalculator��IAssetRepository�̎����t�@�C�����K�v�ł���΁A�p�X���w�肵�ē��e���擾���A�ǉ������\�B

---

## [2025/09/18] ���N���X���W�b�N�Č��� ���،���

### 1. DB�A�N�Z�X�E�d���`�F�b�N�E���𑀍�iAssetRepository�j

- �����E�\�z�����̏d���`�F�b�N�i`IsInHistoryAsync`, `IsInForcastHistoryAsync`�j���o�āA�d�����Ȃ����INSERT���郍�W�b�N�͐V�����ł��Č�����Ă���B
- �����̎擾�E�ۑ��E�폜�A�\�z�����̎擾�A�`���[�g�p�f�[�^�̎擾�A�ŐV������t�̎擾�ȂǁA���N���X��DB���샍�W�b�N�͂��ׂ�AssetRepository�ŃJ�o�[����Ă���B
- ��O���̃f�t�H���g�l�ԋp�∬��Ԃ������N���X���l�B
- **���_�FDB�A�N�Z�X�E�d���`�F�b�N�E���𑀍샍�W�b�N�͐������Č�����Ă���B**

### 2. �v�Z�E�T�}���E�i�������iDefaultAssetCalculator�j

- �z�������EDOE�A�i�����E�c�Ɨ��v���E�O�N�����䐬�����̌v�Z�A�T�}���i�������v���z�L���◦�j�̑g�ݗ��āA�`���[�g�p�w�W�iSMA, RSI, ���������j�̌v�Z�ȂǁA���N���X�̌v�Z�E�T�}�����W�b�N��DefaultAssetCalculator�ōČ�����Ă���B
- �e��private���\�b�h�i�����t��������A�p�[�Z���g�ϊ����j���ڐA�ς݁B
- **���_�F�v�Z�E�T�}���E�i�����E�`���[�g�w�W���W�b�N�͐������Č�����Ă���B**

### 3. ��O�����E�f�t�H���g�l

- ��O���̃f�t�H���g�l�ԋp�i��F���t�擾���s����DateTime.Now��MasterStartDate�j�͋��N���X�Ɠ��������B

### 4. ���O�o��

- AssetRepository����**DB���쎞�̃��O�o�́iCommonUtils.Instance.Logger.LogInformation���j��������**�B
- ���N���X���l�A����o�^�E�폜�E�\�z����o�^���̑��샍�O�o�͂�Repository���Œǉ���������K�v����B

---

#### �������_

- **DB�A�N�Z�X�E�d���`�F�b�N�E�v�Z�E�T�}���E�i�������̃��W�b�N�́A���N���X�̓��e���V�����Ő������Č�����Ă���B**
- **�������ADB���쎞�̃��O�o�́iLogInformation�j��AssetRepository�ɒǉ��������K�v�B**
- ����ȊO�̃��W�b�N�Č����͍����A���N���X�̋@�\�v���𖞂����Ă���B

---

#### �A�N�V��������

- AssetRepository�̊eDB���상�\�b�h�i����o�^�E�폜�E�\�z����o�^���j�ŁA  
  `CommonUtils.Instance.Logger.LogInformation`�ɂ�鑀�샍�O�o�͂�ǉ����邱�ƁB

---
