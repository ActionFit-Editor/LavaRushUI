using System;
using Project.Scripts.Game.UI;

namespace ActionFit.LavaRush.UI
{
    /// <summary>
    /// Owns one popup-queue slot while the neutral Lava Rush presentation moves between screens.
    /// Product-specific eligibility and navigation stay in the consuming project through callbacks.
    /// </summary>
    public sealed class LavaRushFlowView : ViewController, IPopup
    {
        private Func<bool> _canOpen;
        private Action _openRequested;
        private Action _closed;

        public bool CanOpen => _canOpen?.Invoke() ?? true;

        /// <summary>Installs project-owned eligibility and lifecycle callbacks.</summary>
        public void Configure(Func<bool> canOpen, Action openRequested, Action closed)
        {
            _canOpen = canOpen;
            _openRequested = openRequested;
            _closed = closed;
        }

        /// <summary>Closes the active flow in response to a package presentation action.</summary>
        public void CloseFromPresentation()
        {
            Close(CloseReason.UserButton);
        }

        protected override void OnWillOpen()
        {
            _openRequested?.Invoke();
        }

        protected override void OnClose()
        {
            _closed?.Invoke();
        }

        protected override void OnDestroy()
        {
            _canOpen = null;
            _openRequested = null;
            _closed = null;
            base.OnDestroy();
        }
    }
}
