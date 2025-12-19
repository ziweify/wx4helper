import os

# 定义重命名映射
rename_map = {
    "251219-001.md": "251219-001-MVVM架构重构.md",
    "251219-002.md": "251219-002-本地DLL引用.md",
    "251219-003.md": "251219-003-引用缺失解决.md",
    "251219-004.md": "251219-004-NetCore路径.md",
    "251219-005.md": "251219-005-编译错误修复.md",
    "251219-006.md": "251219-006-编译成功说明.md",
    "251219-007.md": "251219-007-MVVM方案对比.md",
    "251219-008.md": "251219-008-框架深度对比.md",
    "251219-009.md": "251219-009-切换操作指南.md",
    "251219-010.md": "251219-010-切换完成报告.md",
    "251219-011.md": "251219-011-Ribbon设计指南.md",
}

# 目标目录
target_dir = r"E:\gitcode\wx4helper\永利系统\项目说明"

# 执行重命名
for old_name, new_name in rename_map.items():
    old_path = os.path.join(target_dir, old_name)
    new_path = os.path.join(target_dir, new_name)
    
    if os.path.exists(old_path):
        os.rename(old_path, new_path)
        print(f"✅ {old_name} → {new_name}")
    else:
        print(f"❌ 文件不存在: {old_name}")

print("\n重命名完成！")

