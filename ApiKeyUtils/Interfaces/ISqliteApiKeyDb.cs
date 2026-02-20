namespace ApiKeyUtils.Interfaces
{
    /// <summary>
    /// SQLite database instance of <see cref="IApiKeyDb"/> that includes a property for the 
    /// directory where the database file is located.
    /// </summary>
    public interface ISqliteApiKeyDb : IApiKeyDb
    {
        /// <summary>
        /// Directory that the database resides in (usually within local host).
        /// </summary>
        public string DatabaseDirectory { get; set; }
    }
}
