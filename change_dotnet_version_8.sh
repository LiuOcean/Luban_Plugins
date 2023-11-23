#!/bin/bash

# 查找所有.csproj文件并替换dotnet版本为8.0

find . -name '*.csproj' -exec sed -i '' 's/net[0-9]\.[0-9]/net8.0/g' {} \;
