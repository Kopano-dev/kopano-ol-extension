using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acacia.ZPush.Connect.Soap
{
    /// <summary>
    /// Specifies a Soap field id for a field. If used, the class must also be annotated with SoapField.
    /// </summary>
    public class SoapFieldAttribute : Attribute
    {
        public object FieldId { get; set; }

        public SoapFieldAttribute(object fieldId = null)
        {
            this.FieldId = fieldId;
        }
    }
}
