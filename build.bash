declare -a arr=(
    fsharpc
    --nologo
    -a
    -r:$(dirname $(which sdb))/../lib/sdb/sdb.exe
    -r:./sdbPlugins/packages/FSharp.Control.Reactive/lib/net45/FSharp.Control.Reactive.dll
    -r:./sdbPlugins/packages/System.Reactive.Core/lib/net46/System.Reactive.Core.dll
    -r:./sdbPlugins/packages/System.Reactive.Linq/lib/net46/System.Reactive.Linq.dll
    -r:./sdbPlugins/packages/System.Reactive.Interfaces/lib/net45/System.Reactive.Interfaces.dll
    -r:./sdbPlugins/packages/System.Reactive.PlatformServices/lib/net46/System.Reactive.PlatformServices.dll
    ./sdbPlugins/sdbfs.fsx
    --out:$HOME/.sdb/test.dll
)

${arr[@]}
cp ./sdbPlugins/packages/FSharp.Control.Reactive/lib/net45/FSharp.Control.Reactive.dll $HOME/.sdb/
cp ./sdbPlugins/packages/System.Reactive.Core/lib/net46/System.Reactive.Core.dll $HOME/.sdb/
cp ./sdbPlugins/packages/System.Reactive.Linq/lib/net46/System.Reactive.Linq.dll $HOME/.sdb/
cp ./sdbPlugins/packages/System.Reactive.Interfaces/lib/net45/System.Reactive.Interfaces.dll $HOME/.sdb/
cp ./sdbPlugins/packages/System.Reactive.PlatformServices/lib/net46/System.Reactive.PlatformServices.dll $HOME/.sdb/



