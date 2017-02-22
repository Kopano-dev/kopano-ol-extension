/// Copyright 2017 Kopano b.v.
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
using Acacia.Native.MAPI;

namespace Acacia
{
    public interface ISearchEncoder
    {
        void Encode(SearchQuery.PropertyBitMask part);
        void Encode(SearchQuery.PropertyCompare part);
        void Encode(SearchQuery.PropertyContent part);
        void Encode(SearchQuery.PropertyExists part);
        void Encode(SearchQuery.And part);
        void Encode(SearchQuery.Or part);
        void Encode(SearchQuery.Not part);
    }

    public class ToStringEncoder : ISearchEncoder
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private int _indent;

        private void Indent()
        {
            _builder.Append(new String(' ', _indent));
        }

        public void Encode(SearchQuery.And part)
        {
            EncodeMulti(part, "AND");
        }

        public void Encode(SearchQuery.Or part)
        {
            EncodeMulti(part, "OR");
        }

        public void Encode(SearchQuery.Not part)
        {
            _builder.Append("NOT ");
            part.Operand.Encode(this);
        }

        private void EncodeMulti(SearchQuery.MultiOperator part, string oper)
        {
            Indent();
            _builder.Append(oper).Append("\n");
            Indent();
            _builder.Append("{\n");

            ++_indent;

            foreach (SearchQuery operand in part.Operands)
                operand.Encode(this);

            --_indent;

            Indent();
            _builder.Append("}\n");
        }

        public void Encode(SearchQuery.PropertyBitMask part)
        {
            _builder.Append("BITMASK:").Append(part.Property); // TODO: operator/value
        }

        public void Encode(SearchQuery.PropertyCompare part)
        {
            _builder.Append("COMPARE:").Append(part.Property); // TODO: operator/value
        }

        public void Encode(SearchQuery.PropertyContent part)
        {
            _builder.Append("CONTENT:").Append(part.Property); // TODO: operator/value
        }

        public void Encode(SearchQuery.PropertyExists part)
        {
            _builder.Append("EXISTS:").Append(part.Property);
        }

        public string GetValue()
        {
            return _builder.ToString();
        }
    }

    abstract public class SearchQuery
    {
        #region Interface

        override public string ToString()
        {
            ToStringEncoder encoder = new ToStringEncoder();
            Encode(encoder);
            return encoder.GetValue();
        }

        abstract public void Encode(ISearchEncoder encoder);

        #endregion

        #region Implementations

        abstract public class MultiOperator : SearchQuery
        {
            private readonly List<SearchQuery> _operands = new List<SearchQuery>();

            public void Add(SearchQuery operand)
            {
                if (operand == null)
                    throw new ArgumentNullException();

                _operands.Add(operand);
            }

            public IEnumerable<SearchQuery> Operands
            {
                get { return _operands; }
            }
        }

        public class And : MultiOperator
        {
            public override void Encode(ISearchEncoder encoder)
            {
                encoder.Encode(this);
            }
        }

        public class Or : MultiOperator
        {
            public override void Encode(ISearchEncoder encoder)
            {
                encoder.Encode(this);
            }
        }

        public class Not : SearchQuery
        {
            public SearchQuery Operand
            {
                get;
                set;
            }

            public Not(SearchQuery operand)
            {
                this.Operand = operand;
            }

            public override void Encode(ISearchEncoder encoder)
            {
                encoder.Encode(this);
            }
        }


        /// <summary>
        /// TODO: this is globally useful
        /// </summary>
        public class PropertyIdentifier
        {
            private string _id;

            public PropertyIdentifier(string id)
            {
                this._id = id;
            }

            public static PropertyIdentifier FromTag(ushort prop, ushort type)
            {
                return new PropertyIdentifier(string.Format("{0:4X}{1:4X}", prop, type));
            }
        }

        abstract public class PropertyQuery : SearchQuery
        {
            public PropertyIdentifier Property { get; set; }

            protected PropertyQuery(PropertyIdentifier property)
            {
                this.Property = property;
            }

        }

        /// <summary>
        /// Order matches MAPI RELOP_ constants
        /// </summary>
        public enum ComparisonOperation : uint
        {
            Smaller,
            SmallerEqual,
            Greater,
            GreaterEqual,
            Equal,
            NotEqual,
            Like
        }

        public class PropertyCompare : PropertyQuery
        {
            public ComparisonOperation Operation { get; set; }
            public object Value { get; set; }

            public PropertyCompare(PropertyIdentifier property, ComparisonOperation operation, object value) : base(property)
            {
                this.Operation = operation;
                this.Value = value;
            }

            public override void Encode(ISearchEncoder encoder)
            {
                encoder.Encode(this);
            }
        }

        public class PropertyExists : PropertyQuery
        {
            public PropertyExists(PropertyIdentifier property) : base(property)
            {
            }

            public override void Encode(ISearchEncoder encoder)
            {
                encoder.Encode(this);
            }
        }

        public class PropertyContent : PropertyQuery
        {
            public PropertyContent(PropertyIdentifier property, uint options, object content) : base(property)
            {
                // TODO
            }

            public override void Encode(ISearchEncoder encoder)
            {
                encoder.Encode(this);
            }
        }

        public class PropertyBitMask : PropertyQuery
        {
            public PropertyBitMask(PropertyIdentifier property, bool wantZero, uint mask) : base(property)
            {
                // TODO
            }

            public override void Encode(ISearchEncoder encoder)
            {
                encoder.Encode(this);
            }
        }

        #endregion
    }
}
