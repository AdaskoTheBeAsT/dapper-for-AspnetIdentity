﻿namespace Lu.Dapper.Extensions.Utils
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Adapters;

    using global::Dapper;

    using DataAnnotations;

    public static partial class SqlMapperExtensions
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> KeyProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypeProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> ComputedProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> AutoIncrementProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>(); 
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> GetQueries = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> TypeTableName = new ConcurrentDictionary<RuntimeTypeHandle, string>();
        private static readonly Dictionary<string, ISqlAdapter> AdapterDictionary =
            new Dictionary<string, ISqlAdapter>()
                {
                    { "sqlconnection", new SqlServerAdapter() },
                    { "sqlceconnection", new SqlCeAdapter() },
                    { "npgsqlconnection", new PostgresAdapter() },
                    { "sqliteconnection", new SQLiteAdapter() },
                    { "mysqlconnection", new MySqlAdapter() }
                };

        public interface IProxy
        {
            bool IsDirty { get; set; }
        }

        public static bool IsWriteable(PropertyInfo pi)
        {
            var attributes = pi.GetCustomAttributes<WriteAttribute>(false).ToList();
            if (attributes.Count == 1)
            {
                return attributes[0].Write;
            }

            return true;
        }

        /// <summary>
        /// Returns a single entity by a single id from table "Ts". T must be of interface type. 
        /// Id must be marked with [Key] attribute.
        /// Created entity is tracked/intercepted for changes and used by the Update() extension. 
        /// </summary>
        /// <typeparam name="T">Interface type to create and populate</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="id">Id of the entity to get, must be marked with [Key] attribute</param>
        /// <returns>Entity of T</returns>
        public static T Get<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var type = typeof(T);
            string sql;
            if (!GetQueries.TryGetValue(type.TypeHandle, out sql))
            { 
                var keys = KeyPropertiesCache(type);

                var count = keys.Count();

                if (count > 1)
                {
                    throw new DataException("Get<T> only supports an entity with a single [Key] property");
                }

                if (count == 0)
                {
                    throw new DataException("Get<T> only supports an entity with a [Key] property");
                }

                var onlyKey = keys.First();

                var name = GetTableName(type);

                // TODO: pluralizer 
                // TODO: query information schema and only select fields that are both in information schema and underlying class / interface 
                sql = "select * from " + name + " where " + onlyKey.Name + " = @id";
                GetQueries[type.TypeHandle] = sql;
            }
           
            var dynParms = new DynamicParameters();
            dynParms.Add("@id", id);

            T obj = null;

            if (type.IsInterface)
            {
                var res = connection.Query(sql, dynParms).FirstOrDefault() as IDictionary<string, object>;

                if (res == null)
                {
                    return (T)((object)null);
                }

                obj = ProxyGenerator.GetInterfaceProxy<T>();

                foreach (var property in TypePropertiesCache(type))
                {
                    var val = res[property.Name];
                    property.SetValue(obj, val, null);
                }

                // reset change tracking and return
                ((IProxy)obj).IsDirty = false;
            }
            else
            {
                obj = connection.Query<T>(sql, dynParms, transaction: transaction, commandTimeout: commandTimeout).FirstOrDefault();
            }

            return obj;
        }

        /// <summary>
        /// Returns a single entity by a single id from table "Ts" asynchronously using .NET 4.5 Task. T must be of interface type. 
        /// Id must be marked with [Key] attribute.
        /// Created entity is tracked/intercepted for changes and used by the Update() extension. 
        /// </summary>
        /// <typeparam name="T">Interface type to create and populate</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="id">Id of the entity to get, must be marked with [Key] attribute</param>
        /// <returns>Entity of T</returns>
        public static async Task<T> GetAsync<T>(this IDbConnection connection, dynamic id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var type = typeof(T);
            string sql;
            if (!GetQueries.TryGetValue(type.TypeHandle, out sql))
            {
                var keys = KeyPropertiesCache(type);
                var count = keys.Count();
                if (count > 1)
                {
                    throw new DataException("Get<T> only supports an entity with a single [Key] property");
                }

                if (count == 0)
                {
                    throw new DataException("Get<T> only supports an entity with a [Key] property");
                }

                var onlyKey = keys.First();

                var name = GetTableName(type);

                // TODO: pluralizer 
                // TODO: query information schema and only select fields that are both in information schema and underlying class / interface 
                sql = "SELECT * FROM " + name + " WHERE " + onlyKey.Name + " = @id";
                GetQueries[type.TypeHandle] = sql;
            }

            var dynParms = new DynamicParameters();
            dynParms.Add("@id", id);

            T obj = null;

            if (type.IsInterface)
            {
                var res = (await connection.QueryAsync<dynamic>(sql, dynParms).ConfigureAwait(false)).FirstOrDefault() as IDictionary<string, object>;

                if (res == null)
                {
                    return (T)((object)null);
                }

                obj = ProxyGenerator.GetInterfaceProxy<T>();

                foreach (var property in TypePropertiesCache(type))
                {
                    var val = res[property.Name];
                    property.SetValue(obj, val, null);
                }

                // reset change tracking and return
                ((IProxy)obj).IsDirty = false;
            }
            else
            {
                obj = (await connection.QueryAsync<T>(sql, dynParms, transaction: transaction, commandTimeout: commandTimeout).ConfigureAwait(false)).FirstOrDefault();
            }

            return obj;
        }
        
        /// <summary>
        /// Inserts an entity into table "Ts" and returns identity id.
        /// </summary>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToInsert">Entity to insert</param>
        /// <returns>Identity of inserted entity</returns>
        public static long Insert<T>(this IDbConnection connection, T entityToInsert, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var type = typeof(T);

            var name = GetTableName(type);

            var sbColumnList = new StringBuilder(null);

            var allProperties = TypePropertiesCache(type);
            var keyProperties = KeyPropertiesCache(type);
            var computedProperties = ComputedPropertiesCache(type);

            var allPropertiesExceptKeyAndComputed = allProperties.Except(computedProperties);

            var autoIncrementProperties = AutoIncrementPropertiesCache(type);
            var hasAutoIncrement = false;
            if (autoIncrementProperties.Any() && keyProperties.Any())
            {
                hasAutoIncrement = keyProperties.Contains(autoIncrementProperties.First());
            }

            if (hasAutoIncrement)
            {
                allPropertiesExceptKeyAndComputed = allProperties.Except(keyProperties.Union(computedProperties));
            }

            for (var i = 0; i < allPropertiesExceptKeyAndComputed.Count(); i++)
            {
                var property = allPropertiesExceptKeyAndComputed.ElementAt(i);
                sbColumnList.AppendFormat("{0}", property.Name);
                if (i < allPropertiesExceptKeyAndComputed.Count() - 1)
                {
                    sbColumnList.Append(", ");
                }
            }

            var sbParameterList = new StringBuilder(null);
            for (var i = 0; i < allPropertiesExceptKeyAndComputed.Count(); i++)
            {
                var property = allPropertiesExceptKeyAndComputed.ElementAt(i);
                sbParameterList.AppendFormat("@{0}", property.Name);
                if (i < allPropertiesExceptKeyAndComputed.Count() - 1)
                {
                    sbParameterList.Append(", ");
                }
            }

            var adapter = GetFormatter(connection);
            var id = adapter.Insert(
                connection,
                transaction,
                commandTimeout,
                name,
                sbColumnList.ToString(),
                sbParameterList.ToString(),
                keyProperties,
                entityToInsert,
                hasAutoIncrement);
            return id;
        }

        /// <summary>
        /// Inserts an entity into table "Ts" asynchronously using .NET 4.5 Task and returns identity id.
        /// </summary>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToInsert">Entity to insert</param>
        /// <returns>Identity of inserted entity</returns>
        public static Task<int> InsertAsync<T>(
            this IDbConnection connection,
            T entityToInsert,
            IDbTransaction transaction = null,
            int? commandTimeout = null) where T : class
        {
            var type = typeof(T);

            var name = GetTableName(type);

            var sbColumnList = new StringBuilder(null);

            var allProperties = TypePropertiesCache(type);
            var keyProperties = KeyPropertiesCache(type);
            var computedProperties = ComputedPropertiesCache(type);
            var allPropertiesExceptKeyAndComputed = allProperties.Except(computedProperties);
            var autoIncrementProperties = AutoIncrementPropertiesCache(type);
            var hasAutoIncrement = false;
            if (autoIncrementProperties.Any() && keyProperties.Any())
            {
                hasAutoIncrement = keyProperties.Contains(autoIncrementProperties.First());
            }

            if (hasAutoIncrement)
            {
                allPropertiesExceptKeyAndComputed = allProperties.Except(keyProperties.Union(computedProperties));
            }

            for (var i = 0; i < allPropertiesExceptKeyAndComputed.Count(); i++)
            {
                var property = allPropertiesExceptKeyAndComputed.ElementAt(i);
                sbColumnList.AppendFormat("{0}", property.Name);
                if (i < allPropertiesExceptKeyAndComputed.Count() - 1)
                {
                    sbColumnList.Append(", ");
                }
            }

            var sbParameterList = new StringBuilder(null);
            for (var i = 0; i < allPropertiesExceptKeyAndComputed.Count(); i++)
            {
                var property = allPropertiesExceptKeyAndComputed.ElementAt(i);
                sbParameterList.AppendFormat("@{0}", property.Name);
                if (i < allPropertiesExceptKeyAndComputed.Count() - 1)
                {
                    sbParameterList.Append(", ");
                }
            }

            var adapter = GetFormatter(connection);
            return adapter.InsertAsync(
                connection,
                transaction,
                commandTimeout,
                name,
                sbColumnList.ToString(),
                sbParameterList.ToString(),
                keyProperties,
                entityToInsert,
                hasAutoIncrement);
        }

        /// <summary>
        /// Updates entity in table "Ts", checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <typeparam name="T">Type to be updated</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToUpdate">Entity to be updated</param>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        public static bool Update<T>(this IDbConnection connection, T entityToUpdate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var proxy = entityToUpdate as IProxy;
            if (proxy != null && !proxy.IsDirty)
            {
                return false;
            }

            var type = typeof(T);

            var keyProperties = KeyPropertiesCache(type);
            if (!keyProperties.Any())
            {
                throw new ArgumentException("Entity must have at least one [Key] property");
            }

            var name = GetTableName(type);

            var sb = new StringBuilder();
            sb.AppendFormat("UPDATE {0} SET ", name);

            var allProperties = TypePropertiesCache(type);
            var computedProperties = ComputedPropertiesCache(type);
            var nonIdProps = allProperties.Except(keyProperties.Union(computedProperties));

            for (var i = 0; i < nonIdProps.Count(); i++)
            {
                var property = nonIdProps.ElementAt(i);
                sb.AppendFormat("{0} = @{1}", property.Name, property.Name);
                if (i < nonIdProps.Count() - 1)
                {
                    sb.AppendFormat(", ");
                }
            }

            sb.Append(" WHERE ");
            for (var i = 0; i < keyProperties.Count(); i++)
            {
                var property = keyProperties.ElementAt(i);
                sb.AppendFormat("{0} = @{1}", property.Name, property.Name);
                if (i < keyProperties.Count() - 1)
                {
                    sb.AppendFormat(" AND ");
                }
            }

            var updated = connection.Execute(sb.ToString(), entityToUpdate, commandTimeout: commandTimeout, transaction: transaction);
            return updated > 0;
        }

        /// <summary>
        /// Updates entity in table "Ts" asynchronously using .NET 4.5 Task, checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <typeparam name="T">Type to be updated</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToUpdate">Entity to be updated</param>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        public static async Task<bool> UpdateAsync<T>(this IDbConnection connection, T entityToUpdate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var proxy = entityToUpdate as IProxy;
            if (proxy != null && !proxy.IsDirty)
            {
                return false;
            }

            var type = typeof(T);

            var keyProperties = KeyPropertiesCache(type);
            if (!keyProperties.Any())
            {
                throw new ArgumentException("Entity must have at least one [Key] property");
            }

            var name = GetTableName(type);

            var sb = new StringBuilder();
            sb.AppendFormat("UPDATE {0} SET ", name);

            var allProperties = TypePropertiesCache(type);
            var computedProperties = ComputedPropertiesCache(type);
            var nonIdProps = allProperties.Except(keyProperties.Union(computedProperties));

            for (var i = 0; i < nonIdProps.Count(); i++)
            {
                var property = nonIdProps.ElementAt(i);
                sb.AppendFormat("{0} = @{1}", property.Name, property.Name);
                if (i < nonIdProps.Count() - 1)
                {
                    sb.AppendFormat(", ");
                }
            }

            sb.Append(" WHERE ");
            for (var i = 0; i < keyProperties.Count(); i++)
            {
                var property = keyProperties.ElementAt(i);
                sb.AppendFormat("{0} = @{1}", property.Name, property.Name);
                if (i < keyProperties.Count() - 1)
                {
                    sb.AppendFormat(" AND ");
                }
            }

            var updated = await connection.ExecuteAsync(sb.ToString(), entityToUpdate, commandTimeout: commandTimeout, transaction: transaction).ConfigureAwait(false);
            return updated > 0;
        }

        /// <summary>
        /// Delete entity in table "Ts".
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToDelete">Entity to delete</param>
        /// <returns>true if deleted, false if not found</returns>
        public static bool Delete<T>(this IDbConnection connection, T entityToDelete, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            if (entityToDelete == null)
            {
                throw new ArgumentException("Cannot Delete null Object", "entityToDelete");
            }

            var type = typeof(T);

            var keyProperties = KeyPropertiesCache(type).ToList();
            if (keyProperties.Count == 0)
            {
                throw new ArgumentException("Entity must have at least one [Key] property");
            }

            var name = GetTableName(type);

            var sb = new StringBuilder();
            sb.AppendFormat("DELETE FROM {0} WHERE ", name);

            for (var i = 0; i < keyProperties.Count(); i++)
            {
                var property = keyProperties.ElementAt(i);
                sb.AppendFormat("{0} = @{1}", property.Name, property.Name);
                if (i < keyProperties.Count() - 1)
                {
                    sb.AppendFormat(" AND ");
                }
            }

            var deleted = connection.Execute(sb.ToString(), entityToDelete, transaction: transaction, commandTimeout: commandTimeout);
            return deleted > 0;
        }

        /// <summary>
        /// Delete entity in table "Ts" asynchronously using .NET 4.5 Task.
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <param name="entityToDelete">Entity to delete</param>
        /// <returns>true if deleted, false if not found</returns>
        public static async Task<bool> DeleteAsync<T>(this IDbConnection connection, T entityToDelete, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            if (entityToDelete == null)
            {
                throw new ArgumentException("Cannot Delete null Object", "entityToDelete");
            }

            var type = typeof(T);

            var keyProperties = KeyPropertiesCache(type).ToList();
            if (keyProperties.Count == 0)
            {
                throw new ArgumentException("Entity must have at least one [Key] property");
            }

            var name = GetTableName(type);

            var sb = new StringBuilder();
            sb.AppendFormat("DELETE FROM {0} WHERE ", name);

            for (var i = 0; i < keyProperties.Count; i++)
            {
                var property = keyProperties.ElementAt(i);
                sb.AppendFormat("{0} = @{1}", property.Name, property.Name);
                if (i < keyProperties.Count() - 1)
                {
                    sb.AppendFormat(" AND ");
                }
            }

            var deleted = await connection.ExecuteAsync(sb.ToString(), entityToDelete, transaction: transaction, commandTimeout: commandTimeout).ConfigureAwait(false);
            return deleted > 0;
        }

        /// <summary>
        /// Delete all entities in the table related to the type T.
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <returns>true if deleted, false if none found</returns>
        public static bool DeleteAll<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var type = typeof(T);
            var name = GetTableName(type);
            var statement = string.Format("DELETE FROM {0}", name);
            var deleted = connection.Execute(statement, null, transaction: transaction, commandTimeout: commandTimeout);
            return deleted > 0;
        }

        /// <summary>
        /// Delete all entities in the table related to the type T asynchronously using .NET 4.5 Task.
        /// </summary>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <param name="connection">Open SqlConnection</param>
        /// <returns>true if deleted, false if none found</returns>
        public static async Task<bool> DeleteAllAsync<T>(this IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null) where T : class
        {
            var type = typeof(T);
            var name = GetTableName(type);
            var statement = string.Format("DELETE FROM {0}", name);
            var deleted = await connection.ExecuteAsync(statement, null, transaction: transaction, commandTimeout: commandTimeout).ConfigureAwait(false);
            return deleted > 0;
        }

        public static ISqlAdapter GetFormatter(IDbConnection connection)
        {
            var name = connection.GetType().Name.ToLower();
            if (!AdapterDictionary.ContainsKey(name))
            {
                return new SqlServerAdapter();
            }

            return AdapterDictionary[name];
        }

        private static IEnumerable<PropertyInfo> ComputedPropertiesCache(Type type)
        {
            IEnumerable<PropertyInfo> pi;
            if (ComputedProperties.TryGetValue(type.TypeHandle, out pi))
            {
                return pi;
            }

            var typeComputedProperties = type.GetCustomAttributes<ComputedAttribute>().Where(
                a =>
                    {
                        if (string.IsNullOrEmpty(a.Name)
                            || type.GetProperty(a.Name) == null)
                        {
                            return false;
                        }

                        return true;

                    }).Select(a => type.GetProperty(a.Name));

            var properties = TypePropertiesCache(type).Where(p => p.GetCustomAttributes(true).Any(a => a is ComputedAttribute)).ToList();

            var computedProperties = properties.Union(typeComputedProperties).Distinct().ToList();

            ComputedProperties[type.TypeHandle] = computedProperties;
            return computedProperties;
        }

        private static IEnumerable<PropertyInfo> KeyPropertiesCache(Type type)
        {
            IEnumerable<PropertyInfo> pi;
            if (KeyProperties.TryGetValue(type.TypeHandle, out pi))
            {
                return pi;
            }

            var typeKeyProperties = type.GetCustomAttributes<KeyAttribute>().Where(
                a =>
                    {
                        if (string.IsNullOrEmpty(a.Name) || type.GetProperty(a.Name) == null)
                        {
                            return false;
                        }

                        return true;

                    }).Select(a => type.GetProperty(a.Name));

            var allProperties = TypePropertiesCache(type);
            var properties = allProperties.Where(p => p.GetCustomAttributes(true).Any(a => a is KeyAttribute)).ToList();

            var keyProperties = properties.Union(typeKeyProperties).Distinct().ToList();

            if (keyProperties.Count == 0)
            {
                var idProp = allProperties.FirstOrDefault(p => p.Name.ToLower() == "id");
                if (idProp != null)
                {
                    keyProperties.Add(idProp);
                }
            }

            KeyProperties[type.TypeHandle] = keyProperties;
            return keyProperties;
        }

        private static IEnumerable<PropertyInfo> TypePropertiesCache(Type type)
        {
            IEnumerable<PropertyInfo> pis;
            if (TypeProperties.TryGetValue(type.TypeHandle, out pis))
            {
                return pis;
            }

            var properties = type.GetProperties().Where(IsWriteable).ToArray();
            TypeProperties[type.TypeHandle] = properties;
            return properties;
        }

        private static IEnumerable<PropertyInfo> AutoIncrementPropertiesCache(Type type)
        {
            IEnumerable<PropertyInfo> pi;
            if (AutoIncrementProperties.TryGetValue(type.TypeHandle, out pi))
            {
                return pi;
            }

            var typeAutoIncrementProperties = type.GetCustomAttributes<AutoIncrementAttribute>().Where(
                a =>
                    {
                        if (string.IsNullOrEmpty(a.Name) || type.GetProperty(a.Name) == null)
                        {
                            return false;
                        }

                        return true;

                    }).Select(a => type.GetProperty(a.Name));

            var properties = TypePropertiesCache(type).Where(p => p.GetCustomAttributes(true).Any(a => a is AutoIncrementAttribute)).ToList();

            var autoIncrementProperties = properties.Union(typeAutoIncrementProperties).Distinct().ToList();

            AutoIncrementProperties[type.TypeHandle] = autoIncrementProperties;
            return autoIncrementProperties;
        }

        private static string GetTableName(Type type)
        {
            string name;
            if (!TypeTableName.TryGetValue(type.TypeHandle, out name))
            {
                name = type.Name.Contains("`") ? type.Name.Substring(0, type.Name.IndexOf("`", StringComparison.Ordinal)) : type.Name;
                name = name + "s";
                if (type.IsInterface && name.StartsWith("I", StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(1);
                }

                // NOTE: This as dynamic trick should be able to handle both our own Table-attribute as well as the one in EntityFramework 
                var tableattr =
                    type.GetCustomAttributes(false).SingleOrDefault(attr => attr.GetType().Name == "TableAttribute") as
                    dynamic;
                if (tableattr != null)
                {
                    name = tableattr.Name;
                }

                TypeTableName[type.TypeHandle] = name;
            }

            return name;
        }

        private static class ProxyGenerator
        {
            private static readonly Dictionary<Type, object> TypeCache = new Dictionary<Type, object>();

            public static T GetClassProxy<T>()
            {
                // A class proxy could be implemented if all properties are virtual
                // otherwise there is a pretty dangerous case where internal actions will not update dirty tracking
                throw new NotImplementedException();
            }

            public static T GetInterfaceProxy<T>()
            {
                var typeOfT = typeof(T);

                object k;
                if (TypeCache.TryGetValue(typeOfT, out k))
                {
                    return (T)k;
                }

                var assemblyBuilder = GetAsmBuilder(typeOfT.Name);

                // NOTE: to save, add "asdasd.dll" parameter
                var moduleBuilder = assemblyBuilder.DefineDynamicModule("SqlMapperExtensions." + typeOfT.Name);

                var interfaceType = typeof(SqlMapperExtensions.IProxy);
                var typeBuilder = moduleBuilder.DefineType(
                    typeOfT.Name + "_" + Guid.NewGuid(),
                    TypeAttributes.Public | TypeAttributes.Class);
                typeBuilder.AddInterfaceImplementation(typeOfT);
                typeBuilder.AddInterfaceImplementation(interfaceType);

                // create our _isDirty field, which implements IProxy
                var setIsDirtyMethod = CreateIsDirtyProperty(typeBuilder);

                // Generate a field for each property, which implements the T
                foreach (var property in typeof(T).GetProperties())
                {
                    var isId = property.GetCustomAttributes(true).Any(a => a is KeyAttribute);
                    CreateProperty<T>(typeBuilder, property.Name, property.PropertyType, setIsDirtyMethod, isId);
                }

                var generatedType = typeBuilder.CreateType();

                //// assemblyBuilder.Save(name + ".dll");  //NOTE: to save, uncomment

                var generatedObject = Activator.CreateInstance(generatedType);

                TypeCache.Add(typeOfT, generatedObject);
                return (T)generatedObject;
            }

            private static AssemblyBuilder GetAsmBuilder(string name)
            {
                var assemblyBuilder = Thread.GetDomain()
                    .DefineDynamicAssembly(new AssemblyName { Name = name }, AssemblyBuilderAccess.Run);

                // NOTE: to save, use RunAndSave
                return assemblyBuilder;
            }

            private static MethodInfo CreateIsDirtyProperty(TypeBuilder typeBuilder)
            {
                var propType = typeof(bool);
                var field = typeBuilder.DefineField("_" + "IsDirty", propType, FieldAttributes.Private);
                var property = typeBuilder.DefineProperty(
                    "IsDirty",
                    System.Reflection.PropertyAttributes.None,
                    propType,
                    new Type[] { propType });

                const MethodAttributes getSetAttr =
                    MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.SpecialName
                    | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig;

                // Define the "get" and "set" accessor methods
                var currGetPropMthdBldr = typeBuilder.DefineMethod(
                    "get_" + "IsDirty",
                    getSetAttr,
                    propType,
                    Type.EmptyTypes);

                var currGetIL = currGetPropMthdBldr.GetILGenerator();
                currGetIL.Emit(OpCodes.Ldarg_0);
                currGetIL.Emit(OpCodes.Ldfld, field);
                currGetIL.Emit(OpCodes.Ret);
                var currSetPropMthdBldr = typeBuilder.DefineMethod(
                    "set_" + "IsDirty",
                    getSetAttr,
                    null,
                    new Type[] { propType });

                var currSetIL = currSetPropMthdBldr.GetILGenerator();
                currSetIL.Emit(OpCodes.Ldarg_0);
                currSetIL.Emit(OpCodes.Ldarg_1);
                currSetIL.Emit(OpCodes.Stfld, field);
                currSetIL.Emit(OpCodes.Ret);

                property.SetGetMethod(currGetPropMthdBldr);
                property.SetSetMethod(currSetPropMthdBldr);
                var getMethod = typeof(SqlMapperExtensions.IProxy).GetMethod("get_" + "IsDirty");
                var setMethod = typeof(SqlMapperExtensions.IProxy).GetMethod("set_" + "IsDirty");
                typeBuilder.DefineMethodOverride(currGetPropMthdBldr, getMethod);
                typeBuilder.DefineMethodOverride(currSetPropMthdBldr, setMethod);

                return currSetPropMthdBldr;
            }

            private static void CreateProperty<T>(TypeBuilder typeBuilder, string propertyName, Type propType, MethodInfo setIsDirtyMethod, bool isIdentity)
            {
                // Define the field and the property 
                var field = typeBuilder.DefineField("_" + propertyName, propType, FieldAttributes.Private);
                var property = typeBuilder.DefineProperty(
                    propertyName,
                    System.Reflection.PropertyAttributes.None,
                    propType,
                    new Type[] { propType });

                const MethodAttributes getSetAttr =
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;

                // Define the "get" and "set" accessor methods
                var currGetPropMthdBldr = typeBuilder.DefineMethod(
                    "get_" + propertyName,
                    getSetAttr,
                    propType,
                    Type.EmptyTypes);

                var currGetIL = currGetPropMthdBldr.GetILGenerator();
                currGetIL.Emit(OpCodes.Ldarg_0);
                currGetIL.Emit(OpCodes.Ldfld, field);
                currGetIL.Emit(OpCodes.Ret);

                var currSetPropMthdBldr = typeBuilder.DefineMethod(
                    "set_" + propertyName,
                    getSetAttr,
                    null,
                    new Type[] { propType });

                // store value in private field and set the isdirty flag
                var currSetIL = currSetPropMthdBldr.GetILGenerator();
                currSetIL.Emit(OpCodes.Ldarg_0);
                currSetIL.Emit(OpCodes.Ldarg_1);
                currSetIL.Emit(OpCodes.Stfld, field);
                currSetIL.Emit(OpCodes.Ldarg_0);
                currSetIL.Emit(OpCodes.Ldc_I4_1);
                currSetIL.Emit(OpCodes.Call, setIsDirtyMethod);
                currSetIL.Emit(OpCodes.Ret);

                // TODO: Should copy all attributes defined by the interface?
                if (isIdentity)
                {
                    var keyAttribute = typeof(KeyAttribute);
                    var myConstructorInfo = keyAttribute.GetConstructor(new Type[] { });
                    var attributeBuilder = new CustomAttributeBuilder(myConstructorInfo, new object[] { });
                    property.SetCustomAttribute(attributeBuilder);
                }

                property.SetGetMethod(currGetPropMthdBldr);
                property.SetSetMethod(currSetPropMthdBldr);
                var getMethod = typeof(T).GetMethod("get_" + propertyName);
                var setMethod = typeof(T).GetMethod("set_" + propertyName);
                typeBuilder.DefineMethodOverride(currGetPropMthdBldr, getMethod);
                typeBuilder.DefineMethodOverride(currSetPropMthdBldr, setMethod);
            }
        }
    }
}