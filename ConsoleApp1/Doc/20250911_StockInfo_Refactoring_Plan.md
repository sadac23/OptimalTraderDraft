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
