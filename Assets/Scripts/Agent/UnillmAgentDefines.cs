using System;

namespace unillm
{
    public class OnReceivedMessageEventArgs: EventArgs
    {
        public UnillmMessage Message { get; set; }
    }

    public delegate void OnReceivedMessageEventHandler(IUnillmAgent agent, OnReceivedMessageEventArgs args);
}