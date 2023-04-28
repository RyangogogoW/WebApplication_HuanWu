using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Configuration;
using WebApplication_HuanWu.Models;
using WebApplication_HuanWu.Services;
using System.Web.Routing;

namespace WebApplication_HuanWu.Controllers
{
    public class EmpController : ApiController
    {
        public readonly IEmpService empService;
        
        public EmpController(IEmpService empService)
        {
            this.empService = empService;
        }
        
        [Route("api/Emp/AllEmployeeInformation")]
        [HttpGet]
        public async Task<IHttpActionResult> EmployeeInfo()
        {
            var result = await empService.EmployeeInfo();
            return Ok(result);
        }
        //#region
        //[Route("api/AllEmployeeInfo")]
        //[HttpGet]
        //public HttpResponseMessage getEmpInfo()
        //{
        //    var sql = @"select * from EmpInfo";
        //    DataTable dataTable = new DataTable();
        //    using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["hahaha"].ConnectionString))
        //    using (var cmd = new SqlCommand(sql, con))
        //    using (var da = new SqlDataAdapter(cmd))
        //    {
        //        cmd.CommandType = CommandType.Text; //将双引号内原封不到导入sql语句
        //        da.Fill(dataTable);
        //    }
        //    return Request.CreateResponse(HttpStatusCode.OK, dataTable);
        //}

        ////"+emp.ID+@" invoke parameter
        //[Route("api/InsertEmployee")]
        //[HttpPost]
        //public string insertEmployee(Employee emp)
        //{
        //    try
        //    {
        //        var query = @"insert into EmpInfo values('"+emp.ID+@"','"+emp.FirstName+@"','"+emp.LastName+ @"','"+emp.Email+ @"','"+emp.DeptID+@"')"; //@用于sql语句
        //        DataTable dataTable = new DataTable();
        //        using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["hahaha"].ConnectionString))
        //        using(var cmd = new SqlCommand(query, con))
        //        using (var da = new SqlDataAdapter(cmd))
        //        {
        //            cmd.CommandType = CommandType.Text;
        //            da.Fill(dataTable);
        //        }
        //        return "Added Successfully!!!";
        //    }
        //    catch (Exception)
        //    {

        //        return "Failed to Add";
        //    }
        //}

        //[Route("api/InsertEmployee")]
        //[HttpPut]
        //public string updateEmployee(Employee emp)
        //{
        //    try
        //    {
        //        var query = @"update EmpInfo set ";
        //        if(!(emp.FirstName is null) &&!emp.FirstName.Equals(""))
        //        {
        //            var query2 = @"FirstName = '" + emp.FirstName + @"',";
        //            query = query + query2;
        //        }
        //        if(!(emp.LastName is null) && !emp.LastName.Equals(""))
        //        {
        //            var query3 = @"LastName = '" + emp.LastName + @"',";
        //            query = query + query3;
        //        }
        //        if(!(emp.Email is null) && !emp.Email.Equals(""))
        //        {
        //            var query4 = @"Email = '" + emp.Email + @"',";
        //            query = query + query4;
        //        }
        //        if(!(emp.DeptID is null) && !emp.DeptID.Equals(""))
        //        {
        //            var query5 = @"DeptID = '" + emp.DeptID + @"',";
        //            query = query + query5;
        //        }
        //        query = query.Substring(0, query.Length - 1);
        //        var query6 = @"where ID = '" + emp.ID + @"'";
        //        query = query + query6;

        //        DataTable dataTable = new DataTable();
        //        using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["hahaha"].ConnectionString))
        //        using(var cmd = new SqlCommand(query, con))
        //        using (var da = new SqlDataAdapter(cmd))
        //        {
        //            cmd.CommandType = CommandType.Text;
        //            da.Fill(dataTable);
        //        }
        //        return "Update Successfully";
        //    }
        //    catch (Exception)
        //    {
        //        return "Faild to Update";
        //    }
        //}
        

        //[Route("api/DeleteEmployee")]
        //[HttpDelete]
        //public string deleteEmployee(string ID)
        //{
        //    try
        //    {
        //        var query = @"delete from EmpInfo where ID = '"+ID+@"'";
        //        DataTable dataTable = new DataTable();
        //        using(var con = new SqlConnection(ConfigurationManager.ConnectionStrings["hahaha"].ConnectionString))
        //        using(var cmd = new SqlCommand(query,con))
        //        using (var da = new SqlDataAdapter(cmd))
        //        {
        //            cmd.CommandType = CommandType.Text;
        //            da.Fill(dataTable);
        //        }
        //        return "Deleted Successfully!!!";
        //    }
        //    catch (Exception)
        //    {
        //        return "Failed to Delete";
        //    }
        //}


        //#endregion
    }

}
