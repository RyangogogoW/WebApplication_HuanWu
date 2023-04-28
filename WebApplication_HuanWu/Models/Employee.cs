using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace WebApplication_HuanWu.Models
{
    public class Employee
    {
        public string ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DeptID { get; set; }
    }
}