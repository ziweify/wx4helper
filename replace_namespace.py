#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import os
import re

def replace_in_file(filepath):
    """替换文件中的命名空间"""
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # 替换命名空间
        content = content.replace('BaiShengVx3Plus', 'zhaocaimao')
        
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        
        return True
    except Exception as e:
        print(f"Error processing {filepath}: {e}")
        return False

def main():
    """主函数"""
    base_dir = 'zhaocaimao'
    
    # 需要处理的文件扩展名
    extensions = ['.cs', '.csproj', '.resx']
    
    # 排除的目录
    exclude_dirs = {'bin', 'obj', '0-资料', 'packages', 'x64', 'MyLog', 'sound', 'tools', '.git'}
    
    count = 0
    for root, dirs, files in os.walk(base_dir):
        # 过滤排除的目录
        dirs[:] = [d for d in dirs if d not in exclude_dirs]
        
        for file in files:
            if any(file.endswith(ext) for ext in extensions):
                filepath = os.path.join(root, file)
                # 跳过 LoginForm.Designer.cs
                if 'LoginForm.Designer.cs' in filepath:
                    continue
                
                # 检查文件是否包含 BaiShengVx3Plus
                try:
                    with open(filepath, 'r', encoding='utf-8') as f:
                        if 'BaiShengVx3Plus' in f.read():
                            if replace_in_file(filepath):
                                count += 1
                                print(f"Processed: {filepath}")
                except:
                    pass
    
    print(f"\nTotal files processed: {count}")

if __name__ == '__main__':
    main()

