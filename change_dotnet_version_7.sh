#!/bin/bash

# 查找所有.csproj文件并替换dotnet版本为7.0

find . -name '*.csproj' -exec sed -i '' 's/net[0-9]\.[0-9]/net7.0/g' {} \;
