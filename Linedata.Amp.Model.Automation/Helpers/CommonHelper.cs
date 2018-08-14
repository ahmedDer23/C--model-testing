using System.Threading;

namespace Linedata.Amp.Model.Automation.Helpers
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.IO;
    using Dapper;
    using Microsoft.Extensions.Configuration;

    public static class CommonHelper
    {
        public static void CreateFolderIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static IDbConnection GetConnection()
        {
            try
            {
                var factory = DbProviderFactories.GetFactory(Constants.MsSqlServerProvider);
                IDbConnection dbConnection = factory.CreateConnection();

                if (dbConnection != null)
                {
                    dbConnection.ConnectionString = GetConfig()["LvDbConnection"];
                }

                return dbConnection;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static long GetCurrentUserId()
        {
            var query =
                "select user_id from user_info where domain_username = 'LDS\\" +
                Environment.UserName + "';";

            try
            {
                long currentUserId;
                using (var connection = GetConnection()) {
                    connection.Open();
                    currentUserId = connection.QuerySingleOrDefault<long>(query);
                }

                return currentUserId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static IConfiguration GetConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json",
                    optional: true,
                    reloadOnChange: true);

            return builder.Build();
        }

        public static TResult Execute<TResult>(
            Func<TResult> action,
            TimeSpan retryInterval,
            int retryCount,
            TResult expectedResult
        )
        {
            var result = default(TResult);

            for (var retry = 0; retry < retryCount; retry++) {
                bool succeeded;
                try {
                    if (retry > 0)
                        Thread.Sleep(retryInterval);
                    result = action();

                    succeeded = result != null;
                }
                catch (Exception ex) {
                    throw ex;
                }

                if (succeeded)
                    return result;
            }

            return result;
        }
    }
}
