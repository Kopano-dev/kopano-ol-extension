using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Utils
{
    public interface DisposableTracer
    {
        void Created(DisposableWrapper wrapper, out int id);
        void Deleted(DisposableWrapper wrapper, bool wasDisposed);
        void Disposed(DisposableWrapper wrapper);
    }
}
