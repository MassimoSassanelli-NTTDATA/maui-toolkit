using CommunityToolkit.Mvvm.Messaging;
using Ndbs.MauiToolkit.Auth.Messaging;

namespace Ndbs.MauiToolkit.Auth.Tests.Fakes
{
    /// <summary>Records published <see cref="AuthenticationStateChangedMessage"/> values.</summary>
    internal sealed class AuthStateRecorder : IRecipient<AuthenticationStateChangedMessage>
    {
        public List<AuthenticationState> States { get; } = new();

        public void Receive(AuthenticationStateChangedMessage message) => States.Add(message.Value);
    }
}
