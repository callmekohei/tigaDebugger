declare -a arr=(
    fsharpc
    --nologo
    -a
    -r:$(dirname $(which sdb))/../lib/sdb/sdb.exe
    ./sdbPlugins/sdbfs.fsx
    --out:$HOME/.sdb/test.dll
)

${arr[@]}
