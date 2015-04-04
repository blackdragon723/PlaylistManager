using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistManager.ApplicationServices
{
    internal static class IDGenerator
    {
        private static int _currentInt = 0;
        private static bool _locked;

        internal static void SetID(int id)
        {
            if (!_locked)
            {
                _currentInt = id;
                _locked = true; // Only allow setting id once
            }
            
        }

        internal static int Next()
        {
            return ++_currentInt;
        }
    }
}
