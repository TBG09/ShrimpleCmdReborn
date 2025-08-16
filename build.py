import sys
import os
import re
import subprocess

# The name of the file to modify
CS_FILE = "ApplicationSettings.cs"

# Check for a command line argument
if len(sys.argv) < 2:
    print("Usage: python build.py <command>")
    print("Commands:")
    print("  build - Updates build number and builds the project.")
    print("  run   - Updates build number, builds, and runs the project.")
    sys.exit(1)

command = sys.argv[1]

# 1. Read the file content
with open(CS_FILE, 'r') as file:
    content = file.read()

# 2. Find and extract the current build number using a regular expression
match = re.search(r'public static string Build = "(\d+)"', content)
if match:
    current_build_str = match.group(1)
    current_build_num = int(current_build_str)
    
    # 3. Increment the number
    new_build_num = current_build_num + 1
    
    # 4. Format with leading zeros (e.g., 0001)
    new_build_str = f"{new_build_num:04d}"
    
    # 5. Replace the old build number in the content
    new_content = re.sub(
        rf'public static string Build = "{current_build_str}"',
        rf'public static string Build = "{new_build_str}"',
        content
    )
    
    # 6. Write the updated content back to the file
    with open(CS_FILE, 'w') as file:
        file.write(new_content)
else:
    print(f"Error: Could not find build number in {CS_FILE}")
    sys.exit(1)

# 7. Execute the dotnet command
if command == "build":
    print("Building...")
    subprocess.run(["dotnet", "build", "-v", "d"])
elif command == "run":
    print("Running...")
    subprocess.run(["dotnet", "run","-v", "d"])
else:
    print(f"Error: Unknown command '{command}'")
    sys.exit(1)

print(f"Build number updated to {new_build_str}")