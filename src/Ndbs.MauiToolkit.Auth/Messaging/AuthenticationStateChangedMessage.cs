using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Ndbs.MauiToolkit.Auth.Messaging
{
    /// <summary>
    /// Broadcast through the CommunityToolkit <c>IMessenger</c>
    /// (<c>WeakReferenceMessenger</c>) whenever the authentication state changes (A15).
    /// Interested components (such as view models) can subscribe to react — for
    /// example by navigating to the login screen — without holding a direct
    /// reference to the authentication service.
    /// </summary>
    public sealed class AuthenticationStateChangedMessage : ValueChangedMessage<AuthenticationState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationStateChangedMessage"/> class.
        /// </summary>
        /// <param name="value">The new authentication state.</param>
        public AuthenticationStateChangedMessage(AuthenticationState value)
            : base(value)
        {
        }
    }
}
