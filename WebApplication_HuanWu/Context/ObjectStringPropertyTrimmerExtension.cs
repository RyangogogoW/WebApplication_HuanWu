using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using FastMember;

namespace WebApplication_HuanWu.Context
{
    public static class ObjectStringPropertyTrimmerExtension
    {
        #region Synchronous

        private static void TrimStringReference(ref string s)
        {
            if (string.IsNullOrEmpty(s)) { return; }

            s = s.Trim();
        }

        public static void TrimStringProperties<T>(this T o)
        {
            if (o == null) { return; }

            var type = o.GetType();

            // Trim each object in a collection
            if (typeof(IList).IsAssignableFrom(type) && type != typeof(string))
            {
                var co = o as IList;

                co.TrimCollection();
            }
            else if (typeof(IDictionary).IsAssignableFrom(type) && type != typeof(string))
            {
                var dict = o as IDictionary;

                dict.TrimDictionary();
            }
            else if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type) && type != typeof(string))
            {
                var dyno = o as DynamicObject;
                var expando = o as IDictionary<string, object>;

                if (dyno != null)
                {
                    dyno.TrimDynamic();
                }
                else
                {
                    expando.TrimDynamic();
                }
            }
            else
            {
                o.TrimObject();
            }
        }

        private static void TrimDynamic(this DynamicObject o)
        {
            var properties = o.GetDynamicMemberNames().ToArray();

            var accessor = ObjectAccessor.Create(o);

            foreach (var property in properties)
            {
                var propValue = accessor[property];

                if (propValue is string)
                {
                    var so = propValue as string;

                    TrimStringReference(ref so);

                    propValue = so;
                }
                else
                {
                    propValue.TrimStringProperties();
                }

                accessor[property] = propValue;
            }
        }

        private static void TrimDynamic(this IDictionary<string, object> o)
        {
            var properties = new string[o.Keys.Count];

            o.Keys.CopyTo(properties, 0);

            foreach (var property in properties)
            {
                var propValue = o[property];

                if (propValue is string)
                {
                    var so = propValue as string;

                    TrimStringReference(ref so);

                    propValue = so;
                }
                else
                {
                    propValue.TrimStringProperties();
                }

                o[property] = propValue;
            }
        }

        private static void TrimDictionary(this IDictionary o)
        {
            var properties = new object[o.Keys.Count];

            o.Keys.CopyTo(properties, 0);

            foreach (var property in properties)
            {
                var propValue = o[property];

                if (propValue is string)
                {
                    var so = propValue as string;

                    TrimStringReference(ref so);

                    propValue = so;
                }
                else
                {
                    propValue.TrimStringProperties();
                }

                o[property] = propValue;
            }
        }

        private static void TrimCollection(this IList o)
        {
            for (var i = 0; i < o.Count; i++)
            {
                var obj = o[i];
                if (obj is string)
                {
                    var so = obj as string;

                    TrimStringReference(ref so);

                    o[i] = so;
                }
                else
                {
                    obj.TrimStringProperties();
                }
            }
        }

        private static void TrimObject<T>(this T o)
        {
            var type = o.GetType();

            var accessor = TypeAccessor.Create(type);

            var members = accessor.GetMembers();

            foreach (var member in members)
            {
                if (member.Type.IsValueType
                    || !member.CanWrite
                    || ((ComputedAttribute[])type.GetCustomAttributes<ComputedAttribute>()).Length > 0
                    || ((NotMappedAttribute[])type.GetCustomAttributes<NotMappedAttribute>()).Length > 0)
                {
                    continue;
                }

                var propValue = accessor[o, member.Name];

                if (propValue is string)
                {
                    var strProp = propValue as string;

                    TrimStringReference(ref strProp);

                    accessor[o, member.Name] = strProp;
                }
                else
                {
                    propValue.TrimStringProperties();
                }
            }
        }

        #endregion

        #region Asynchronous

        public static async Task TrimStringPropertiesAsync<T>(this T o)
        {
            if (o == null) { return; }

            var type = o.GetType();

            // Trim each object in a collection
            if (typeof(IList).IsAssignableFrom(type) && type != typeof(string))
            {
                var co = o as IList;

                await co.TrimCollectionAsync();
            }
            else if (typeof(IDictionary).IsAssignableFrom(type) && type != typeof(string))
            {
                var dict = o as IDictionary;

                await dict.TrimDictionaryAsync();
            }
            else if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type) && type != typeof(string))
            {
                var dyno = o as DynamicObject;
                var expando = o as IDictionary<string, object>;

                if (dyno != null)
                {
                    await dyno.TrimDynamicAsync();
                }
                else
                {
                    await expando.TrimDynamicAsync();
                }
            }
            else
            {
                await o.TrimObjectAsync();
            }
        }

        private static async Task TrimDynamicAsync(this DynamicObject o)
        {
            var properties = o.GetDynamicMemberNames().ToArray();

            var accessor = ObjectAccessor.Create(o);

            var trimTasks = properties.Select(async property =>
            {
                var propValue = accessor[property];

                if (propValue is string)
                {
                    var so = propValue as string;

                    TrimStringReference(ref so);

                    propValue = so;
                }
                else
                {
                    await propValue.TrimStringPropertiesAsync();
                }

                accessor[property] = propValue;
            });

            await Task.WhenAll(trimTasks);
        }

        private static async Task TrimDynamicAsync(this IDictionary<string, object> o)
        {
            var properties = new string[o.Keys.Count];

            o.Keys.CopyTo(properties, 0);

            var trimTasks = properties.Select(async property =>
            {
                var propValue = o[property];

                if (propValue is string)
                {
                    var so = propValue as string;

                    TrimStringReference(ref so);

                    propValue = so;
                }
                else
                {
                    await propValue.TrimStringPropertiesAsync();
                }

                o[property] = propValue;
            });

            await Task.WhenAll(trimTasks);
        }

        private static async Task TrimDictionaryAsync(this IDictionary o)
        {
            var properties = new object[o.Keys.Count];

            o.Keys.CopyTo(properties, 0);

            var trimTasks = properties.Select(async property =>
            {
                var propValue = o[property];

                if (propValue is string)
                {
                    var so = propValue as string;

                    TrimStringReference(ref so);

                    propValue = so;
                }
                else
                {
                    await propValue.TrimStringPropertiesAsync();
                }

                o[property] = propValue;
            });

            await Task.WhenAll(trimTasks);
        }

        private static async Task TrimCollectionAsync(this IList o)
        {
            var trimTasks = new List<Task>(o.Count);

            for (var i = 0; i < o.Count; i++)
            {
                var obj = o[i];
                if (obj is string)
                {
                    var task = Task.Run(() =>
                    {
                        var so = obj as string;

                        TrimStringReference(ref so);

                        o[i] = so;
                    });

                    trimTasks.Add(task);
                }
                else
                {
                    trimTasks.Add(obj.TrimStringPropertiesAsync());
                }
            }

            await Task.WhenAll(trimTasks);
        }

        private static async Task TrimObjectAsync<T>(this T o)
        {
            var type = o.GetType();

            var accessor = TypeAccessor.Create(type);

            var members = accessor.GetMembers();

            var trimTasks = members.Select(async member =>
            {
                if (member.Type.IsValueType
                    || !member.CanWrite
                    || ((ComputedAttribute[])type.GetCustomAttributes<ComputedAttribute>()).Length > 0
                    || ((NotMappedAttribute[])type.GetCustomAttributes<NotMappedAttribute>()).Length > 0)
                {
                    return;
                }

                var propValue = accessor[o, member.Name];

                if (propValue is string)
                {
                    var strProp = propValue as string;

                    TrimStringReference(ref strProp);

                    accessor[o, member.Name] = strProp;
                }
                else
                {
                    await propValue.TrimStringPropertiesAsync();
                }
            });

            await Task.WhenAll(trimTasks);
        }

        #endregion
    }
}