#!/usr/bin/env python3
"""
File and Directory Extractor with Cleanup

This script searches for specified files and/or directories, extracts content, and optionally outputs the project structure.
It supports removing comments from code files and can also display the directory structure without file contents.
It also deletes any old 'extracted_content_*.json' files in the current directory before saving the new one.

Usage:
    python content2.py <file_or_dir1> [<file_or_dir2> ...] [--output OUTPUT_FILE] [--strip-comments] [--structure-only]

Example:
    python content2.py SiteType.cs FertilityOverlay.cs Simulation --strip-comments
"""

import os
import re
import argparse
import json
import glob
from datetime import datetime

# File types to be processed (can be customized)
FILE_TYPES = [".py", ".txt", ".md", ".js", ".html", ".css", ".cs", ".sln", ".csproj"]

# Directories to exclude
EXCLUDE_DIRS = {"ACE", "venv", ".venv", "build", ".git", ".idea", "node_modules", "__pycache__", "dist", "bin", "obj"}

# File exclusions
EXCLUDE_FILES = {"content2.py", "README.md", ".DS_Store", "Thumbs.db", ".env", "requirements.txt"}

# Maximum file size for content extraction (1MB)
MAX_FILE_SIZE = 1000000  # 1MB

def strip_comments(content):
    """Remove single-line and multi-line comments from the file content."""
    content = re.sub(r'//.*', '', content)  # Remove single-line comments (//)
    content = re.sub(r'/\*.*?\*/', '', content, flags=re.DOTALL)  # Multi-line comments (/* */)
    content = re.sub(r'///.*', '', content)  # Remove doc comments (///)
    content = re.sub(r'\n\s*\n', '\n', content)  # Remove empty lines
    return content

def is_ignored(path):
    """Check if the path should be ignored based on exclusion lists."""
    return any(exclude in path for exclude in EXCLUDE_DIRS) or \
           any(path.endswith(file) for file in EXCLUDE_FILES)

def find_files_in_dir(directory):
    """Recursively find all files in the given directory."""
    file_list = []
    for dirpath, dirnames, filenames in os.walk(directory):
        dirnames[:] = [d for d in dirnames if not is_ignored(os.path.join(dirpath, d))]
        for filename in filenames:
            if any(filename.endswith(ext) for ext in FILE_TYPES):
                file_path = os.path.join(dirpath, filename)
                if not is_ignored(file_path):
                    file_list.append(file_path)
    return file_list

def find_files_and_dirs(search_items, root_dir):
    """Search for files and directories matching the search_items in the root directory."""
    found_items = {}

    for search_item in search_items:
        search_path = os.path.join(root_dir, search_item)
        if os.path.isfile(search_path):  # If it's a file
            found_items[search_item] = 'file'
        elif os.path.isdir(search_path):  # If it's a directory
            found_items[search_item] = 'directory'
        else:
            # Search for matching filenames within subdirectories
            for dirpath, dirnames, filenames in os.walk(root_dir):
                dirnames[:] = [d for d in dirnames if not is_ignored(os.path.join(dirpath, d))]

                # Search for matching filenames
                found_files = [os.path.join(dirpath, file) for file in filenames if file == search_item]
                for f in found_files:
                    relative_path = os.path.relpath(f, root_dir)
                    found_items[relative_path] = 'file'

    return found_items

def get_content(path, strip_comments_option):
    """Read the file content and optionally strip comments."""
    try:
        with open(path, 'r', encoding='utf-8', errors='ignore') as file:
            content = file.read()
            if strip_comments_option:
                content = strip_comments(content)
            return content
    except Exception as e:
        return f"Error reading file: {str(e)}"

def remove_old_json_files():
    """Remove old extracted_content_*.json files to avoid clutter."""
    for old_file in glob.glob('extracted_content_*.json'):
        os.remove(old_file)

def main():
    parser = argparse.ArgumentParser(description="File and directory content extractor with cleanup.")
    parser.add_argument("search_items", nargs='+', help="Filenames and/or directories to search for.")
    parser.add_argument("--output", help="Output file path for the JSON result (optional).")
    parser.add_argument("--strip-comments", action="store_true", help="Strip comments from file contents.")
    parser.add_argument("--structure-only", action="store_true", help="Only output the project structure without file content.")
    args = parser.parse_args()

    # Start in the current directory
    root_dir = os.getcwd()

    # Remove old extracted_content_*.json files
    remove_old_json_files()

    # Find files and directories
    found_items = find_files_and_dirs(args.search_items, root_dir)

    # Prepare the result
    result = {
        "files": {},
        "structure": []
    }

    # Traverse each found item (file or directory)
    for item, item_type in found_items.items():
        if item_type == 'file':
            result['structure'].append(item)
            if not args.structure_only:
                file_path = os.path.join(root_dir, item)
                if os.path.getsize(file_path) <= MAX_FILE_SIZE:
                    result['files'][item] = get_content(file_path, args.strip_comments)
                else:
                    result['files'][item] = "File too large to process."
        elif item_type == 'directory':
            # Recursively find all files in this directory
            full_dir_path = os.path.join(root_dir, item)
            all_files_in_dir = find_files_in_dir(full_dir_path)
            for file_in_dir in all_files_in_dir:
                relative_file_path = os.path.relpath(file_in_dir, root_dir)
                result['structure'].append(relative_file_path)
                if not args.structure_only:
                    if os.path.getsize(file_in_dir) <= MAX_FILE_SIZE:
                        result['files'][relative_file_path] = get_content(file_in_dir, args.strip_comments)
                    else:
                        result['files'][relative_file_path] = "File too large to process."

    # Determine output filename
    output_file = args.output or f"extracted_content_{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"

    # Save the output
    with open(output_file, 'w', encoding='utf-8') as outfile:
        json.dump(result, outfile, indent=2)
    
    print(f"Output saved to {output_file}")

if __name__ == "__main__":
    main()
