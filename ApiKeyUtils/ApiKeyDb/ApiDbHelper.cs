using System.Security.Cryptography;
using System.Text;

namespace ApiKeyUtils.ApiKeyDb
{
    /// <summary>
    /// API DB helper methods.
    /// </summary>
    public static class ApiDbHelper
    {
        private const string WindowsDataDir = @"C:\ProgramData\ApiCore\Data";
        private const string LinuxDataDir = "/var/lib/apicore/data";

        /// <summary>
        /// Generates a secure random API key string. The generated API key is a 32-byte random value 
        /// encoded as a Base64 string, providing a high level of entropy and uniqueness suitable for 
        /// use as an API key.
        /// </summary>
        /// <returns>New API key as string.</returns>
        public static string GenerateApiKey()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }

        /// <summary>
        /// Gets the SHA-256 hash of the provided API key as a Base64 string.
        /// </summary>
        /// <param name="apiKey">Raw API key.</param>
        /// <returns>Hash of the API key.</returns>
        public static string GetKeyHash(string apiKey)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(apiKey);
                var hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Gets the data directory path for storing the database (SQLite only).
        /// </summary>
        /// <returns>Default SQLite database directory.</returns>
        internal static string GetDefaultDataDirectory()
        {
            if (OperatingSystem.IsWindows())
            {
                Directory.CreateDirectory(WindowsDataDir);
                return WindowsDataDir;
            }
            else if (OperatingSystem.IsLinux())
            {
                Directory.CreateDirectory(LinuxDataDir);
                return LinuxDataDir;
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported operating system.");
            }
        }

        /// <summary>
        /// Gets the MySQL privileges string based on the provided <see cref="UserPrivileges"/> flags. 
        /// This method translates the specified privileges into a comma-separated string of MySQL privilege 
        /// keywords, which can be used in SQL statements to grant the appropriate permissions to a database 
        /// user. If the ADMIN flag is set, it returns "ALL PRIVILEGES".
        /// </summary>
        /// <param name="privileges">User privileges.</param>
        /// <returns>MySql user privileges string to add to MySql query.</returns>
        /// <exception cref="ArgumentException">Thrown if no privileges passed in.</exception>
        internal static string GetMySqlPrivilegesString(UserPrivileges privileges)
        {
            if (privileges == UserPrivileges.NONE)
                throw new ArgumentException("No privileges specified.");

            if (privileges.HasFlag(UserPrivileges.ADMIN))
                return "ALL PRIVILEGES";

            var parts = new List<string>();

            if (privileges.HasFlag(UserPrivileges.READ))
                parts.Add("SELECT");

            if (privileges.HasFlag(UserPrivileges.WRITE))
            {
                parts.Add("INSERT");
                parts.Add("UPDATE");
            }

            return string.Join(", ", parts);
        }

        /// <summary>
        /// Gets the SQL Server privileges string based on the provided <see cref="UserPrivileges"/> flags.
        /// </summary>
        /// <param name="privileges">User privileges.</param>
        /// <param name="username">Database username.</param>
        /// <returns>SQL user privileges string to add to SQL query.</returns>
        /// <exception cref="ArgumentException">Thrown if no privileges passed in.</exception>
        internal static string GetSqlPrivilegesString(UserPrivileges privileges, string username)
        {
            if (privileges == UserPrivileges.NONE)
                throw new ArgumentException("No privileges specified.");

            var rolesSb = new StringBuilder();

            if (privileges.HasFlag(UserPrivileges.ADMIN))
            {
                rolesSb.AppendLine($@"ALTER ROLE db_owner ADD MEMBER {username};");
            }
            else
            {
                if (privileges.HasFlag(UserPrivileges.READ))
                    rolesSb.AppendLine($@"ALTER ROLE db_datareader ADD MEMBER {username};");

                if (privileges.HasFlag(UserPrivileges.WRITE))
                    rolesSb.AppendLine($@"ALTER ROLE db_datawriter ADD MEMBER {username};");
            }

            return rolesSb.ToString();
        }
    }
}
