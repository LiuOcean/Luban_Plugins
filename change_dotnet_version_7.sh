#!/bin/bash

# 查找所有.csproj文件并替换dotnet版本为8.0

find . -name "*.csproj" -type f -exec sed -i 's/<TargetFramework>net[0-9.]*<\/TargetFramework>/<TargetFramework>net7.0<\/TargetFramework>/g' {} +
