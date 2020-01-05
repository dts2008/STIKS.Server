using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace STIKS.Model
{
    public enum FilterType
    {
        Equal = 0,
        MoreOrEqual = 1,
        LessOrEqual = 2,
        More = 3,
        Less = 4,
        In = 5
    }
    public class FilterItem
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public FilterType fType { get; set; }

        public FieldInfo Field { get; set; }
    }
}
