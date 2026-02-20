using ApiKeyUtils.Interfaces;

namespace ApiKeyUtils.ApiKeyDb
{
    /// <inheritdoc/>
    public class SqLiteServerAdmin : ISqLiteServerAdmin
    {
        /// <inheritdoc/>
        public bool DeleteDatabase(string dbPath)
        {
            if (File.Exists(dbPath))
            {
                try
                {
                    File.Delete(dbPath);
                    return true;
                }
                catch (Exception e)
                {
                    // Log
                    return false;
                }
            }
            else
            {
                return true;
            }
            
        }
    }
}
