using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string file = @"E:\gitcode\wx4helper\BinGoPlans\Services\DatabaseService.cs";
        byte[] bytes = File.ReadAllBytes(file);
        string content = Encoding.UTF8.GetString(bytes);
        
        content = content.Replace("throw new Exception($\"鏁版嵁搴撳垵濮嬪寲澶辫触: {ex.Message}\", ex);", 
                                   "throw new Exception(\"数据库初始化失败: \" + ex.Message, ex);");
        content = content.Replace("throw new Exception($\"淇濆瓨鏁版嵁澶辫触 (IssueId={data.IssueId}): {ex.Message}\", ex);", 
                                   "throw new Exception(\"保存数据失败 (IssueId=\" + data.IssueId + \"): \" + ex.Message, ex);");
        content = content.Replace("throw new Exception($\"鎵归噺淇濆瓨鏁版嵁澶辫触: {ex.Message}\", ex);", 
                                   "throw new Exception(\"批量保存数据失败: \" + ex.Message, ex);");
        content = content.Replace("throw new Exception($\"鍔犺浇鏁版嵁澶辫触 (Date={date:yyyy-MM-dd}): {ex.Message}\", ex);", 
                                   "throw new Exception(\"加载数据失败 (Date=\" + date.ToString(\"yyyy-MM-dd\") + \"): \" + ex.Message, ex);");
        content = content.Replace("throw new Exception($\"鍔犺浇鎵€鏈夋暟鎹け璐? {ex.Message}\", ex);", 
                                   "throw new Exception(\"加载所有数据失败: \" + ex.Message, ex);");
        
        byte[] newBytes = new UTF8Encoding(true).GetBytes(content);
        File.WriteAllBytes(file, newBytes);
        Console.WriteLine("已修复");
    }
}

