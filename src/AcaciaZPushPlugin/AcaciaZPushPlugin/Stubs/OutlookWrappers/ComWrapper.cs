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

using Acacia.Features.DebugSupport;
using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Acacia.Stubs.OutlookWrappers
{
    public abstract class ComWrapper : IComWrapper
    {

        /// <summary>
        /// Creates a wrapper.
        /// </summary>
        internal ComWrapper()
        {
            Interlocked.Increment(ref Statistics.CreatedWrappers);
            this._createdTrace = new System.Diagnostics.StackTrace();
            MustRelease = true;
        }

        ~ComWrapper()
        {
            Interlocked.Increment(ref Statistics.DeletedWrappers);
            if (!_isDisposed)
            {
                Logger.Instance.Warning(this, "Undisposed wrapper: {0}", _createdTrace);
                // Dispose, but don't count auto disposals, so the stats show it.
                DoRelease();
            }
        }

        private bool _isDisposed;
        private readonly System.Diagnostics.StackTrace _createdTrace;

        virtual public void Dispose()
        {
            if (!_isDisposed)
            {
                Logger.Instance.TraceExtra(this, "Disposing wrapper: {0}", new System.Diagnostics.StackTrace());
                _isDisposed = true;
                Interlocked.Increment(ref Statistics.DisposedWrappers);
                DoRelease();
            }
        }

        public bool MustRelease
        {
            get;
            set;
        }

        abstract protected void DoRelease();
    }
}
