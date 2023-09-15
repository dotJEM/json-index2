using System.Collections.Generic;
using System.Linq;

namespace DotJEM.Json.Index2.QueryParsers.Ast;

public class ListValue : Value
{
    private readonly Value[] values;

    public int Count => values.Length;
    public IEnumerable<Value> Values => values;

    public ListValue(Value[] values)
    {
        this.values = values;
    }
    public override string ToString() => "( " + string.Join(", ", values.Select(v => v.ToString())) + " )";
}