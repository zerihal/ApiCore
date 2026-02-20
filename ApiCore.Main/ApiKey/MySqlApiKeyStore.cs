using ApiKeyUtils.ApiKeyDb;
using ApiKeyUtils.Interfaces;

namespace ApiCore.Main.ApiKey
{
    public sealed class MySqlApiKeyStore : ApiKeyStoreBase
    {
        /// <inheritdoc/>
        protected override IApiKeyDb ApiKeyDb { get; }

        public MySqlApiKeyStore(IConfiguration config)
        {
            var dbUser = config.GetValue<string>("MySqlServerInstance:UserId");
            var dbPassword = config.GetValue<string>("MySqlServerInstance:Password");

            if (string.IsNullOrEmpty(dbUser) || string.IsNullOrEmpty(dbPassword))
            {
                throw new ArgumentException("MySQL database username or password configuration is missing or empty.");
            }

            // Although not mandatory for localhost, if server, database, or port configuration values are specified, 
            // use these, otherwise rely on default values.

            var dbServer = config.GetValue<string>("MySqlServerInstance:Server");
            var dbName = config.GetValue<string>("MySqlServerInstance:Database");
            var dbPort = config.GetValue<string>("MySqlServerInstance:Port");

            var connection = new MySqlDbConnection(dbUser, dbPassword);

            if (!string.IsNullOrEmpty(dbServer))
                connection.Server = dbServer;

            if (!string.IsNullOrEmpty(dbName))
                connection.Database = dbName;

            if (!string.IsNullOrEmpty(dbPort))
                connection.Port = dbPort;

            ApiKeyDb = ApiKeyDbFactory.CreateHosted(ApiKeyDbType.MySQL, connection);
        }
    }
}
