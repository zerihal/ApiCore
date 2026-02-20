namespace ApiKeyUtils.Interfaces
{
    /// <summary>
    /// Database connection parameter with key-value pair and equality checking.
    /// </summary>
    public interface IDbConnectionParameter
    {
        /// <summary>
        /// Database connection parameter key.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Database connection parameter value.
        /// </summary>
        public object? Value { get; set; }
    }
}
