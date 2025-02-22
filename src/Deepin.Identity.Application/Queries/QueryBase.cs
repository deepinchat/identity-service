namespace Deepin.Identity.Application.Queries;

public abstract class QueryBase
{
    protected string BuildSqlWithSchema(string sql)
    {
        return $"set search_path to idsv; {sql}";
    }
}

