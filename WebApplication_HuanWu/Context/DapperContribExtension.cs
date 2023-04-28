#region

using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper.Contrib.Extensions;

#endregion

namespace WebApplication_HuanWu.Context
{
    public static class DapperContribExtension
    {
        private static bool IsIdentity(PropertyInfo pi)
        {
            var attributes = pi.GetCustomAttributes(false);
            if (attributes.Length > 0)
            {
                dynamic generated = attributes.FirstOrDefault(x => x.GetType().Name == "DatabaseGeneratedAttribute");
                if (generated != null)
                {
                    return generated.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;
                }
            }
            return false;
        }

        private static bool IsEditable(PropertyInfo pi)
        {
            var attributes = pi.GetCustomAttributes(false);
            if (attributes.Length > 0)
            {
                dynamic write = attributes.FirstOrDefault(x => x.GetType().Name == "EditableAttribute");
                if (write != null)
                {
                    return write.AllowEdit;
                }
            }
            return false;
        }

        private static bool IsComputed(PropertyInfo pi)
        {
            var attributes = pi.GetCustomAttributes(false);
            if (attributes.Length > 0)
            {
                dynamic result = attributes.Any(a => a is ComputedAttribute || a is NotMappedAttribute);
                return result;
            }
            return false;
        }

        private static bool IsSimpleType(this Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            type = underlyingType ?? type;
            var simpleTypes = new List<Type>
                                       {
                                           typeof(byte),
                                           typeof(sbyte),
                                           typeof(short),
                                           typeof(ushort),
                                           typeof(int),
                                           typeof(uint),
                                           typeof(long),
                                           typeof(ulong),
                                           typeof(float),
                                           typeof(double),
                                           typeof(decimal),
                                           typeof(bool),
                                           typeof(string),
                                           typeof(char),
                                           typeof(Guid),
                                           typeof(DateTime),
                                           typeof(DateTimeOffset),
                                           typeof(byte[])
                                       };
            return simpleTypes.Contains(type) || type.IsEnum;
        }

        public static async Task<int> FullInsertAsync<T>(
            this IDbConnection cnn,
            T entity,
            IDbTransaction transaction = null,
            int? commandTimeout = default(int?))
        {
            var tableName = entity.GetType().Name.Split('.').Last();
            var entityType = entity.GetType();
            //  var entityProperties = entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);.
            //  var props = entity.GetType().GetProperties().Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == "EditableAttribute"&& !(attr is ComputedAttribute) && !IsEditable(p)) == false);
            var props = entityType.GetProperties().Where(p => !IsComputed(p) && !IsEditable(p));
            var entityProperties = props.Where(p => p.PropertyType.IsSimpleType()).ToArray();

            var columnMappings = entityProperties.Select(prop => new
            {
                Property = prop.Name,
                Column = ((ColumnAttribute[])prop.GetCustomAttributes(typeof(ColumnAttribute), false)).FirstOrDefault()?.Name ?? prop.Name
            }).ToArray();

            var columnClause = string.Join(",", columnMappings.Select(m => m.Column));
            var valuesClause = string.Join(",", columnMappings.Select(m => $"@{m.Property}"));

            var result =
                await
                    cnn.ExecuteAsync(
                        $"INSERT INTO {tableName} ({columnClause}) values ({valuesClause})",
                        entity,
                        transaction,
                        commandTimeout);

            return result;
        }

        public static async Task<int> IdentityInsertAsync<T>(
            this IDbConnection cnn,
            T entity,
            IDbTransaction transaction = null,
            int? commandTimeout = default(int?))
        {
            var tableName = entity.GetType().Name.Split('.').Last();
            var entityType = entity.GetType();
            // var entityProperties = entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            // var props = entity.GetType().GetProperties().Where(p => p.GetCustomAttributes(true).Any(attr => attr.GetType().Name == "EditableAttribute"&& !(attr is ComputedAttribute) && !IsEditable(p)) == false);
            var props = entityType.GetProperties().Where(p => !IsComputed(p) && !IsEditable(p));
            var entityProperties = props.Where(p => p.PropertyType.IsSimpleType()).ToArray();

            var columnMappings = entityProperties.Select(prop => new
            {
                Property = prop.Name,
                Column = ((ColumnAttribute[])prop.GetCustomAttributes(typeof(ColumnAttribute), false)).FirstOrDefault()?.Name ?? prop.Name
            }).ToArray();

            var columnClause = string.Join(",", columnMappings.Select(m => m.Column));
            var valuesClause = string.Join(",", columnMappings.Select(m => $"@{m.Property}"));

            var result =
                await
                    cnn.QueryAsync<int>(
                        $"INSERT INTO {tableName} ({columnClause}) VALUES ({valuesClause}); SELECT CAST(SCOPE_IDENTITY() as int)",
                        entity,
                        transaction,
                        commandTimeout);

            var val = result.Single();

            return val;
        }

        public static async Task<int> UpdateMappedEntityAsync<T>(this IDbConnection cnn, T entity, IDbTransaction transaction = null, int? commandTimeout = default(int?))
        {
            var tableName = entity.GetType().Name.Split('.').Last();
            var entityType = entity.GetType();

            var keys = entityType.GetProperties().Where(p => !IsComputed(p) && IsIdentity(p));
            var props = entityType.GetProperties().Where(p => !IsComputed(p) && !IsIdentity(p));
            var entityProperties = props.Where(p => p.PropertyType.IsSimpleType()).ToArray();

            var keyMappings = keys.Select(key => new
            {
                Property = key.Name,
                Column = ((ColumnAttribute[])key.GetCustomAttributes(typeof(ColumnAttribute), false)).FirstOrDefault()?.Name ?? key.Name
            });
            var columnMappings = entityProperties.Select(prop => new
            {
                Property = prop.Name,
                Column = ((ColumnAttribute[])prop.GetCustomAttributes(typeof(ColumnAttribute), false)).FirstOrDefault()?.Name ?? prop.Name
            }).ToArray();

            var setClause = string.Join(",", columnMappings.Select(m => $"{m.Column} = @{m.Property}"));
            var whereClause = string.Join(",", keyMappings.Select(m => $"{m.Column} = @{m.Property}"));

            var result =
                await
                    cnn.ExecuteAsync(
                        $"UPDATE {tableName} SET {setClause} WHERE {whereClause}",
                        entity,
                        transaction,
                        commandTimeout);

            return result;
        }
    }
}