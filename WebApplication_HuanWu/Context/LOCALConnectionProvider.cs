using WebApplication_HuanWu.Context;

namespace WebApplication_HuanWu.Context
{
    public class LOCALConnectionProvider : DbConnectionProvider
    {
        public static readonly string DbContextName = "hahaha";

        public LOCALConnectionProvider() : base(DbContextName) { }
    }
}