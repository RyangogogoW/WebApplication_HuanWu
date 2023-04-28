using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WebApplication_HuanWu.Services
{
    public interface IEmpService
    {
        Task<IEnumerable<dynamic>> EmployeeInfo();
    }
}