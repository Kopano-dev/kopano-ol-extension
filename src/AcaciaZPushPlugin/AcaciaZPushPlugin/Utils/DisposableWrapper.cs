using Acacia.Features.DebugSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Acacia.Utils
{
    abstract public class DisposableWrapper : IDisposable
    {
        protected DisposableWrapper()
        {
            Interlocked.Increment(ref Statistics.CreatedWrappers);
            this._createdTrace = new System.Diagnostics.StackTrace();
        }

        ~DisposableWrapper()
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

        abstract protected void DoRelease();
    }

}
