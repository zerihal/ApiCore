namespace ApiCore.Common
{
    public class EndpointInfo
    {
        /// <summary>
        /// Name of the controller.
        /// </summary>
        public string Controller { get; set; } = string.Empty;

        /// <summary>
        /// Endpoint method.
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Endpoint HTTP method type (e.g. POST, GET, etc).
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;

        /// <summary>
        /// Route (API endpoint name).
        /// </summary>
        public string Route { get; set; } = string.Empty;

        /// <summary>
        /// API method parameters.
        /// </summary>
        public List<string> Parameters { get; set; } = new();

        /// <summary>
        /// Endpoint return type.
        /// </summary>
        public string ReturnType { get; set; } = string.Empty;
    }
}
