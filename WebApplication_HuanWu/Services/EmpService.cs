using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebApplication_HuanWu.Context;

namespace WebApplication_HuanWu.Services
{
    public class EmpService : IEmpService
    {
        public async Task<IEnumerable<dynamic>> EmployeeInfo()
        {
            var query = @"select * from EmpInfo";

            var result = await DapperConnectionHelper<LOCALConnectionProvider>.WithConnectionAsync(async (conn, trx) =>
            {
                return await conn.QueryAsync<dynamic>(query);
            }, false);
            return result;
        }
    }
}