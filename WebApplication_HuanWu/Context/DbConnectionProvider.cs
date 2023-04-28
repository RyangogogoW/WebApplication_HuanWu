using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;

namespace WebApplication_HuanWu.Context
{
    public abstract class DbConnectionProvider : IDbConnectionProvider
    {
        protected readonly string ContextName;

        protected DbConnectionProvider(string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
                throw new ArgumentNullException(
                    nameof(contextName),
                    "Context name cannot be null or empty. Please check your configuration file for the connection string.");

            ContextName = contextName;
        }

        public virtual IDbConnection CreateConnection()
        {
            var localConnectionString = ConfigurationManager.ConnectionStrings[ContextName];

            var factory = DbProviderFactories.GetFactory(localConnectionString.ProviderName);

            var connection = factory.CreateConnection();

            if (connection != null)
            {
                connection.ConnectionString = localConnectionString.ToString();

                return connection;
            }

            return null;
        }

        //With Dynamic Server Name
        public virtual IDbConnection CreateConnection(string serverName)
        {
            var localConnectionString = ConfigurationManager.ConnectionStrings[ContextName];

            var factory = DbProviderFactories.GetFactory(localConnectionString.ProviderName);

            var connection = factory.CreateConnection();

            if (connection != null)
            {
                connection.ConnectionString = localConnectionString.ToString().Replace("$ServerName", serverName);

                return connection;
            }

            return null;
        }
    }
}