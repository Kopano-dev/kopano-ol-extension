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

using Microsoft.Test.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;

namespace AcaciaTest.Framework
{
    public static class AutomationExtensions
    {
        #region Searching

        public static AutomationElement DescendantByName(this AutomationElement element, params string[] path)
        {
            return element.DescendantByCondition(
                path.Select(value => new PropertyCondition(AutomationElement.NameProperty, value, PropertyConditionFlags.IgnoreCase)));
        }

        public static AutomationElement DescendantByCondition(this AutomationElement element, IEnumerable<Condition> conditionPath)
        {
            if (!conditionPath.Any())
                return element;

            var result = conditionPath.Aggregate(
                element,
                (parentElement, nextCondition) => parentElement == null
                                                      ? null
                                                      : parentElement.FindFirst(TreeScope.Children, nextCondition));

            return result;
        }

        #endregion

        #region Mouse

        /// <summary>
        /// Performs a mouse click in the center of the element.
        /// </summary>
        public static AutomationElement MouseClick(this AutomationElement element, MouseButton button = MouseButton.Left)
        {
            Rect r = element.Current.BoundingRectangle;
            Mouse.MoveTo(new System.Drawing.Point((int)(r.X + r.Width / 2), (int)(r.Y + r.Height / 2)));
            Mouse.Click(button);
            return element;
        }

        #endregion

    }
}
