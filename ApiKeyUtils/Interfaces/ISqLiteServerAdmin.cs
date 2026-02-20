namespace ApiKeyUtils.Interfaces
{
    /// <summary>
    /// Admin methods for managing SQLite databases. Since SQLite is a file-based database, administrative operations are limited.
    /// </summary>
    public interface ISqLiteServerAdmin
    {
        /// <summary>
        /// Deletes the specified SQLite database file from the file system. This operation is irreversible 
        /// and will permanently.
        /// </summary>
        /// <param name="dbPath">Database path.</param>
        /// <returns><see langword="True"/> if database deleted or does not exist, otherwise <see langword="false"/>.</returns>
        public bool DeleteDatabase(string dbPath);
    }
}
