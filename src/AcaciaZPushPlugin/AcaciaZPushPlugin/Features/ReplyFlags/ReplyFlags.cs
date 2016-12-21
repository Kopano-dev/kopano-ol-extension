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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Acacia.Stubs;
using Acacia.Utils;

namespace Acacia.Features.ReplyFlags
{
    public enum Verb
    {
        NONE,
        REPLIED,
        REPLIED_TO_ALL,
        FORWARDED
    }

    // TODO: unit tests for parsing
    public class ReplyFlags
    {
        private readonly IMailItem _item;

        public Verb Verb
        {
            get;
            private set;
        }

        public DateTime? Date
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructor. Reads the reply state from the mail item's reply properties
        /// </summary>
        public ReplyFlags(IMailItem item)
        {
            this._item = item;
            ReadFromProperties();
        }

        /// <summary>
        /// Fully initializing constructor.
        /// </summary>
        private ReplyFlags(IMailItem item, Verb verb, DateTime date)
        {
            this._item = item;
            this.Verb = verb;
            this.Date = date;
        }

        /// <summary>
        /// Constructs the ReplyFlags object from the mail's categories, if present.
        /// If the category is present, it is removed. Changes are not saved, so this will have to be done explicitly.
        /// If no category is present, null is returned.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ReplyFlags FromCategory(IMailItem item, bool updateCategories = true)
        {
            string[] categories = item.AttrCategories;
            if (categories == null || categories.Length == 0)
                return null;

            // See if we have the z-push reply header
            for (int i = 0; i < categories.Length; ++i)
            {
                string category = categories[i];

                // This test will be invoked on every change, so do a quick test first
                if (category.StartsWith(Constants.ZPUSH_REPLY_CATEGORY_PREFIX))
                {
                    string suffix = category.Substring(Constants.ZPUSH_REPLY_CATEGORY_PREFIX.Length);
                    Match match = Constants.ZPUSH_REPLY_CATEGORY_REGEX.Match(suffix);
                    if (match.Success)
                    {
                        try
                        {
                            string dateString = match.Groups[2].Value;

                            // Parse the state
                            Verb verb = VerbFromString(match.Groups[1].Value);

                            // Parse the date
                            DateTime date = DateTime.Parse(dateString);

                            // Remove the category
                            if (updateCategories)
                            {
                                var categoriesList = new List<string>(categories);
                                categoriesList.RemoveAt(i);
                                item.AttrCategories = categoriesList.ToArray();
                            }

                            // Return the flags
                            return new ReplyFlags(item, verb, date);
                        }
                        catch (System.Exception e)
                        {
                            // Ignore any exception
                            Logger.Instance.Error(typeof(ReplyFlags), "Exception while parsing reply category: {0}", e);
                        }
                    }
                }
            }

            return null;
        }
        
        private void ReadFromProperties()
        {
            // Read the date
            Date = _item.AttrLastVerbExecutionTime;

            // And the state
            int state = _item.AttrLastVerbExecuted;
            switch (state)
            {
                case OutlookConstants.EXCHIVERB_FORWARD:
                    Verb = Verb.FORWARDED;
                    break;
                case OutlookConstants.EXCHIVERB_REPLYTOALL:
                case OutlookConstants.EXCHIVERB_REPLYTOSENDER:
                case OutlookConstants.EXCHIVERB_REPLYTOFOLDER:
                    Verb = Verb.REPLIED;
                    break;
                default:
                    Verb = Verb.NONE;
                    break;
            }
        }

        /// <summary>
        /// Updates the local mail item from the current state
        /// </summary> 
        public void UpdateLocal()
        {
            // Determine icon and verb
            int icon = OutlookConstants.PR_ICON_INDEX_NONE;
            int verb = OutlookConstants.EXCHIVERB_OPEN;
            switch (Verb)
            {
                case Verb.REPLIED:
                    icon = OutlookConstants.PR_ICON_INDEX_REPLIED;
                    verb = OutlookConstants.EXCHIVERB_REPLYTOSENDER;
                    break;
                case Verb.REPLIED_TO_ALL:
                    icon = OutlookConstants.PR_ICON_INDEX_REPLIED;
                    verb = OutlookConstants.EXCHIVERB_REPLYTOALL;
                    break;
                case Verb.FORWARDED:
                    icon = OutlookConstants.PR_ICON_INDEX_FORWARDED;
                    verb = OutlookConstants.EXCHIVERB_FORWARD;
                    break;
            }

            // Set the properties
            _item.SetProperties(
                new string[]
                {
                    OutlookConstants.PR_ICON_INDEX,
                    OutlookConstants.PR_LAST_VERB_EXECUTED,
                    OutlookConstants.PR_LAST_VERB_EXECUTION_TIME
                },
                new object[]
                {
                    icon,
                    verb,
                    Date
                }
            );

            // And save
            _item.Save();
        }

        override public string ToString()
        {
            return Verb + "=" + Date;
        }

        public static Verb VerbFromString(string verb)
        {
            if (verb == Constants.ZPUSH_REPLY_CATEGORY_REPLIED)
                return Verb.REPLIED;
            else if (verb == Constants.ZPUSH_REPLY_CATEGORY_REPLIED_TO_ALL)
                return Verb.REPLIED_TO_ALL;
            else if (verb == Constants.ZPUSH_REPLY_CATEGORY_FORWARDED)
                return Verb.FORWARDED;
            else
                throw new System.Exception("Invalid verb: " + verb);
        }

        public static int VerbToExchange(Verb verb)
        {
            switch(verb)
            {
                case Verb.REPLIED:
                    return OutlookConstants.EXCHIVERB_REPLYTOSENDER;
                case Verb.REPLIED_TO_ALL:
                    return OutlookConstants.EXCHIVERB_REPLYTOALL;
                case Verb.FORWARDED:
                    return OutlookConstants.EXCHIVERB_FORWARD;
                default:
                    throw new System.Exception("Invalid verb: " + verb);
            }
        }
    }
}
