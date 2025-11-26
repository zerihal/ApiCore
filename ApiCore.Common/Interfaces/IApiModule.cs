namespace ApiCore.Common.Interfaces
{
    public interface IApiModule
    {
        /// <summary>
        /// Gets all endpoint info for this API module.
        /// </summary>
        /// <returns>Array of <see cref="EndpointInfo"/>.</returns>
        public EndpointInfo[] GetApiEndpoints();
    }
}
