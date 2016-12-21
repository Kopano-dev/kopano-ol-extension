/// Project   :   Kopano OL Extension

/// 
/// Copyright 2016 Kopano b.v.
/// 
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Affero General Public License, version 3,
/// as published by the Free Software Foundation.
/// 
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU Affero General Public License for more details.
/// 
/// You should have received a copy of the GNU Affero General Public License
/// along with this program.If not, see<http://www.gnu.org/licenses/>.
/// 
/// Consult LICENSE file for details

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AcaciaTest.Framework
{
    public static class Util
    {
        /// <summary>
        /// Waits for the function to return a value, by polling.
        /// </summary>
        /// <param name="func">The function. If this returns a value, wait is finished and the value is returned.
        ///                    If null is returned, another attempt will be made, unless the timeout has been reached</param>
        /// <param name="timeout">The timeout, in milliseconds.</param>
        /// <returns>The functions return value, or null if the timeout elapsed</returns>
        public static Type WaitFor<Type>(Func<Type> func, long timeout)
        {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            var result = default(Type);
            while (stopWatch.ElapsedMilliseconds < timeout)
            {
                result = func();
                if (result != null)
                    break;

                // Sleep and try again
                Thread.Sleep(100);
            }

            return result;
        }
    }
}
