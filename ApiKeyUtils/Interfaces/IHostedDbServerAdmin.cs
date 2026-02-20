using ApiKeyUtils.ApiKeyDb;

namespace ApiKeyUtils.Interfaces
{
    /// <summary>
    /// Provides administrative operations for managing users and databases on a hosted database server.
    /// </summary>
    public interface IHostedDbServerAdmin
    {
        /// <summary>
        /// Gets or sets the administrative database connection used for privileged operations.
        /// </summary>
        public IHostedDbConnection AdminConnection { get; set; }

        /// <summary>
        /// Creates a new database user with the specified username, password, privileges, and optional database scope.
        /// </summary>
        /// <param name="username">New user username.</param>
        /// <param name="password">New user password.</param>
        /// <param name="privileges">User privileges to be granted to the new user.</param>
        /// <param name="database">Database to add user to.</param>
        /// <returns><see langword="True"/> if the user was created, otherwise <see langword="false"/>.</returns>
        public bool CreateUser(string username, string password, UserPrivileges privileges, string? database);

        /// <summary>
        /// Deletes the user account associated with the specified username.
        /// </summary>
        /// <param name="username">The username of the user to delete. Cannot be null or empty.</param>
        /// <returns><see langword="True"/> if the user was deleted, otherwise <see langword="false"/>.</returns>
        public bool DeleteUser(string username);

        /// <summary>
        /// Retrieves a read-only list of user names associated with the specified database.
        /// </summary>
        /// <param name="database">
        /// The name of the database for which to retrieve user names. Cannot be null or empty.
        /// </param>
        /// <returns>
        /// A read-only list of strings containing the user names for the specified database. The list is empty if the
        /// database has no users.
        /// </returns>
        public IReadOnlyList<string> GetDatabaseUsers(string database);

        /// <summary>
        /// Deletes the specified database from the system.
        /// </summary>
        /// <remarks>
        /// If the database does not exist, the method returns false. Deleting a database is irreversible and 
        /// will remove all associated data.
        /// </remarks>
        /// <param name="databaseName">The name of the database to delete. Cannot be null or empty.</param>
        /// <returns><see langword="True"/> if the database was deleted, otherwise <see langword="false"/>.</returns>
        public bool DeleteDatabase(string databaseName);
    }
}
