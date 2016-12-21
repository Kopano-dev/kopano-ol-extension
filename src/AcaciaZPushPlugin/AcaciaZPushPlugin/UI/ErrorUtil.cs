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

using Acacia.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.UI
{
    public static class ErrorUtil
    {
        // TODO: remove this
        public static void HandleError(object context, string logMessage, Exception e, 
                                       string titleId, string messageId, params object[] messageParams)
        {
            Logger.Instance.Error(context, "{0}: {1}", logMessage, e);
            string msg = StringUtil.GetResourceString(messageId);
            if (messageParams.Length > 0)
                msg = string.Format(msg, messageParams);
            MessageBox.Show(msg,
                            StringUtil.GetResourceString(titleId),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                            );

        }

        public static void HandleErrorNew(object context, string logMessage, Exception e,
                                       string title, string message, params object[] messageParams)
        {
            if (messageParams.Length > 0)
            {
                logMessage = string.Format(logMessage, messageParams);
                message = string.Format(message, messageParams);
            }
            Logger.Instance.Error(context, "{0}: {1}", logMessage, e);
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

        }
    }
}
