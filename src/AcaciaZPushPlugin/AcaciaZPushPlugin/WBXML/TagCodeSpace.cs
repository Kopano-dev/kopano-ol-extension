﻿// Copyright 2012 - Johan de Koning (johan@johandekoning.nl)
// 
// This file is part of WBXML .Net Library.
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal in 
// the Software without restriction, including without limitation the rights to use, 
// copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the 
// Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies 
// or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE 
// USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// The WAP Binary XML (WBXML) specification is developed by the 
// Open Mobile Alliance (http://www.openmobilealliance.org/)
// Details about this specification can be found at
// http://www.openmobilealliance.org/tech/affiliates/wap/wap-192-wbxml-20010725-a.pdf

using System.Collections.Generic;

namespace Acacia.WBXML
{
    public abstract class TagCodeSpace
    {
        private readonly List<TagCodePage> codePages = new List<TagCodePage>();

        public void AddCodePage(TagCodePage codePage)
        {
            codePages.Add(codePage);
        }

        public virtual TagCodePage GetCodePage(int codepageId)
        {
            return codePages[codepageId];
        }

        public int ContainsTag(int codepageId, string name)
        {
            if (codePages[codepageId].ContainsTag(name))
            {
                return codepageId;
            }

            for (int i = 0; i < codePages.Count; i++)
            {
                if (i != codepageId)
                {
                    if (codePages[i].ContainsTag(name))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public abstract int GetPublicIdentifier();
    }
}