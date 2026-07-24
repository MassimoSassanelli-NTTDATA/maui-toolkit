namespace Ndbs.MauiToolkit.Xia.Initialization
{
    /// <summary>
    /// Holds the micro app that should become active on the next activation of the
    /// micro-app stage. It is the hand-off point for runtime micro-app switching: a
    /// micro-app picker sets <see cref="DesiredMicroApp"/> and then triggers
    /// <c>IContextChain.ActivateFromAsync(XiaContextOrders.MicroApp)</c>.
    /// </summary>
    public sealed class MicroAppSelection
    {
        /// <summary>
        /// Gets or sets the name of the micro app desired for the next activation, or
        /// <see langword="null"/> when none is selected (the stage then defers).
        /// </summary>
        public string? DesiredMicroApp { get; set; }
    }
}
