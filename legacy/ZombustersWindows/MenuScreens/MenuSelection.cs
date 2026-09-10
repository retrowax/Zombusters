using System;

namespace ZombustersWindows
{
    // Used by the Menu event handlers to get the menu selection.
    public class MenuSelection : EventArgs
    {
        private int m_selection;

        public MenuSelection(int s) {
            m_selection = s;
        }

        public int Selection {
            get { return m_selection; }
            set { m_selection = value; }
        }
    }
}
