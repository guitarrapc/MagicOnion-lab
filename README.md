# MagicOnion-lab

[Cysharp/MagicOnion](https://github.com/Cysharp/MagicOnion) lab.

# List of projects

## .NET Shared

* [x] src/MagicOnionLab.Shared: .NET Shared with Service/Hub and MessagePack object

## .NET Server

* [x] src/MagicOnionLab.Server: .NET Server

## Client

.NET

* [x] src/MagicOnionLab.Net.Client: .NET Client

Unity - Desktop

* [x] src/MagicOnionLab.Unity.Windows.Mono: Windows Unity Client (Mono)
* [ ] src/MagicOnionLab.Unity.Windows.IL2CPP: Windows Unity Client (IL2CPP)
* [ ] src/MagicOnionLab.Unity.Linux.Mono: Linux Unity Client (Mono)
* [ ] src/MagicOnionLab.Unity.Linux.IL2CPP: Linux Unity Client (IL2CPP)
* [ ] src/MagicOnionLab.Unity.macOS.Mono: Linux Unity Client (Mono)
* [ ] src/MagicOnionLab.Unity.macOS.IL2CPP: Linux Unity Client (IL2CPP)

Unity - Mobile

* [ ] src/MagicOnionLab.Unity.Android.Mono: Android Unity Client (Mono)
* [ ] src/MagicOnionLab.Unity.Android.IL2CPP: Android Unity Client (IL2CPP)
* [ ] src/MagicOnionLab.Unity.iOS.Mono: Android Unity Client (Mono)
* [ ] src/MagicOnionLab.Unity.iOS.IL2CPP: Android Unity Client (IL2CPP)

# FAQ

## Shared - MagicOnion.Generator (dotnet-moc) not support Central Package Magement

dotnet tool [`Magiconion.Generator (dotnet-moc)`](https://www.nuget.org/packages/MagicOnion.Generator/) are used to generate Unity MagicOnion client code from Interface files. However moc is not support Central Package Management. So, you need to install and mange each NuGet packages in Shared project.

If you use Central Package Management, you will encouter following error on `GenerateMagicOnion` MsBuild task.

```
$ dotnet tool run dotnet-moc -i ./ChatApp.Shared.csproj -o ../ChatApp.Unity/Assets/Scripts/Generated/MagicOnion.Generated.cs --verbose
[MagicOnionCompiler] Option:Input: ./ChatApp.Shared.csproj
[MagicOnionCompiler] Option:Output: ../ChatApp.Unity/Assets/Scripts/Generated/MagicOnion.Generated.cs
[MagicOnionCompiler] Option:DisableAutoRegister: False
[MagicOnionCompiler] Option:Namespace: MagicOnion
[MagicOnionCompiler] Option:ConditionalSymbol:
[MagicOnionCompiler] Option:UserDefinedFormattersNamespace: MessagePack.Formatters
[MagicOnionCompiler] Option:SerializerType: MessagePack
[MagicOnionCompiler] Assembly version: 5.1.8+391032b35d9b79fc5ad866311dbae0c24e80d8f9
[MagicOnionCompiler] RuntimeInformation.OSDescription: Microsoft Windows 10.0.19045
[MagicOnionCompiler] RuntimeInformation.ProcessArchitecture: X64
[MagicOnionCompiler] RuntimeInformation.FrameworkDescription: .NET 7.0.10
Project Compilation Start:./ChatApp.Shared.csproj
[PseudoCompilation] Creating compilation from project(s). (PreprocessorSymbolNames=)
[PseudoCompilation] Open project './ChatApp.Shared.csproj'
Fail in application running on Program.RunAsync
System.NullReferenceException: Object reference not set to an instance of an object.
   at MagicOnion.Generator.Utils.PseudoCompilation.CollectDocument(String csproj, HashSet`1 source, List`1 metadataLocations, HashSet`1 globalUsings, IMagicOnionGeneratorLogger logger) in /home/runner/work/MagicOnion/MagicOnion/src/MagicOnion.GeneratorCore/Utils/PseudoCompilation.cs:line 395
   at MagicOnion.Generator.Utils.PseudoCompilation.CreateFromProjectAsync(String[] csprojs, String[] preprocessorSymbols, IMagicOnionGeneratorLogger logger, CancellationToken cancellationToken) in /home/runner/work/MagicOnion/MagicOnion/src/MagicOnion.GeneratorCore/Utils/PseudoCompilation.cs:line 25
   at MagicOnion.Generator.MagicOnionCompiler.GenerateFileAsync(String input, String output, Boolean disableAutoRegister, String namespace, String conditionalSymbol, String userDefinedFormattersNamespace, SerializerType serializerType) in /home/runner/work/MagicOnion/MagicOnion/src/MagicOnion.GeneratorCore/MagicOnionCompiler.cs:line 87
   at MagicOnion.Generator.Program.RunAsync(ConsoleAppContext ctx, String input, String output, Boolean noUseUnityAttr, Boolean disableAutoRegister, String namespace, String messagepackFormatterNamespace, String conditionalSymbol, Boolean verbose, SerializerType serializer) in /home/runner/work/MagicOnion/MagicOnion/src/MagicOnion.Generator/Program.cs:line 28
   at ConsoleAppFramework.WithFilterInvoker.RunCore(ConsoleAppContext _)
   at ConsoleAppFramework.WithFilterInvoker.InvokeAsync()
   at ConsoleAppFramework.ConsoleAppEngine.RunCore(Type type, MethodInfo methodInfo, Object instance, String[] args, Int32 argsOffset)
   at ConsoleAppFramework.ConsoleAppEngine.RunCore(Type type, MethodInfo methodInfo, Object instance, String[] args, Int32 argsOffset)
```

To collect this issue, Shared project should disable Central Package Management by adding following to csproj.

```xml
  <PropertyGroup>
    <!-- dotnet-moc not support ManagePackageVersionsCentrally, therefore disable on Shared project.-->
    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
  </PropertyGroup>
```

Then manage NuGet package versions directly in Shared csproj.

```xml
  <ItemGroup>
    <PackageReference Include="MagicOnion.Abstractions" Version="x.y.z" />
    <PackageReference Include="MessagePack" Version="xx.yy.zz" />
  </ItemGroup>
```


## Unity - How to install MagicOnion and related packages.

* gRPC: Download `grpc_unity_package.2.47.0-dev202204190851.zip` package from [Daily build of 2022/4/19](https://packages.grpc.io/archive/2022/04/67538122780f8a081c774b66884289335c290cbe-f15a2c1c-582b-4c51-acf2-ab6d711d2c59/index.xml), unzip and move `Grpc.Core` & `Grpc.Core.Api` folder to Unity's Assets/Plugins.
* MessagePack: Install `MessagePack.Unity.unitypackage` from [MessagePack-CSharp/Releases/v2.5.124](https://github.com/MessagePack-CSharp/MessagePack-CSharp/releases/tag/v2.5.124).
* MagicOnion: Install `MagicOnion.Client.Unity.unitypackage` from [MagicOnion/Releases/Ver.5.1.8].

> [!NOTE]: iOS `Assets/Plugins/Grpc.Core/runtimes/ios/libgrpc.a` and Andorid packages `Assets/Plugins/Grpc.Core/runtimes/android/PLATFORM/libgrpc_csharp_ext.so` are removed as they are over git limitation 100mb. You need strip them to keep in git.
>
> see: https://github.com/Cysharp/MagicOnion/tree/main?tab=readme-ov-file#stripping-debug-symbols-from-ioslibgrpca

## Unity - How to install MageicOnion Shared project to Unity by Pacakge Manager

Unity can reference Shared by Package Manager. You need to create `package.json` and `asmdef` manually, add MessagePack Generator & MagicOnion Generator, and adjust bin/obj output. Let's say Sahred csproj as `MagicOnionLab.Shared` and Unity project as `MagicOnionLab.Unity`. Then you need to do following.

1. Create `package.json`
  You need create package.json in `MagicOnionLab.Shared/package.json`. package.json defines `package name` and displa name. Make sure `displayName` should be same as asmdef definition.

  ```json
  {
    "name": "com.guitarrapc.magiconionlab.shared.unity",
    "version": "1.0.0",
    "displayName": "MagicOnionLab.Shared.Unity",
    "description": "MagicOnionLab.Shared.Unity",
    "unity": "2022.1"
  }
  ```

2. Create `asmdef`
  You need create asmdef in `MagicOnionLab.Shared/MagicOnionLab.Shared.asmdef`. asmdef defines `namd`, `references` and`autoReferenced`. Make sure `name` should match `displayName` in package.json. Also reference should resolve dependencies pacakges to MagicOnion and MessagePack, this is key point.

  ```json
  {
    "name": "MagicOnionLab.Shared.Unity",
    "references": [
        "MessagePack",
        "MessagePack.Annotations",
        "MagicOnion.Abstractions"
    ],
    "optionalUnityReferences": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": []
  }
  ```

3. Add MessagePack Generator & MagicOnion Generator
  You nedd add dotnet tools to generate MessagePack object and MagicOnion Client. You can add it by following commands.

  ```sh
  dotnet tool install MessagePack.Generator
  dotnet tool install MagicOnion.Generator
  ```

  Then modify Shared csproj `MagicOnionLab.Shared.csproj` to run MessagePackage Generator and MagicOnion Client Generator on build.

  ```xml
  <Target Name="RestoreLocalTools" BeforeTargets="GenerateMessagePack;GenerateMagicOnion">
    <Exec Command="dotnet tool restore" />
  </Target>

  <Target Name="GenerateMessagePack" AfterTargets="Build">
    <PropertyGroup>
      <_MessagePackGeneratorArguments>-i ./MagicOnionLab.Shared.csproj -o ../MagicOnionLab.Unity.Windows.Mono/Assets/Scripts/Generated/MessagePack.Generated.cs</_MessagePackGeneratorArguments>
    </PropertyGroup>
    <Exec Command="dotnet tool run mpc $(_MessagePackGeneratorArguments)" />
  </Target>

  <Target Name="GenerateMagicOnion" AfterTargets="Build">
    <PropertyGroup>
      <_MagicOnionGeneratorArguments>-i ./MagicOnionLab.Shared.csproj -o ../MagicOnionLab.Unity.Windows.Mono/Assets/Scripts/Generated/MagicOnion.Generated.cs</_MagicOnionGeneratorArguments>
    </PropertyGroup>
    <Exec Command="dotnet tool run dotnet-moc $(_MagicOnionGeneratorArguments)" />
  </Target>
  ```

5. Adjust bin/obj output

  bin/obj is special folder for .NET on build. However these folder conflict on reference Shared csproj in Unity. Therefore you need to adjust output folder to avoid conflict. You can do it by following.

  Add following to Shared csproj `MagicOnionLab.Shared.csproj`.

  ```xml
  <ItemGroup>
    <None Remove="**\package.json" />
    <None Remove="**\*.asmdef" />
    <None Remove="**\*.meta" />

    <Compile Remove="bin\**;obj\**" />
    <EmbeddedResource Remove="bin\**;obj\**" />
    <None Remove="bin\**;obj\**" />
  </ItemGroup>
  ```

  Add Directory.Build.props to `MagicOnionLab.Shared/Directory.Build.props`.

  ```xml
  <?xml version="1.0" encoding="utf-8"?>
  <Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <BaseIntermediateOutputPath>.output\obj\</BaseIntermediateOutputPath>
    <BaseOutputPath>.output\bin\</BaseOutputPath>
  </PropertyGroup>
  <ItemGroup>
    <None Remove=".output\**\**.*" />
    <None Remove="obj\**\*.*;bin\**\*.*" />
    <Compile Remove=".output\**\**.*" />
    <Compile Remove="bin\**\*.*;obj\**\*.*" />
    <EmbeddedResource Remove=".output\**\**.*" />
    <EmbeddedResource Remove="bin\**\*.*;obj\**\*.*" />
  </ItemGroup>
  </Project>
  ```
