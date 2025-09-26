# �X�N���C�p�[�݌v�Ɗg�����Ɋւ��錟���h�L�������g

## 1. ����̉ۑ�

- �eScraper�iKabutanScraper, MinkabuScraper, YahooScraper�j�́A���Y��ʂ��񌹂��Ƃɏ�����URL/XPath���x�^��������Ă���B
- ��ʒǉ���V�T�C�g�Ή����A�eScraper�̓����ŕ����d�������������A�ێ琫�E�g�������ቺ����B
- ��F�č��ʊ��iUSAssetInfo�j�ǉ����A����Scraper�̑啝�ȏC�����K�v�B

---

### 1.1 ����̏d���E����̐􂢏o��

- �ȉ��̂悤�ȏd���E���򂪊eScraper�Ŕ������Ă���B

#### 1.1.1 URL�����̕���
- ���Y��ʂ��񌹂��Ƃ�URL�̑g�ݗ��Ă��قȂ�A�eScraper���ŕ���B
  - KabutanScraper: ���{��/�č�����URL���قȂ�B
  - YahooScraper: �w��/���{��/ETF/�č����ȂǂŃT�t�B�b�N�X��p�X���قȂ�B
  - MinkabuScraper: ���{���O�񂾂��A�����ʒǉ����ɕ��򂪕K�v�B

#### 1.1.2 XPath/HTML�m�[�h�I���̕���E�d��
- ���Y��ʂ��񌹂��Ƃ�XPath��HTML�m�[�h�̑I�����قȂ�A�����悤�ȃp�^�[���������ӏ��ɑ��݁B
  - KabutanScraper: ���{��/�č�����XPath���قȂ�B�e�[�u���s�̃��[�v�E�J�������o�����������B
  - YahooScraper: �w��/ETF/�ʊ���XPath���قȂ�B��������o�����A�M�p�c�Ȃǂ̃m�[�h���o���d���B
  - MinkabuScraper: �z���E�D�҂Ŏ����悤�ȃe�[�u���p�[�X�����������B

#### 1.1.3 �l�̃p�[�X�E�ϊ������̏d��
- �p�[�Z���g�E�{���E���{��P�ʁi���E���Ȃǁj�̐��l�ϊ��������eScraper�ŏd���B
  - KabutanScraper: ConvertToDoubleForYield, ConvertToDoubleForPerPbr, ConvertJapaneseNumberToDouble �ȂǁB
  - YahooScraper: ConvertToDouble, ConvertToDatetime �ȂǁB
  - MinkabuScraper: ConvertToDoubleForYield�B

#### 1.1.4 �v���p�e�B�Z�b�g�E�f�[�^�i�[�����̏d��
- AssetInfo�ւ̒l�Z�b�g�������eScraper�łقړ����p�^�[���ŌJ��Ԃ���Ă���B
  - ��FstockInfo.DividendYield = ...�AstockInfo.ShareholderBenefitYield = ... �ȂǁB
  - �e�[�u���s���[�v���J�������o���v���p�e�B�Z�b�g�̗��ꂪ�e���ŏd���B

#### 1.1.5 ��O�����E���M���O�̏d��
- try-catch�⃍�M���O�������e���\�b�h�łقړ����`�ŌJ��Ԃ���Ă���B
  - 404��HttpRequestException�A���̑���O�̃n���h�����O�B
  - ���O�o�́iURL��G���[���e�j�B

#### 1.1.6 ���Y��ʂ��Ƃ̕���iif/switch�j�̑��p
- ���Y��ʁi���{��/�č���/ETF/�w���Ȃǁj���Ƃ�if��switch�ŕ��򂵁A��������剻�B
  - KabutanScraper, YahooScraper�Ō����B
  - �����ʒǉ����ɂ���ɕ��򂪑����錜�O�B

---

## 2. AssetInfo�̌���

- AssetInfo�͒��ۊ��N���X�ł���A�p���ɂ��g�����\�B
- �ˑ��������iDI�j�Ŋe��X�g���e�W�[�⃊�|�W�g�����_��ɍ����ւ��\�B
- ��v�ȍ����E�����E�D�ҁE�z���E�ƐсE�`���[�g�E�J�����Ȃǂ�ԗ��I�Ƀv���p�e�B�Ƃ��ĕێ��B
- ����E�v�Z���W�b�N�̓X�g���e�W�[�p�^�[���ňϏ�����Ă���B

�y�C�����C����i�����j�z
public abstract class AssetInfo
{
    // �ˑ�������
    protected IExternalSourceUpdatable _updater;
    protected IOutputFormattable _formatter;
    protected IAssetRepository _repository;
    protected IAssetJudgementStrategy _judgementStrategy;
    protected IAssetCalculator _calculator;
    protected IAssetSetupStrategy _setupStrategy;
    // ...�i���j...
    public virtual string Code { get; set; }
    public virtual string Classification { get; set; }
    public virtual double DividendYield { get; set; }
    public virtual double ShareholderBenefitYield { get; set; }
    // ...�i���j...
}

---

## 3. ����̊g�����m�ۂ̂��߂̃��t�@�N�^�����O��

### 3.1 Scraper�̃C���^�[�t�F�[�X���{DI/Factory�Ή�

�y�T���v���z
public interface IAssetScraper
{
    Task ScrapeAsync(AssetInfo assetInfo, ScrapeTarget target);
}
public enum ScrapeTarget
{
    Dividend, Yutai, Finance, History, Top, Profile, Disclosure
}
- Scraper�͂��̃C���^�[�t�F�[�X���������ADI��Factory�Œ����E�����B
- ScrapeTarget�Łu�����擾���邩�v�𖾎��I�Ɏw��B

---

### 3.2 Scrape�헪�̕����iStrategy�p�^�[���j

�y�T���v���z
public interface IAssetScrapeStrategy
{
    string GetUrl(AssetInfo assetInfo, ScrapeTarget target);
    Dictionary<string, string> GetXPaths(ScrapeTarget target);
    void Parse(HtmlDocument doc, AssetInfo assetInfo, ScrapeTarget target);
}
- �e���Y��ʂ��Ƃ�Strategy���������AURL��XPath�A�p�[�X���W�b�N�𕪗��B
- Scraper�{�̂�Strategy�ɏ������Ϗ����A��ʒǉ�����Strategy�ǉ��݂̂ōςށB

---

### 3.3 AssetInfo�̌p���ɂ��g��

- USAssetInfo : AssetInfo �̂悤�ɁA�č�����V��ʂɓ��������v���p�e�B�E���W�b�N��ǉ��\�B

---

### 3.4 �}�[�W�@�\�̓���

�y�T���v���z
public interface IAssetInfoMerger
{
    AssetInfo Merge(IEnumerable<AssetInfo> sources);
}
- �����T�C�g����擾���������}�[�W����N���X��p�ӂ��A�D�揇�ʂ�}�[�W���[�����_��Ɋg���\�B

---

### 3.5 DI/Factory�ɂ��_��ȑg�ݍ��킹

�y�T���v���z
public class ScrapeStrategyFactory
{
    public IAssetScrapeStrategy Create(AssetInfo assetInfo)
    {
        return assetInfo switch
        {
            JapaneseAssetInfo => new JapaneseStockScrapeStrategy(),
            USAssetInfo => new USStockScrapeStrategy(),
            _ => throw new NotSupportedException()
        };
    }
}
- DI�R���e�i��Factory��Scraper��Strategy�AMerger��g�ݍ��킹�Ē����B

---

## 4. ��̓I�Ȋg����F�č��ʊ��iUSAssetInfo�j�Ή�

1. USAssetInfo : AssetInfo ��ǉ�
2. USStockScrapeStrategy ���������AURL��XPath�A�p�[�X���W�b�N���`
3. Factory/DI�Łu�č��ʊ��v��ʂ̏ꍇ�� USStockScrapeStrategy �𗘗p
4. Scraper�{�̂�Strategy�ɏ������Ϗ����邾���Ȃ̂ŁA�����R�[�h�ւ̉e���͍ŏ���

---

## 5. ���_

- �����AssetInfo�\���́A�p���EDI�E�X�g���e�W�[�p�^�[�������p���邱�Ƃō����g���������B
- Scraper/Strategy/Factory/�}�[�W���̓����ŁA�V��ʁE�V�T�C�g�E���}�[�W�ɂ��_��ɑΉ��\�B
- ����́u�h���N���X�̒ǉ��v�uDI/Factory�̊g���v�uStrategy/Updater�̎����ǉ��v�ŁA�č�����V��ʁA�����T�C�g�Ή����X���[�Y�Ɏ����ł���B

---

## 6. ���t�@�N�^�����O�̐i�ߕ��i�i�K�I�Ȏ��{�āj

�{�h�L�������g�ŋ������ۑ�ƃ��t�@�N�^�����O�Ă𓥂܂��A�ȉ��̎菇�Œi�K�I�ɐi�߂邱�Ƃ𐄏�����B

### 6.1 ����̏d���E����̃��X�g�A�b�v
- �eScraper�Ŏ��Y��ʂ��Ƃɕ��򂵂Ă���ӏ���A�d�����Ă��鏈���iURL�����AXPath�I���A�l�̃p�[�X�A�v���p�e�B�Z�b�g�A��O�����Ȃǁj��􂢏o���B
- �ǂ������ʉ��E�������ׂ����𖾊m�ɂ���B

### 6.2 �C���^�[�t�F�[�X�E�헪�p�^�[���̐��`�쐬
- IAssetScraper��IAssetScrapeStrategy�ȂǁA����̊g�������������C���^�[�t�F�[�X�⒊�ۃN���X�̐��`��V�K�쐬����B

### 6.3 �����ȒP�ʂł̃��t�@�N�^�����O�J�n
- �܂���URL������XPath�I���ȂǁA�e���͈͂��������������狤�ʉ��E������i�߂�B
- �����̃e�X�g�⓮��m�F�����Ȃ���A�i�K�I�ɐi�߂�B

### 6.4 Factory/DI�̓���
- Scraper��Strategy�̐����E�������@�iFactory��DI�R���e�i�̐ݒ�j���������A���X�ɓ�������B

### 6.5 AssetInfo�̌p���݌v
- USAssetInfo�ȂǁA����ǉ��\��̔h���N���X�̐݌v���j�����߂�B
- �K�v�ɉ����āA������AssetInfo�̃v���p�e�B��R���X�g���N�^���������B

### 6.6 �}�[�W�@�\�̓���
- �����T�C�g����擾����AssetInfo���}�[�W����IAssetInfoMerger���������A��񓝍��̎d�g�݂𐮔�����B

---

#### �⑫
- �����Ȃ�S�̂�傫������������̂ł͂Ȃ��A�u�����ȒP�ʂŋ��ʉ�������m�F�����̒P�ʂցv�Ƃ����T�C�N���Ői�߂邱�ƂŁA���X�N��}�������Ɋg���������߂邱�Ƃ��ł���B
- �ŏ��̃X�e�b�v�Ƃ��āu�d���E����̃��X�g�A�b�v�v�Ɓu�C���^�[�t�F�[�X���`�̍쐬�v���璅�肷�邱�Ƃ𐄏�����B

---