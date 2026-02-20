using ApiKeyUtils.Interfaces;

namespace ApiKeyUtils.ApiKeyDb.Extensions
{
    /// <summary>
    /// ApiKeyDb extension methods.
    /// </summary>
    public static class ApiKeyDbExtensionMethods
    {
        /// <summary>
        /// Converts an instance of <see cref="IMySqlHostedDbConnection"/> to <see cref="MySqlDbConnection"/>.
        /// </summary>
        /// <returns>Server connection.</returns>
        public static MySqlDbConnection ToServerConnection(this IMySqlHostedDbConnection dbConnection)
        {
            return MySqlDbConnection.ServerConnection(dbConnection.User, dbConnection.Password, 
                dbConnection.Server, dbConnection.Port);
        }
    }
}
