using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Acacia.Controls
{
    public class KComboBox : KAbstractComboBox
    {
        private readonly ListBox _dropList;

        public KComboBox()
        {
            _dropList = new ListBox();
            _dropList.IntegralHeight = true;
            DropControl = _dropList;
            _dropList.DisplayMember = "DisplayName"; // TODO: remove from here
        }

        public void BeginUpdate()
        {
            _dropList.BeginUpdate();
        }

        public void EndUpdate()
        {
            _dropList.EndUpdate();
        }

        public object DataSource
        {
            get
            {
                return _dropList.DataSource;
            }

            set
            {
                _dropList.BindingContext = new BindingContext();
                _dropList.DataSource = value;
            }
        }
    }
}
