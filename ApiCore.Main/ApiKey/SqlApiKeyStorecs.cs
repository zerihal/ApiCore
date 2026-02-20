using ApiKeyUtils.ApiKeyDb;
using ApiKeyUtils.Interfaces;

namespace ApiCore.Main.ApiKey
{
    public sealed class SqlApiKeyStore : ApiKeyStoreBase
    {
        /// <inheritdoc/>
        protected override IHostedApiKeyDb ApiKeyDb { get; }

        public SqlApiKeyStore(IConfiguration config)
        {
            // Constructor implementation (e.g., initialize connection string) goes here.
            var authenticationType = config.GetValue<string>("SqlAuthenticationType:Value");
            var sqlServerInstanceValue = config.GetValue<string>("SqlServerInstance:Server");

            if (sqlServerInstanceValue != null && int.TryParse(authenticationType, out var iAuthType) && 
                Enum.IsDefined(typeof(SqlDbAuthentication), iAuthType))
            {
                var authType = (SqlDbAuthentication)iAuthType;
                IHostedSqlDbConnection sqlDbConnection = new SqlDbConnection(sqlServerInstanceValue, authType);

                if (authType == SqlDbAuthentication.SQLAuthentication)
                {
                    var sqlServerUser = config.GetValue<string>("SqlServerInstance:UserId");
                    var sqlServerPassword = config.GetValue<string>("SqlServerInstance:Password");

                    if (string.IsNullOrEmpty(sqlServerUser) || string.IsNullOrEmpty(sqlServerPassword))
                    {
                        throw new ArgumentException("SQL authentication type is set to SQLAuthentication, but UserId " +
                            "or Password configuration is missing or empty.");
                    }

                    sqlDbConnection.UserCredentials = new Tuple<string, string>(sqlServerUser, sqlServerPassword);
                }

                ApiKeyDb = ApiKeyDbFactory.CreateHosted(ApiKeyDbType.SQL, sqlDbConnection);
            }
            else
            {
                throw new InvalidOperationException("Invalid SQL authentication type configuration.");
            }
        }
    }
}
