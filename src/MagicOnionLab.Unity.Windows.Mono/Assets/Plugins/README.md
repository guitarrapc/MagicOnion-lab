# REAMDE

How to install MagicOnion and related packages.

* gRPC: Download `grpc_unity_package.2.47.0-dev202204190851.zip` package from [Daily build of 2022/4/19](https://packages.grpc.io/archive/2022/04/67538122780f8a081c774b66884289335c290cbe-f15a2c1c-582b-4c51-acf2-ab6d711d2c59/index.xml), unzip and move `Grpc.Core` & `Grpc.Core.Api` folder to Unity's Assets/Plugins.
* MessagePack: Install `MessagePack.Unity.unitypackage` from [MessagePack-CSharp/Releases/v2.5.124](https://github.com/MessagePack-CSharp/MessagePack-CSharp/releases/tag/v2.5.124).
* MagicOnion: Install `MagicOnion.Client.Unity.unitypackage` from [MagicOnion/Releases/Ver.5.1.8].

> [!NOTE]: iOS `Assets/Plugins/Grpc.Core/runtimes/ios/libgrpc.a` and Andorid packages `Assets/Plugins/Grpc.Core/runtimes/android/PLATFORM/libgrpc_csharp_ext.so` are removed as they are over git limitation 100mb. You need strip them to keep in git.
>
> see: https://github.com/Cysharp/MagicOnion/tree/main?tab=readme-ov-file#stripping-debug-symbols-from-ioslibgrpca
