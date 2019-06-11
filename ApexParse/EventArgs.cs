using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApexParse
{
    class UpdateTickEventArgs : EventArgs
    {
        public TimeSpan ElapsedTime { get; private set; }

        public UpdateTickEventArgs(TimeSpan elapsedTime)
        {
            ElapsedTime = elapsedTime;
        }
    }

    class UserDoubleClickedEventArgs : EventArgs
    {
        public PSO2Player DoubleClickedPlayer { get; private set; }

        public UserDoubleClickedEventArgs(PSO2Player player)
        {
            DoubleClickedPlayer = player;
        }
    }
}
