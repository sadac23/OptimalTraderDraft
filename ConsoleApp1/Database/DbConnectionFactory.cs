using System;
using System.Data.SQLite;

namespace ConsoleApp1.Database
{
    /// <summary>
    /// �A�v���S�̂ŗ��p�ł���V���O���g��DB�R�l�N�V�����t�@�N�g��
    /// </summary>
    public static class DbConnectionFactory
    {
        private static string? _connectionString;
        private static SQLiteConnection? _singletonConnection;
        private static readonly object _lock = new();

        /// <summary>
        /// �A�v���N�����Ɉ�x�����Ăяo���Đڑ���������Z�b�g
        /// </summary>
        public static void Initialize(string connectionString)
        {
            lock (_lock)
            {
                if (_singletonConnection != null)
                {
                    _singletonConnection.Dispose();
                    _singletonConnection = null;
                }
                _connectionString = connectionString;
            }
        }

        /// <summary>
        /// �V���O���g����SQLiteConnection�C���X�^���X��Ԃ�
        /// </summary>
        public static SQLiteConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("DbConnectionFactory is not initialized.");

            lock (_lock)
            {
                if (_singletonConnection == null)
                {
                    _singletonConnection = new SQLiteConnection(_connectionString);
                    _singletonConnection.Open();
                }
                else if (_singletonConnection.State != System.Data.ConnectionState.Open)
                {
                    _singletonConnection.Open();
                }
                return _singletonConnection;
            }
        }

        /// <summary>
        /// �R�l�N�V�����𖾎��I�ɃN���[�Y�E�j������ꍇ�ɌĂяo��
        /// </summary>
        public static void Dispose()
        {
            lock (_lock)
            {
                if (_singletonConnection != null)
                {
                    _singletonConnection.Dispose();
                    _singletonConnection = null;
                }
            }
        }

        /// <summary>
        /// �e�X�g�p: �O���Ő�������SQLiteConnection�𒼐ڃZ�b�g����
        /// </summary>
        public static void SetConnection(SQLiteConnection connection)
        {
            lock (_lock)
            {
                if (_singletonConnection != null && !object.ReferenceEquals(_singletonConnection, connection))
                {
                    _singletonConnection.Dispose();
                }
                _singletonConnection = connection;
                _connectionString = connection.ConnectionString;
            }
        }
    }
}