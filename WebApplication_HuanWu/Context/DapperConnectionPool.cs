using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication_HuanWu.Context
{
    public sealed class DapperConnectionPool
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> Pool = new ConcurrentDictionary<string, SemaphoreSlim>();

        static DapperConnectionPool()
        {
            var connectionStrings = ConfigurationManager.ConnectionStrings;

            foreach (ConnectionStringSettings connectionString in connectionStrings)
            {
                var poolSize = GetMaxPoolSize(connectionString);

                Pool.TryAdd(connectionString.ConnectionString, new SemaphoreSlim(poolSize, poolSize));
            }
        }

        private static int GetMaxPoolSize(ConnectionStringSettings connectionString)
        {
            return GetMaxPoolSize(connectionString.ConnectionString);
        }

        private static int GetMaxPoolSize(string connectionString)
        {
            var sections = connectionString.Split(';');

            var maxPoolSection =
                    sections.Where(section => section.StartsWith("Max Pool Size", StringComparison.OrdinalIgnoreCase)).ToArray();

            var poolSize = maxPoolSection.Length > 0 ? int.Parse(maxPoolSection.First().Split('=').Last()) : 100;

            return poolSize;
        }

        public async Task<DbConnection> GetConnectionAsync<TProvider>() where TProvider : class, IDbConnectionProvider
        {
            var provider = Activator.CreateInstance(typeof(TProvider)) as TProvider;

            if (provider == null) { throw new NullReferenceException("Database connection provider must implement IDbConnectionProvider."); }

            var connection = provider.CreateConnection();

            var wrappedConnection = new DapperPooledConnection(connection);


            if (Pool.ContainsKey(connection.ConnectionString))
            {
                var existingSemaphore = Pool[connection.ConnectionString];

                await existingSemaphore.WaitAsync();

                wrappedConnection.OnRelease += sender =>
                {
                    existingSemaphore.Release();
                };

                return wrappedConnection;
            }

            var maxPoolSize = GetMaxPoolSize(connection.ConnectionString);

            var semaphore = new SemaphoreSlim(maxPoolSize, maxPoolSize);

            if (Pool.TryAdd(connection.ConnectionString, semaphore))
            {
                await semaphore.WaitAsync();

                wrappedConnection.OnRelease += sender =>
                {
                    semaphore.Release();
                };

                return wrappedConnection;
            }

            throw new InvalidOperationException("Failed to create new connection pool");
        }

        //With Dynamic Server Name
        public async Task<DbConnection> GetConnectionAsync<TProvider>(string serverName) where TProvider : class, IDbConnectionProvider
        {
            var provider = Activator.CreateInstance(typeof(TProvider)) as TProvider;

            if (provider == null) { throw new NullReferenceException("Database connection provider must implement IDbConnectionProvider."); }

            var connection = provider.CreateConnection(serverName);

            var wrappedConnection = new DapperPooledConnection(connection);


            if (Pool.ContainsKey(connection.ConnectionString))
            {
                var existingSemaphore = Pool[connection.ConnectionString];

                await existingSemaphore.WaitAsync();

                wrappedConnection.OnRelease += sender =>
                {
                    existingSemaphore.Release();
                };

                return wrappedConnection;
            }

            var maxPoolSize = GetMaxPoolSize(connection.ConnectionString);

            var semaphore = new SemaphoreSlim(maxPoolSize, maxPoolSize);

            if (Pool.TryAdd(connection.ConnectionString, semaphore))
            {
                await semaphore.WaitAsync();

                wrappedConnection.OnRelease += sender =>
                {
                    semaphore.Release();
                };

                return wrappedConnection;
            }

            throw new InvalidOperationException("Failed to create new connection pool");
        }

        private delegate void OnConnectionRelease(object sender);

        private sealed class DapperPooledConnection : DbConnection
        {
            private readonly DbConnection _internalConnection;

            public event OnConnectionRelease OnRelease;

            public DapperPooledConnection(IDbConnection connection)
            {
                _internalConnection = (DbConnection)connection;
            }

            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            {
                return _internalConnection.BeginTransaction(isolationLevel);
            }

            public override void Close()
            {
                _internalConnection.Close();

                TriggerOnRelease();
            }

            public override void ChangeDatabase(string databaseName)
            {
                _internalConnection.ChangeDatabase(databaseName);
            }

            public override void Open()
            {
                var connectionTask = _internalConnection.OpenAsync();

                connectionTask.Wait();
            }

            public override Task OpenAsync(CancellationToken cancellationToken)
            {
                return _internalConnection.OpenAsync(cancellationToken);
            }

            public override string ConnectionString
            {
                get { return _internalConnection.ConnectionString; }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public override string Database => _internalConnection.Database;

            public override ConnectionState State => _internalConnection.State;

            public override string DataSource => _internalConnection.DataSource;

            public override string ServerVersion => _internalConnection.ServerVersion;

            protected override DbCommand CreateDbCommand()
            {
                return _internalConnection.CreateCommand();
            }

            private void TriggerOnRelease()
            {
                OnRelease?.Invoke(this);
            }
        }
    }
}