# �O�����擾�����̒����܂Ƃ�

## ����
- �E�H�b�`���X�g�̖��������Ԃɏ������A����^�C�~���O����S�����ŊO���T�C�g����̏��擾����������B
- �������A�擾�l��null��󕶎��ƂȂ�B
- �G���[���O��x���͏o�͂��ꂸ�A�A�v���͐���I������B

## ��Ȍ������
1. **�O���ڑ���F�؂̃O���[�o���Ȏ��s**
   - API�L�[�̊����؂�AIP�u���b�N�A�Z�b�V�����؂ꓙ�ňȍ~�̎擾���S�Ď��s����\���B

2. **��O������Ԃ���Ă���**
   - �擾�����ŗ�O���������Ă�catch���ꂸ�A�����o�͂���Ȃ��B

3. **��Ԃ̋��L�E����p**
   - �O���[�o���ϐ���V���O���g���A�ÓI�v���p�e�B�̕s���ȏ�Ԃ��g�y���Ă���\���B

4. **�񓯊������̖��**
   - await�R���A�^�X�N�̗�O���\�ʉ����Ȃ��܂܏������i��ł���B

## Updater�N���X�̎�������
- `JapaneseStockUpdater`�A`JapaneseETFUpdater`�A`IndexUpdater` �� `UpdateFromExternalSourceAsync` �ł́A�����̊O���X�N���C�s���O������ `Task.Run` �ŕ�����s���A`Task.WhenAll` �őҋ@���Ă���B
- �������**try-catch�ɂ���O�������Ȃ��A���s���ɉ����o�͂���Ȃ�**�B
- ���s���̓v���p�e�B��null��󕶎��̂܂܂ƂȂ�B

## ���P��
- �eTask����try-catch���Acatch���ɕK�����O�o�͂���B
- ���s���͖����I�ɗ�O��throw���A��ʂŌ��m�E�ʒm����B
- �K�v�ɉ����ă��g���C������ǉ�����B

---

## ���t�@�N�^�����O��̋�̓I�Ȏ�����i�C�����C���L�q�j

- **YahooScraper��ScrapeHistory���\�b�h**  
  ��O�������́A���O�o�͌�ɗ�O����throw����B  
  �C�����C���L�q�F
  `try { ... } catch (Exception e) { CommonUtils.Instance.Logger.LogError(e.Message, e); throw; }`

- **JapaneseStockUpdater��UpdateFromExternalSourceAsync���\�b�h**  
  �eTask�i�X�N���C�s���O�����j�� `try-catch` �Ń��b�v���A���s���͕K�����O�o�́{��O��throw����B  
  �C�����C���L�q�F
  `Task.Run(async () => { try { await ... } catch (Exception ex) { CommonUtils.Instance.Logger.LogError(ex.Message, ex); throw; } })`  
  ���ׂĂ�Task������i`Task.WhenAll`�j�A`try-catch`�ŏW��I�ɃG���[���L�^���A�K�v�ɉ����ď�ʂ֒ʒm����B  
  �C�����C���L�q�F
  `try { await Task.WhenAll(tasks); } catch (Exception ex) { CommonUtils.Instance.Logger.LogError(ex.Message, ex); throw; }`

- **���g���C����**  
  �O���T�C�g�̈ꎞ�I�Ȏ��s�ɔ����A�K�v�ɉ����ă��g���C�񐔂�ҋ@���Ԃ�ݒ肵�A������擾�����݂�B  
  �C�����C���L�q�F
  `while (retryCount < maxRetry) { try { ... } catch { CommonUtils.Instance.Logger.LogError(...); retryCount++; Thread.Sleep(retryDelayMs); } }`

---

## �X�N���C�s���O���̗�O�������ɏ������p�����邽�߂̑Ή����j

- �e�������Ƃ̃X�N���C�s���O������ `try-catch` �Ń��b�v���A��O�������͕K�����O�o�͂���B
- ��O��������catch�u���b�N����throw�����A���̖����̏������p������B
- �C�����C���L�q�F
  `foreach (var stock in watchList) { try { await scraper.ScrapeHistory(stock, from, to); } catch (Exception ex) { CommonUtils.Instance.Logger.LogError($"�����R�[�h: {stock.Code} �̃X�N���C�s���O���s: {ex.Message}", ex); } }`
- ������s�̏ꍇ���eTask���œ��l��try-catch���A���s���̓��O�o�͂̂ݍs���Athrow���Ȃ����ƂőS�̂̏������~�߂Ȃ��B
- �K�v�ɉ����āA���s���������̃��X�g���L�^���A�㑱������ʒm�Ɋ��p����B

---

���̂悤�ȕ��j�ɂ��A1�����̃X�N���C�s���O���s���S�̂̏�����~�ɂȂ��炸�A���肵���o�b�`�����������ł���B