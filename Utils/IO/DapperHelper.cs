using System.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;

using Dapper;
using Dapper.Contrib.Extensions;

namespace Utils.IO
{
    /// <summary>
    /// Dapper 帮助类
    /// </summary>
    public class DapperHelper
    {
        //单例
        public static readonly DapperHelper Instance = new DapperHelper();

        private DapperHelper()
        {

        }

        /// <summary>
        /// 获取数据库连接串
        /// </summary>
        /// <param name="database">数据库文件</param>
        private string GetConnectionString(string database)
        {
            return "Data Source=" + database;
        }

        /// <summary>
        /// 获取实体列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="database">数据库文件</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时</param>
        /// <returns>实体列表</returns>
        public async Task<IEnumerable<T>> GetAllAsync<T>(string database, IDbTransaction transaction = null, int? commandTimeout = null) where T : class, new()
        {
            using (var connection = new SQLiteConnection(GetConnectionString(database)))
            {
                try
                {
                    return await connection.GetAllAsync<T>(transaction, commandTimeout);
                }
                catch (Exception ex)
                {
                    //Logger.Instance.Common.Error($"[ Dapper ] GetAllAsync, Database = {database}, Message = {ex.Message}");
                    return null;
                }
            }
        }

        /// <summary>
        /// 通过 in 方式获取实体列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="database">数据库文件</param>
        /// <param name="table">表名</param>
        /// <param name="column">列名</param>
        /// <param name="values">范围值</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时</param>
        /// <returns>实体列表</returns>
        public async Task<IEnumerable<T>> GetAllByInAsync<T>(string database, string table, string column, string values, IDbTransaction transaction = null, int? commandTimeout = null) where T : class, new()
        {
            using (var connection = new SQLiteConnection(GetConnectionString(database)))
            {
                try
                {
                    return await connection.QueryAsync<T>($"select * from {table} where {column} in @valueArray", new { columnName = column, valueArray = values.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries) }, transaction, commandTimeout);
                }
                catch (Exception ex)
                {
                    //Logger.Instance.Common.Error($"[ Dapper ] GetAllAsync, Database = {database}, Message = {ex.Message}");
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取实体列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="database">数据库文件</param>
        /// <param name="table">表名</param>
        /// <param name="condition">过滤条件</param>
        /// <param name="param">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时</param>
        /// <returns>实体列表</returns>
        public async Task<IEnumerable<T>> GetByCondition<T>(string database, string table, string condition, object param, IDbTransaction transaction = null, int? commandTimeout = null) where T : class, new()
        {
            using (var connection = new SQLiteConnection(GetConnectionString(database)))
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(condition))
                        await connection.GetAllAsync<T>(transaction, commandTimeout);

                    return await connection.QueryAsync<T>($"select * from {table} {condition}", param, transaction, commandTimeout);
                }
                catch (Exception ex)
                {
                   // Logger.Instance.Common.Error($"[ Dapper ] GetByCondition, Database = {database}, condition = {condition}, Message = {ex.Message}");
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取总记录数
        /// </summary>
        /// <param name="database">数据库文件</param>
        /// <param name="table">表名</param>
        /// <param name="condition">条件</param>
        /// <param name="param">参数</param>
        /// <param name="transaction">事务</param>
        /// <param name="commandTimeout">超时</param>
        /// <param name="commandType">类型</param>
        public async Task<int> GetTotalCount(string database, string table, string condition = null, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var connection = new SQLiteConnection(GetConnectionString(database)))
            {
                try
                {
                    return await connection.ExecuteScalarAsync<int>($"select count(*) from {table} {condition}".Trim(), param, transaction, commandTimeout, commandType);
                }
                catch (Exception ex)
                {
                   // Logger.Instance.Common.Error($"[ Dapper ] GetTotalCount, Database = {database}, Message = {ex.Message}");
                    return 0;
                }
            }
        }

        /// <summary>
        /// 删表
        /// </summary>
        /// <param name="database">数据库文件</param>
        /// <param name="table">表名</param>
        public async Task DeleteTable(string database, string table)
        {
            if (!File.Exists(database))
                return;

            using (var connection = new SQLiteConnection(GetConnectionString(database)))
            {
                try
                {
                    await connection.OpenAsync();
                    await connection.ExecuteAsync($"DROP TABLE {table}");
                }
                catch (Exception ex)
                {
                   // Logger.Instance.Common.Error($"[ Dapper ] DeleteTable, Database = {database}, table = {table}, Message = {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 建表
        /// </summary>
        /// <param name="database">数据库文件</param>
        /// <param name="sql">SQL 语句</param>
        public async Task<bool> CreateTable(string database, string sql)
        {
            using (var connection = new SQLiteConnection(GetConnectionString(database)))
            {
                try
                {
                    await connection.OpenAsync();
                    var result = await connection.ExecuteAsync(sql);

                    return result == 0;
                }
                catch (Exception ex)
                {
                   // Logger.Instance.Common.Error($"[ Dapper ] CreateTable, Database = {database}, Message = {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="database">数据库文件</param>
        /// <param name="entities">实体列表</param>
        public async Task<bool> InsertAsync<T>(string database, IEnumerable<T> entities) where T : class, new()
        {
            using (var connection = new SQLiteConnection(GetConnectionString(database)))
            {
                await connection.OpenAsync();
                var dbTran = await connection.BeginTransactionAsync();

                try
                {
                    foreach (var entity in entities)
                    {
                        await connection.InsertAsync(entity, dbTran);
                    }

                    dbTran.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    dbTran.Rollback();

                    //Logger.Instance.Common.Error($"[ Dapper ] InsertAsync, Database = {database}, Message = {ex.Message}");
                    return false;
                }
            }
        }
    }
}
