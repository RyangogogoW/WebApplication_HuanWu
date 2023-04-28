using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Threading.Tasks;

namespace WebApplication_HuanWu.Context
{
    public static class DapperConnectionHelper<TProvider> where TProvider : class, IDbConnectionProvider
    {
        private static readonly DapperConnectionPool ConnectionPool = new DapperConnectionPool();

        private static T WithProvidedConnection<T>(Func<IDbConnection, T> action)
        {
            var connectionTask = ConnectionPool.GetConnectionAsync<TProvider>();

            connectionTask.Wait();

            using (var connection = connectionTask.Result)
            {
                try
                {
                    connection.Open();

                    return action(connection);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private static async Task<T> WithProvidedConnectionAsync<T>(Func<IDbConnection, Task<T>> action)
        {
            using (var connection = await ConnectionPool.GetConnectionAsync<TProvider>())
            {
                try
                {
                    await connection.OpenAsync();

                    return await action(connection);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        //With Dynamic Server Name
        private static async Task<T> WithProvidedConnectionAsync<T>(Func<IDbConnection, Task<T>> action, string serverName)
        {
            using (var connection = await ConnectionPool.GetConnectionAsync<TProvider>(serverName))
            {
                try
                {
                    await connection.OpenAsync();

                    return await action(connection);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Performs an action WITHOUT a transaction using a database connection.
        /// 
        /// This method is synchronous and trims all string values on entities (nested entities too).
        /// </summary>
        /// <typeparam name="T">Action return type.</typeparam>
        /// <param name="action">Action that uses the database connection.</param>
        /// <returns>A result from the defined action.</returns>
        public static T WithConnection<T>(Func<IDbConnection, T> action)
        {
            return WithProvidedConnection(
                connection =>
                {
                    var result = action(connection);

                    if (result == null) { return default(T); }

                    ToExpandoObject(ref result);

                    result.TrimStringProperties();

                    return result;
                });
        }

        /// <summary>
        /// Performs an action WITH a transaction using a database connection.
        /// 
        /// This method is synchronous and trims all string values on entities (nested entities too).
        /// </summary>
        /// <typeparam name="T">Action return type.</typeparam>
        /// <param name="action">Action that uses the database connection.</param>
        /// <param name="level">Transaction locking level.</param>
        /// <returns>A result from the defined action.</returns>
        public static T WithConnection<T>(Func<IDbConnection, IDbTransaction, T> action, IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            return WithProvidedConnection(
                connection =>
                {
                    using (var transaction = connection.BeginTransaction(level))
                    {
                        try
                        {
                            var result = action(connection, transaction);

                            transaction.Commit();

                            if (result == null) { return default(T); }

                            ToExpandoObject(ref result);

                            result.TrimStringProperties();

                            return result;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();

                            throw;
                        }
                    }
                });
        }

        /// <summary>
        /// Performs an action WITHOUT a transaction using a database connection.
        /// 
        /// This method is asynchronous and trims all string values on entities (nested entities too).
        /// </summary>
        /// <typeparam name="T">Action return type.</typeparam>
        /// <param name="action">Action that uses the database connection.</param>
        /// <returns>A Task that resturns the result from the defined action.</returns>
        public static async Task<T> WithConnectionAsync<T>(Func<IDbConnection, Task<T>> action)
        {
            return await WithProvidedConnectionAsync(
                async connection =>
                {
                    var result = await action(connection);

                    if (result == null) { return default(T); }

                    ToExpandoObject(ref result);

                    result.TrimStringProperties();

                    return result;
                });
        }

        /// <summary>
        /// Performs an action WITHOUT a transaction using a database connection AND a specific server name.
        /// </summary>
        public static async Task<T> WithServerNameConnectionAsync<T>(Func<IDbConnection, Task<T>> action, string serverName)
        {
            return await WithProvidedConnectionAsync(
                async connection =>
                {
                    var result = await action(connection);

                    if (result == null) { return default(T); }

                    ToExpandoObject(ref result);

                    result.TrimStringProperties();

                    return result;
                },
                serverName);
        }

        /// <summary>
        /// Performs an action WITH a transaction using a database connection.
        /// 
        /// This method is asynchronous and trims all string values on entities (nested entities too).
        /// </summary>
        /// <typeparam name="T">Action return type.</typeparam>
        /// <param name="action">Action that uses the database connection.</param>
        /// <param name="level">Transaction locking level.</param>
        /// <returns>A Task that returns the result from the defined action.</returns>
        public static async Task<T> WithConnectionAsync<T>(Func<IDbConnection, IDbTransaction, Task<T>> action, IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            return await WithProvidedConnectionAsync(
                async connection =>
                {
                    using (var transaction = connection.BeginTransaction(level))
                    {
                        try
                        {
                            var result = await action(connection, transaction);

                            transaction.Commit();

                            if (result == null) { return default(T); }

                            ToExpandoObject(ref result);

                            result.TrimStringProperties();

                            return result;
                        }
                        catch (SqlException ex)
                        {
                            transaction.Rollback();

                            // handle sql query timeout separately
                            if (ex.Number == -2)
                            {
                                throw new TimeoutException();
                            }

                            throw;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();

                            throw;
                        }
                    }
                });
        }

        /// <summary>
        /// Performs an action WITH a transaction using a database connection AND a specific server name.
        /// </summary>
        public static async Task<T> WithServerNameConnectionAsync<T>(Func<IDbConnection, IDbTransaction, Task<T>> action, string serverName, IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            return await WithProvidedConnectionAsync(
                async connection =>
                {
                    using (var transaction = connection.BeginTransaction(level))
                    {
                        try
                        {
                            var result = await action(connection, transaction);

                            transaction.Commit();

                            if (result == null) { return default(T); }

                            ToExpandoObject(ref result);

                            result.TrimStringProperties();

                            return result;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();

                            throw;
                        }
                    }
                },
                serverName);
        }

        /// <summary>
        /// Performs an action using a database connection.
        /// 
        /// This method is synchronous and trims all string values on entities (nested entities too).
        /// </summary>
        /// <typeparam name="T">Action return type.</typeparam>
        /// <param name="action">Action that uses the database connection.</param>
        /// <param name="useTransaction">Whether to use a transaction or not.</param>
        /// <param name="level">Transaction locking level.</param>
        /// <returns>A Task that returns the result from the defined action.</returns>
        public static T WithConnection<T>(
            Func<IDbConnection, IDbTransaction, T> action,
            bool useTransaction,
            IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            return useTransaction
                ? WithConnection(action, level)
                : WithConnection(connection => action(connection, null));
        }

        /// <summary>
        /// Performs an action using a database connection.
        /// 
        /// This method is asynchronous and trims all string values on entities (nested entities too).
        /// </summary>
        /// <typeparam name="T">Action return type.</typeparam>
        /// <param name="action">Action that uses the database connection.</param>
        /// <param name="useTransaction">Whether to use a transaction or not.</param>
        /// <param name="level">Transaction locking level.</param>
        /// <returns>A Task that returns the result from the defined action.</returns>
        public static async Task<T> WithConnectionAsync<T>(
            Func<IDbConnection, IDbTransaction, Task<T>> action,
            bool useTransaction,
            IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            return
                await
                (useTransaction
                    ? WithConnectionAsync(action, level)
                    : WithConnectionAsync(connection => action(connection, null)));
        }

        /// <summary>
        /// Performs an action using a database connection AND a specific server name.
        /// </summary>
        /// <typeparam name="T">Action return type.</typeparam>
        /// <param name="action">Action that uses the database connection.</param>
        /// <param name="serverName">Specific a server name for the database connection.</param>
        /// <param name="useTransaction">Whether to use a transaction or not.</param>
        /// <param name="level">Transaction locking level.</param>
        /// <returns>A Task that returns the result from the defined action.</returns>
        public static async Task<T> WithServerNameConnectionAsync<T>(
            Func<IDbConnection, IDbTransaction, Task<T>> action,
            string serverName,
            bool useTransaction,
            IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            return
                await
                (useTransaction
                    ? WithServerNameConnectionAsync(action, serverName, level)
                    : WithServerNameConnectionAsync(connection => action(connection, null), serverName));
        }

        private static void ToExpandoObject<T>(ref T o)
        {
            var type = o.GetType();

            if (!type.Name.StartsWith("DapperRow", StringComparison.Ordinal))
            {
                return;
            }

            var originalRecords = (IDictionary<string, object>)o;
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (var record in originalRecords) { expando[record.Key] = record.Value; }

            dynamic result = expando;

            o = result;
        }
    }
}