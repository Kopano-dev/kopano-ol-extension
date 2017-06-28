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

using Acacia.ZPush;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Acacia.Utils
{
    public static class Util
    {
        public static bool NullSafeEquals<ObjType>(this ObjType a, ObjType b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        #region Enumeration helpers

        /// <summary>
        /// Extension for a Com enumeration. Disposes the enumerated object and - optionally - the returned elements.
        /// </summary>
        /// <param name="source">The object to be enumerated. This will be released.</param>
        /// <param name="releaseElements">If true (the default), elements will also be released.</param>
        /// <returns></returns>
        public static IEnumerable<T> ComEnum<T>(this IEnumerable<T> source, bool releaseElements = true)
        {
            foreach (T item in source)
            {
                try
                {
                    yield return item;
                }
                finally
                {
                    if (releaseElements)
                        ComRelease.Release(item);
                }
            }
            ComRelease.Release(source);
        }

        /// <summary>
        /// Extension for a Com enumeration. Disposes the enumerated object and - optionally - the returned elements.
        /// </summary>
        /// <param name="source">The object to be enumerated. This will be released.</param>
        /// <param name="releaseElements">If true (the default), elements will also be released.</param>
        /// <returns></returns>
        public static IEnumerable ComEnum(this IEnumerable source, bool releaseElements = true)
        {
            foreach (object item in source)
            {
                try
                {
                    yield return item;
                }
                finally
                {
                    if (releaseElements)
                        ComRelease.Release(item);
                }
            }
            ComRelease.Release(source);
        }

        /// <summary>
        /// Helper for enumeration that disposes the returned items. Note that source will not be disposed,
        /// as that is normally done by foreach.
        /// </summary>
        public static IEnumerable<T> DisposeEnum<T>(this IEnumerable<T> source)
            where T : IDisposable
        {
            foreach (T item in source)
            {
                try
                {
                    yield return item;
                }
                finally
                {
                    item.Dispose();
                }
            }
        }

        #endregion

        public static void GarbageCollect()
        {
            for (int i = 0; i < 4; ++i)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }


        #region Timers

        public static void Delayed(LogContext log, int millis, System.Action action)
        {
            RegisterTimer(log, millis, action, false);
        }

        public static void Timed(LogContext log, int millis, System.Action action)
        {
            RegisterTimer(log, millis, action, true);
        }

        private static void RegisterTimer(LogContext log, int millis, System.Action action, bool repeat)
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = millis;
            timer.Tick += (s, eargs) =>
            {
                try
                {
                    action();
                    if (!repeat)
                    {
                        timer.Enabled = false;
                        timer.Dispose();
                    }
                }
                catch (System.Exception e)
                {
                    Logger.Instance.Trace(log, "Exception in timer: {0}", e);
                }
            };
            timer.Start();
        }

        #endregion

        #region Command line

        /// <summary>
        /// Quotes a single command line argument
        /// </summary>
        public static string QuoteCommandLine(string arg)
        {
            return "\"" + Regex.Replace(arg, @"(\\*)" + "\"", @"$1$1\" + "\"") + "\"";
        }

        #endregion

        public static int Align(int size, int align)
        {
            int additional = (align - (size % align)) % align;
            return size + additional;
        }

        public static NumType Bound<NumType>(NumType value, NumType min, NumType max)
            where NumType : IComparable
        {
            if (value.CompareTo(min) < 0)
                return min;
            if (value.CompareTo(max) > 0)
                return max;
            return value;
        }
    }
}
