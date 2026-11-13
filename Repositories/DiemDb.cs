using Microsoft.Data.SqlClient;
using QLThueDoCuoi.Data;
using System.Data;
namespace QLThueDoCuoi.Repositories;
// Cac truy van cua Diem di qua KetNoiDatabase co san, khong sua lop dung chung.
public sealed class DiemDb(KetNoiDatabase db)
{
    public async Task<List<Dictionary<string,object?>>> Query(string sql, params SqlParameter[] args)
    {
        await using var c = db.TaoKetNoi(SqlClientFactory.Instance);
        await c.OpenAsync();
        await using var cmd = c.CreateCommand(); cmd.CommandText=sql;
        foreach(var p in args) cmd.Parameters.Add(p);
        await using var r=await cmd.ExecuteReaderAsync();
        var rows=new List<Dictionary<string,object?>>();
        while(await r.ReadAsync()) { var d=new Dictionary<string,object?>(); for(int i=0;i<r.FieldCount;i++) d[r.GetName(i)]=r.IsDBNull(i)?null:r.GetValue(i);rows.Add(d); }
        return rows;
    }
    public async Task<int> Execute(string sql,params SqlParameter[] args)
    {
        await using var c=db.TaoKetNoi(SqlClientFactory.Instance); await c.OpenAsync();
        await using var cmd=c.CreateCommand();cmd.CommandText=sql;foreach(var p in args)cmd.Parameters.Add(p);
        return await cmd.ExecuteNonQueryAsync();
    }
    public static SqlParameter P(string key,object? value)=>new(key,value??DBNull.Value);
}
