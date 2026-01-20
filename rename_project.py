#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
重命名"永利系统"项目为"YongLiSystem"
包括：
1. 重命名文件夹
2. 重命名项目文件
3. 更新所有代码中的命名空间
4. 更新解决方案文件
5. 更新文档中的引用
"""

import os
import re
import shutil
from pathlib import Path

# 配置
OLD_NAME = "永利系统"
NEW_NAME = "YongLiSystem"
OLD_NAMESPACE = "永利系统"
NEW_NAMESPACE = "YongLiSystem"

PROJECT_ROOT = Path(__file__).parent
OLD_PROJECT_DIR = PROJECT_ROOT / OLD_NAME
NEW_PROJECT_DIR = PROJECT_ROOT / NEW_NAME

def replace_in_file(file_path, old_text, new_text):
    """在文件中替换文本"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        if old_text in content:
            content = content.replace(old_text, new_text)
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            return True
        return False
    except Exception as e:
        print(f"错误处理文件 {file_path}: {e}")
        return False

def update_namespace_in_file(file_path):
    """更新文件中的命名空间"""
    modified = False
    
    # 替换 namespace 声明
    if replace_in_file(file_path, f"namespace {OLD_NAMESPACE}", f"namespace {NEW_NAMESPACE}"):
        modified = True
    
    # 替换 using 语句
    if replace_in_file(file_path, f"using {OLD_NAMESPACE}", f"using {NEW_NAMESPACE}"):
        modified = True
    
    return modified

def update_all_code_files():
    """更新所有代码文件中的命名空间"""
    print("正在更新代码文件中的命名空间...")
    
    code_extensions = ['.cs', '.csproj', '.sln', '.md', '.ps1', '.bat']
    modified_count = 0
    
    # 更新新项目目录中的文件
    if NEW_PROJECT_DIR.exists():
        for ext in code_extensions:
            for file_path in NEW_PROJECT_DIR.rglob(f'*{ext}'):
                if update_namespace_in_file(file_path):
                    modified_count += 1
                    print(f"  已更新: {file_path.relative_to(PROJECT_ROOT)}")
    
    # 更新解决方案文件
    sln_file = PROJECT_ROOT / "Vx3Plus.sln"
    if sln_file.exists():
        if replace_in_file(sln_file, f'"{OLD_NAME}"', f'"{NEW_NAME}"'):
            modified_count += 1
            print(f"  已更新解决方案文件")
        if replace_in_file(sln_file, f'{OLD_NAME}\\{OLD_NAME}.csproj', f'{NEW_NAME}\\{NEW_NAME}.csproj'):
            modified_count += 1
    
    print(f"共更新 {modified_count} 个文件")
    return modified_count

def main():
    print("=" * 60)
    print("项目重命名工具")
    print("=" * 60)
    print(f"旧项目名: {OLD_NAME}")
    print(f"新项目名: {NEW_NAME}")
    print(f"旧命名空间: {OLD_NAMESPACE}")
    print(f"新命名空间: {NEW_NAMESPACE}")
    print("=" * 60)
    
    # 检查旧项目目录是否存在
    if not OLD_PROJECT_DIR.exists():
        print(f"错误: 找不到项目目录 {OLD_PROJECT_DIR}")
        return
    
    # 步骤1: 重命名文件夹（如果新目录不存在）
    if not NEW_PROJECT_DIR.exists():
        print(f"\n步骤1: 重命名文件夹 {OLD_NAME} -> {NEW_NAME}")
        try:
            shutil.move(str(OLD_PROJECT_DIR), str(NEW_PROJECT_DIR))
            print(f"  ✓ 文件夹重命名成功")
        except Exception as e:
            print(f"  ✗ 文件夹重命名失败: {e}")
            return
    else:
        print(f"\n步骤1: 目标目录已存在，跳过文件夹重命名")
    
    # 步骤2: 重命名项目文件
    print(f"\n步骤2: 重命名项目文件")
    old_csproj = NEW_PROJECT_DIR / f"{OLD_NAME}.csproj"
    new_csproj = NEW_PROJECT_DIR / f"{NEW_NAME}.csproj"
    
    if old_csproj.exists() and not new_csproj.exists():
        try:
            shutil.move(str(old_csproj), str(new_csproj))
            print(f"  ✓ {OLD_NAME}.csproj -> {NEW_NAME}.csproj")
        except Exception as e:
            print(f"  ✗ 项目文件重命名失败: {e}")
    
    old_csproj_user = NEW_PROJECT_DIR / f"{OLD_NAME}.csproj.user"
    new_csproj_user = NEW_PROJECT_DIR / f"{NEW_NAME}.csproj.user"
    
    if old_csproj_user.exists() and not new_csproj_user.exists():
        try:
            shutil.move(str(old_csproj_user), str(new_csproj_user))
            print(f"  ✓ {OLD_NAME}.csproj.user -> {NEW_NAME}.csproj.user")
        except Exception as e:
            print(f"  ✗ 项目用户文件重命名失败: {e}")
    
    # 步骤3: 更新所有代码文件
    print(f"\n步骤3: 更新代码文件中的命名空间")
    update_all_code_files()
    
    print("\n" + "=" * 60)
    print("重命名完成！")
    print("=" * 60)
    print("\n接下来需要执行：")
    print("1. 使用 git 添加新文件: git add YongLiSystem/")
    print("2. 使用 git 删除旧文件: git rm -r 永利系统/")
    print("3. 提交更改: git commit -m '重命名项目: 永利系统 -> YongLiSystem'")

if __name__ == "__main__":
    main()
