# �f�[�^�x�[�X����̃��t�@�N�^�����O���������i��̗�t���j

## 1. DbConnection�̊Ǘ����@
- **����**: �e���� `new SQLiteConnection(...)` ��s�x�������Ausing�ŕ��Ă���B
- **����**:  
  - �R�l�N�V�������������� `DbConnectionFactory` �Ȃǂ̃N���X�ɂ܂Ƃ߂邱�ƂŁA�ڑ��������ݒ�̈ꌳ�Ǘ����\�B
  - ��:  
    ```csharp
    public class DbConnectionFactory
    {
        private readonly string _connectionString;
        public DbConnectionFactory(string connectionString) => _connectionString = connectionString;
        public SQLiteConnection Create() => new SQLiteConnection(_connectionString);
    }
    ```
  - DI�i�ˑ��������j�����p���A�e�X�g���̓C��������DB�⃂�b�N�ɍ����ւ��₷������B

## 2. SQL�N�G���̏d���E�n�[�h�R�[�f�B���O
- **����**: SQL�����e�N���X�E���\�b�h���ɒ��ڋL�q����Ă���B
- **����**:  
  - SQL����萔���p�̃��|�W�g���N���X�ɂ܂Ƃ߂�B
  - ��:  
    ```csharp
    public static class SqlQueries
    {
        public const string SelectHistory = "SELECT * FROM history WHERE code = @code ORDER BY date DESC LIMIT @limit";
    }
    ```
  - ����ɂ��ASQL�̏C���⃌�r���[���e�ՂɂȂ�A�d����~�X��h����B

## 3. DatabaseAccessor�̊��p�E���ۉ�
- **����**: �ꕔ�� `DatabaseAccessor` ���g���A���͒���ADO.NET�𗘗p���Ă���B
- **����**:  
  - ���ׂĂ�DB�A�N�Z�X�� `IDatabaseAccessor` �C���^�[�t�F�[�X�o�R�ɓ���B
  - ��:  
    ```csharp
    public interface IDatabaseAccessor
    {
        List<Dictionary<string, object>> ExecuteQuery(string query, Dictionary<string, object> parameters);
    }
    ```
  - ����ɂ��A�e�X�g���Ƀ��b�N��X�^�u�𒍓��ł��A�e�X�g�e�Ր������シ��B

## 4. ��O�����E�G���[�n���h�����O
- **����**: try-catch�ŗ�O������Ԃ��Ă���A�܂���catch���Ȃ��ӏ�������B
- **����**:  
  - ��O�������͕K�����O�o�͂��A�K�v�ɉ����ď�ʂɍ�throw����B
  - ��:  
    ```csharp
    try
    {
        // DB����
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "DB���쒆�ɃG���[����");
        throw;
    }
    ```
  - ��O�𖳎������A��Q�����⃆�[�U�[�ʒm�Ɋ��p�ł���悤�ɂ���B

## 5. �e�X�g�e�Ր��̌���
- **����**: DB�A�N�Z�X�����ړI�Ȃ��߁A���j�b�g�e�X�g������B
- **����**:  
  - DB�A�N�Z�X�������C���^�[�t�F�[�X�����A�e�X�g���̓��b�N��C��������DB�i��: SQLite�� `Data Source=:memory:`�j�𗘗p�B
  - ��:  
    ```csharp
    var accessor = new Mock<IDatabaseAccessor>();
    accessor.Setup(a => a.ExecuteQuery(...)).Returns(...);
    ```
  - �e�X�g�p��DB�������E�N���[���A�b�v��������������B

## 6. �g�����U�N�V�����Ǘ�
- **����**: ������DB�X�V���ʂɎ��s���Ă���ӏ�������B
- **����**:  
  - ������DB�X�V����A�̏����ŕK�v�ȏꍇ�́A`SQLiteTransaction` �ȂǂŖ����I�Ƀg�����U�N�V�����Ǘ����s���B
  - ��:  
    ```csharp
    using (var connection = new SQLiteConnection(...))
    {
        connection.Open();
        using (var transaction = connection.BeginTransaction())
        {
            // �����̃R�}���h
            transaction.Commit();
        }
    }
    ```
  - ����ɂ��A�r�����s���̃��[���o�b�N���\�ƂȂ�B

## 7. SQL�C���W�F�N�V�����΍�
- **����**: �p�����[�^������Ă��Ȃ�SQL�����݂��Ă���\��������B
- **����**:  
  - ���ׂĂ�SQL�ŕK���p�����[�^����O�ꂵ�A������A���ɂ��SQL�����������B
  - ��:  
    ```csharp
    command.Parameters.AddWithValue("@code", code);
    ```

## 8. �R�l�N�V�����v�[�����O�̊��p
- **����**: SQLite�ł̓R�l�N�V�����v�[�����O�̉��b�͌���I�����A�����I��DB�ڍs���l������B
- **����**:  
  - �R�l�N�V�����̎g���񂵂�v�[�����O�ݒ���ӎ������݌v�ɂ��Ă����ƁARDBMS�ڍs�����Ή����₷���B

---

## �܂Ƃ�

- DB�A�N�Z�X�̃��b�p�[/�C���^�[�t�F�[�X����DI�Ή�
- SQL���E�R�l�N�V���������̈ꌳ�Ǘ�
- ��O�����E�G���[�n���h�����O�̋���
- �e�X�g�e�Ր��̂��߂̃��b�N�E�C��������DB�Ή�
- �g�����U�N�V�����Ǘ��̖��m��
- SQL�C���W�F�N�V�����΍�̓O��

---