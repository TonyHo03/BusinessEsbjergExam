using LinqToDB.Mapping;

namespace Infra;


public enum RequestAction
{
    [MapValue("Approve")]Approve,
    [MapValue("Reject")]Reject,
    [MapValue("Cancel")]Cancel
}