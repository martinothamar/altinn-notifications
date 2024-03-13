﻿namespace Altinn.Notifications.Core.Shared;

/// <summary>
/// Exception class to hold exceptions when interacting with other Altinn platform REST services
/// </summary>   
public class PlatformHttpException : Exception
{
    /// <summary>
    /// Responsible for holding an http request exception towards platform (storage).
    /// </summary>
    public HttpResponseMessage Response { get; }

    /// <summary>
    /// Copy the response for further investigations
    /// </summary>
    /// <param name="response">the response</param>
    /// <param name="message">A description of the cause of the exception.</param>
    public PlatformHttpException(HttpResponseMessage response, string message) : base(message)
    {
        this.Response = response;
    }

    /// <summary>
    /// Create a new <see cref="PlatformHttpException"/> by reading the <see cref="HttpResponseMessage"/>
    /// content asynchronously.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage"/> to read.</param>
    /// <returns>A new <see cref="PlatformHttpException"/>.</returns>
    public static async Task<PlatformHttpException> CreateAsync(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        string message = $"{(int)response.StatusCode} - {response.ReasonPhrase} - {content}";

        return new PlatformHttpException(response, message);
    }
}
