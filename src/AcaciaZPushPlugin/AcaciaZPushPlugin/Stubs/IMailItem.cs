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
using System.Threading.Tasks;

namespace Acacia.Stubs
{
    /// <summary>
    /// Specialisation for mail messages
    /// </summary>
    public interface IMailItem : IItem
    {
        #region Reply verbs

        DateTime? AttrLastVerbExecutionTime { get; set; }
        int AttrLastVerbExecuted { get; set; }

        #endregion

        #region Sender

        string SenderEmailAddress { get; }
        string SenderName { get; }

        /// <summary>
        /// Sets the sender.
        /// </summary>
        /// <param name="addressEntry">The address. The caller is responsible for disposing.</param>
        void SetSender(IAddressEntry addressEntry);

        #endregion

        #region Recipients

        string To { get; set; }
        string CC { get; set; }
        string BCC { get; set; }
        IRecipients Recipients { get; }

        #endregion
    }
}
