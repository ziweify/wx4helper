using System;
using System.Linq;
using Unit.Shared.Platform;

Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("🔍 平台枚举诊断");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

// 1. 获取所有平台
var allPlatforms = BetPlatformHelper.GetAllPlatforms();
Console.WriteLine($"\n📋 共有 {allPlatforms.Length} 个平台:");
for (int i = 0; i < allPlatforms.Length; i++)
{
    var platform = allPlatforms[i];
    Console.WriteLine($"   [{i:D2}] {platform,-15} (值={(int)platform:D2})");
}

// 2. 获取所有平台名称
Console.WriteLine($"\n📋 GetAllPlatformNames():");
var platformNames = BetPlatformHelper.GetAllPlatformNames();
for (int i = 0; i < platformNames.Length; i++)
{
    Console.WriteLine($"   [{i:D2}] {platformNames[i]}");
}

// 3. 过滤 yyds 后的列表（BaiShengVx3Plus 的实际列表）
Console.WriteLine($"\n📋 BaiShengVx3Plus 支持的平台（过滤 yyds）:");
var supported = platformNames.Where(p => p != "yyds").ToArray();
for (int i = 0; i < supported.Length; i++)
{
    Console.WriteLine($"   [{i:D2}] {supported[i]}");
}

// 4. 测试平台的索引
Console.WriteLine($"\n🔍 查找'测试平台':");
var testPlatformEnum = BetPlatform.测试平台;
Console.WriteLine($"   枚举值: {(int)testPlatformEnum}");
Console.WriteLine($"   ToString(): {testPlatformEnum}");
Console.WriteLine($"   在 allPlatforms 中的索引: {Array.IndexOf(allPlatforms, testPlatformEnum)}");
Console.WriteLine($"   在 platformNames 中的索引: {Array.IndexOf(platformNames, "测试平台")}");
Console.WriteLine($"   在 supported 中的索引: {Array.IndexOf(supported, "测试平台")}");

// 5. 黄金海岸的索引
Console.WriteLine($"\n🔍 查找'黄金海岸':");
var goldCoastEnum = BetPlatform.黄金海岸;
Console.WriteLine($"   枚举值: {(int)goldCoastEnum}");
Console.WriteLine($"   ToString(): {goldCoastEnum}");
Console.WriteLine($"   在 allPlatforms 中的索引: {Array.IndexOf(allPlatforms, goldCoastEnum)}");
Console.WriteLine($"   在 platformNames 中的索引: {Array.IndexOf(platformNames, "黄金海岸")}");
Console.WriteLine($"   在 supported 中的索引: {Array.IndexOf(supported, "黄金海岸")}");

// 6. 模拟快速设置选择"测试平台"
Console.WriteLine($"\n🧪 模拟快速设置选择'测试平台':");
int testPlatformIndex = Array.IndexOf(supported, "测试平台");
Console.WriteLine($"   用户选择索引: {testPlatformIndex}");
var selectedPlatform = BetPlatformHelper.GetByIndex(testPlatformIndex);
Console.WriteLine($"   GetByIndex({testPlatformIndex}) = {selectedPlatform} ({(int)selectedPlatform})");
Console.WriteLine($"   保存到数据库: Platform = \"{selectedPlatform}\"");

// 7. 模拟配置管理器加载
Console.WriteLine($"\n🧪 模拟配置管理器加载:");
string dbPlatform = selectedPlatform.ToString();
Console.WriteLine($"   从数据库读取: Platform = \"{dbPlatform}\"");
var parsedPlatform = BetPlatformHelper.Parse(dbPlatform);
Console.WriteLine($"   Parse(\"{dbPlatform}\") = {parsedPlatform} ({(int)parsedPlatform})");

// 在配置管理器的下拉框中查找
int findIndex = -1;
for (int i = 0; i < supported.Length; i++)
{
    if (supported[i] == parsedPlatform.ToString())
    {
        findIndex = i;
        break;
    }
}
Console.WriteLine($"   在配置管理器下拉框中查找'{parsedPlatform}'的索引: {findIndex}");
if (findIndex >= 0)
{
    Console.WriteLine($"   ✅ 找到！应该显示: {supported[findIndex]}");
}
else
{
    Console.WriteLine($"   ❌ 未找到！");
}

Console.WriteLine($"\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
