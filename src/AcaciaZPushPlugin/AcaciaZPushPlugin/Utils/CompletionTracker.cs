using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Acacia.Utils
{
    public class CompletionTracker
    {
        public class Step : IDisposable
        {
            private readonly CompletionTracker _tracker;

            public Step(CompletionTracker tracker)
            {
                this._tracker = tracker;
            }

            public void Dispose()
            {
                _tracker.End();
            }
        }

        private readonly Action _completion;
        private int steps = 0;

        public CompletionTracker(Action completion)
        {
            this._completion = completion;
        }

        /// <summary>
        /// Begins a sub-step.
        /// </summary>
        /// <returns>A step. This may be disposed, or End may be used</returns>
        public Step Begin()
        {
            Interlocked.Increment(ref steps);
            return new Step(this);
        }

        public void End()
        {
            if (Interlocked.Decrement(ref steps) == 0)
            {
                // Done
                _completion();
            }
        }
    }
}
