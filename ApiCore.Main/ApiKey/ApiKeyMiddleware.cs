using ApiCore.Main.Interfaces;

namespace ApiCore.Main.ApiKey
{
    public sealed class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes API key validation.
        /// </summary>
        /// <param name="context">App context.</param>
        /// <param name="apiKeyStore">Instance of <see cref="IApiKeyStore"/> that holds API keys.</param>
        /// <returns><see cref="Task"/>.</returns>
        public async Task InvokeAsync(HttpContext context, IApiKeyStore apiKeyStore)
        {
            // Check for the presence of the X-API-KEY header
            if (!context.Request.Headers.TryGetValue("X-API-KEY", out var auth))
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("API Key is missing");
                return;
            }

            // Hash the provided API key
            var rawKey = auth.ToString();
            var hashedKey = ApiKeyHasher.HashApiKey(rawKey);

            // Validate the hashed API key
            var validationResult = await apiKeyStore.ValidateAsync(hashedKey);

            // If the key is invalid, return 403 Forbidden
            if (validationResult == null || !validationResult.IsValid)
            {
                context.Response.StatusCode = 403; // Forbidden
                await context.Response.WriteAsync("Invalid API Key");
                return;
            }

            // Store the owner and key type in the HttpContext for later use
            context.Items["ApiKeyOwner"] = validationResult.Owner;
            context.Items["ApiKeyType"] = validationResult.KeyType;

            // If we are here, API key has been validated so continue to next middleware or controller action
            await _next(context);
        }
    }
}
