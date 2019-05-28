using System;
using System.Collections.Generic;
using System.Text;

namespace RulesX.Metadata.Rule
{
    public enum OperationCode
    {
        Equals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        EndsWith,
        NotEquals,
        StartsWith,
        Contains,
        IsBetween,
        Map,
        And,
        Or
    }
}
