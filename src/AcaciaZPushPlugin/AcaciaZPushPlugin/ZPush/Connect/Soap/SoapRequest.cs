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
using System.Xml;

namespace Acacia.ZPush.Connect.Soap
{
    /// <summary>
    /// Abstract non-generic base for Soap requests. Used by the encoder to handle the parts that are not specific to the request
    /// </summary>
    public abstract class SoapRequestBase
    {
        virtual public string RequestName
        {
            get { return this.GetType().Name.StripSuffix("Request"); }
        }

        public SoapParameters Parameters { get; protected set; }
        
        abstract public object ParseResponse(XmlNode part);
    };

    /// <summary>
    /// Base class for a Soap request.
    /// </summary>
    /// <typeparam name="ResponseType">
    /// The type of response that is expected. Specify DBNull for void.
    /// </typeparam>
    abstract public class SoapRequest<ResponseType> : SoapRequestBase
    {
        protected SoapRequest()
        {
            Parameters = new SoapParameters();
        }

        public override object ParseResponse(XmlNode part)
        {
            return SoapSerializer.Deserialize(part, typeof(ResponseType));
        }

        virtual public string UserName { get { return null; } }
    }
}
