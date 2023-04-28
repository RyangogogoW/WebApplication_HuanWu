using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace WebApplication_HuanWu.Context
{
    public interface IDbConnectionProvider
    {
        IDbConnection CreateConnection();

        IDbConnection CreateConnection(string serverName);
    }
}