namespace ApiKeyUtils.ApiKeyDb
{
    /// <summary>
    /// Database user privileges to be granted.
    /// </summary>
    /// <remarks>
    /// All privileges essentially gives full database administration rights.
    /// </remarks>
    [Flags]
    public enum DbUserPrivileges
    {
        /// <summary>
        /// Select privilege.
        /// </summary>
        SELECT = 1,
        /// <summary>
        /// Insert privilege.
        /// </summary>
        INSERT = 2,
        /// <summary>
        /// Update privilege.
        /// </summary>
        UPDATE = 4,
        /// <summary>
        /// All privileges (admin).
        /// </summary>
        ALL = 8
    }

    /// <summary>
    /// User privileges to be granted when creating a new database user. These are abstracted privileges that 
    /// are translated by helper methods into the appropriate database-specific privilege strings for MySQL and 
    /// SQL Server.
    /// </summary>
    [Flags]
    public enum UserPrivileges
    {
        /// <summary>
        /// None (no privileges granted).
        /// </summary>
        NONE = 0,
        /// <summary>
        /// Read privileges (e.g. SELECT).
        /// </summary>
        READ = 1,
        /// <summary>
        /// Write privileges (e.g. INSERT, UPDATE).
        /// </summary>
        WRITE = 2,
        /// <summary>
        /// Full privileges (admin).
        /// </summary>
        ADMIN = 4
    }
}
