var dir = @"项目说明";
var map = new Dictionary<string, string>
{
    ["251219-002.md"] = "251219-002-本地DLL引用.md",
    ["251219-003.md"] = "251219-003-引用缺失解决.md",
    ["251219-004.md"] = "251219-004-NetCore路径.md",
    ["251219-005.md"] = "251219-005-编译错误修复.md",
    ["251219-006.md"] = "251219-006-编译成功说明.md",
    ["251219-007.md"] = "251219-007-MVVM方案对比.md",
    ["251219-008.md"] = "251219-008-框架深度对比.md",
    ["251219-009.md"] = "251219-009-切换操作指南.md",
    ["251219-010.md"] = "251219-010-切换完成报告.md",
    ["251219-011.md"] = "251219-011-Ribbon设计指南.md",
};

foreach (var kvp in map)
{
    var old = Path.Combine(dir, kvp.Key);
    var newPath = Path.Combine(dir, kvp.Value);
    if (File.Exists(old))
    {
        File.Move(old, newPath);
        Console.WriteLine($"✅ {kvp.Key} → {kvp.Value}");
    }
}
Console.WriteLine("\n✅ 全部完成！");

