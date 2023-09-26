# Luban_Plugins

## 使用说明

当前为了保证和 Luban 官方仓库一致, 使用 submodule 进行管理, 使用前请更新对应的仓库

在 Luban.Plugins 仓库下, 定制了 3 个工程

- Luban.Runner
- Luban.DataValidator.Strict
- Luban.DataTarget.Json.Newtonsoft


## Luban.Runner

为了不修改 Luban.csproj, 又能方便的测试和打包, 因此分出来 Luban.Runner 代替 Luban 作为运行时

> 因此你需要在 dotnet 执行的参数中, 将 Luban.dll 修改为 Luban.Runner.dll

该项目下并没有任何 cs 文件, 仅 link 了 Luban.csproj 内必要的 cs 文件

## Luban.DataValidator.Strict

在我们使用的流程中, 通常会标记一些行的数据为 `test`, 也就是测试数据, 如果策划没有注意, 在一些 ref 的字段中, 刚好填了 `test` 数据, 就会导致运行时的错误

而在 BuildIn 中的 RefValidator, 仅打印了错误日志, 并没有执行如下代码

```csharp
GenerationContext.Current.LogValidatorFail(this);
```

StrictRefValidator 为了解决这个问题, 重写了 Validate 函数, 并追加了对应的标记代码

## Luban.DataTarget.Json.Newtonsoft

在 Json 面对多态的序列化和反序列化时, 通常会追加 "$type" 字段, 比如当我们需要序列化 `Model.dll` 下的 `Example.Circle` 类时, 实际的 json 文件长这样

```json
"name_shape": {
  "$type": "Example.Triangle, Model",
  "a": 1,
  "b": 2,
  "c": 3
},
```

而 BuildIn 环境下的 json 长这样

```json
"name_shape": {
  "$type": "Example.Triangle",
  "a": 1,
  "b": 2,
  "c": 3
}
```

这里缺少了 dll 信息, 因此这个插件的主要作用就是补全这个缺失信息, 目前提供了如下参数列表

- -x newtonsoft.dll=Model 
- -x newtonsoft.namespace=Example

> 我平时的配表习惯, 不喜欢在 Luban 内引入 namespace, 而是直接在目标文件中固定写入, 因此需要手动追加