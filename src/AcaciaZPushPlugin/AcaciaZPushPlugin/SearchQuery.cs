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
        void Encode(SearchQuery.PropertyIdentifier part);
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
            EncodeMulti("AND", part.Operands);
        }

        public void Encode(SearchQuery.Or part)
        {
            EncodeMulti("OR", part.Operands);
        }

        public void Encode(SearchQuery.Not part)
        {
            EncodeMulti("NOT", new[] { part.Operand });
        }

        private void EncodeMulti(string oper, IEnumerable<SearchQuery> parts)
        {
            Indent();
            _builder.Append(oper).Append("\n");
            Indent();
            _builder.Append("{\n");

            ++_indent;

            foreach (SearchQuery operand in parts)
                operand.Encode(this);

            --_indent;

            Indent();
            _builder.Append("}\n");
        }

        public void Encode(SearchQuery.PropertyBitMask part)
        {
            Indent();
            _builder.Append("BITMASK{");
            part.Property.Encode(this);
            _builder.Append(" ").Append(part.Operation).Append(" ");
            _builder.Append(part.Mask.ToString("X8"));
            _builder.Append("}\n");
        }

        private static readonly string[] COMPARISON_OPERATORS = {"<", "<=", ">", ">=", "==", "!=", "LIKE"};

        public void Encode(SearchQuery.PropertyCompare part)
        {
            Indent();
            _builder.Append("COMPARE{");
            part.Property.Encode(this);
            _builder.Append(" ").Append(COMPARISON_OPERATORS[(int)part.Operation]).Append(" ");
            _builder.Append(part.Value);
            _builder.Append("}\n"); 
        }

        public void Encode(SearchQuery.PropertyContent part)
        {
            Indent();
            _builder.Append("CONTENT{");
            part.Property.Encode(this);

            List<string> options = new List<string>();
            if (part.Operation != SearchQuery.ContentMatchOperation.Full)
                options.Add(part.Operation.ToString());

            if (part.Modifiers != SearchQuery.ContentMatchModifiers.None)
            {
                options.Add(part.Modifiers.ToString());
            }

            string optionsString = options.Count == 0 ? "" : ("(" + string.Join(",", options) + ")");

            _builder.Append(" ==").Append(optionsString).Append(" ");
            _builder.Append(part.Content);
            _builder.Append("}\n");
        }

        public void Encode(SearchQuery.PropertyExists part)
        {
            Indent();
            _builder.Append("EXISTS{");
            part.Property.Encode(this);
            _builder.Append("}\n");
        }

        public void Encode(SearchQuery.PropertyIdentifier part)
        {
            _builder.Append(part.Id);
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

            public ICollection<SearchQuery> Operands
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
            public string Id { get; private set; }
            public PropTag Tag { get; private set; }

            public PropertyIdentifier(PropTag tag)
            {
                this.Tag = tag;
                Id = string.Format("{0:X4}{1:X4}", tag.prop, (int)tag.type);
            }

            public void Encode(ISearchEncoder encoder)
            {
                encoder.Encode(this);
            }

            public override string ToString()
            {
                return Id;
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

        public enum ContentMatchOperation
        {
            /// <summary>
            /// Match full content
            /// </summary>
            Full,

            /// <summary>
            /// Match part of the content
            /// </summary>
            SubString,

            /// <summary>
            /// Match the start of the content
            /// </summary>
            Prefix
        }

        [Flags]
        public enum ContentMatchModifiers
        {
            None = 0,
            CaseInsensitive = 1,
            IgnoreNonSpace = 2,
            Loose = 4
        }

        public class PropertyContent : PropertyQuery
        {
            public ContentMatchOperation Operation { get; set; }
            public ContentMatchModifiers Modifiers { get; set; }
            public object Content { get; set; }

            public PropertyContent(PropertyIdentifier property, ContentMatchOperation operation, ContentMatchModifiers modifiers, object content) : base(property)
            {
                this.Operation = operation;
                this.Modifiers = modifiers;
                this.Content = content;
            }

            public override void Encode(ISearchEncoder encoder)
            {
                encoder.Encode(this);
            }
        }

        public enum BitMaskOperation
        {
            EQZ, NEZ
        }

        public class PropertyBitMask : PropertyQuery
        {
            public BitMaskOperation Operation { get; set; }
            public uint Mask { get; set; }

            public PropertyBitMask(PropertyIdentifier property, BitMaskOperation operation, uint mask) : base(property)
            {
                this.Operation = operation;
                this.Mask = mask;
            }

            public override void Encode(ISearchEncoder encoder)
            {
                encoder.Encode(this);
            }
        }

        #endregion
    }
}
