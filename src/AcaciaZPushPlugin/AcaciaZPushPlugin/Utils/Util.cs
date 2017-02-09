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
        public static bool NullSafeEquals<ObjType>(ObjType a, ObjType b)
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

        public static IEnumerable<T> RawEnum<T>(this IEnumerable<T> source, bool releaseItems = true)
        {
            foreach (T item in source)
            {
                try
                {
                    yield return item;
                }
                finally
                {
                    if (releaseItems)
                        ComRelease.Release(item);
                }
            }
            ComRelease.Release(source);
        }

        // TODO: check this
        public static IEnumerable RawEnum(this IEnumerable source, bool releaseItems = true)
        {
            foreach (object item in source)
            {
                try
                {
                    yield return item;
                }
                finally
                {
                    if (releaseItems)
                        ComRelease.Release(item);
                }
            }
            ComRelease.Release(source);
        }

        public static void GarbageCollect()
        {
            for (int i = 0; i < 4; ++i)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
