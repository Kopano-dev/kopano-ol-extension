using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.Features.SyncState
{
    public enum ResyncOption
    {
        GAB, Signatures, ServerData, Full
    }

    public interface SyncState
    {
        /// <summary>
        /// Returns the total amount of items to sync.
        /// </summary>
        long Total { get; }

        /// <summary>
        /// Returns the number of remaining items to sync.
        /// </summary>
        long Remaining { get; }

        /// <summary>
        /// Returns the number of items already synced;
        /// </summary>
        long Done { get; }

        bool IsSyncing { get; }

        /// <summary>
        /// Resynchronises the specified option.
        /// </summary>
        /// <param name="option"></param>
        /// <returns>True if the task is complete, false if it is still running.</returns>
        bool Resync(ResyncOption option);

        bool CanResync(ResyncOption option);

        void Update();
    }
}
