#!/usr/bin/env bash

mono ./.paket/paket.bootstrapper.exe
mono ./.paket/paket.exe install
mono ./packages/FSharp.Compiler.Tools/tools/fsi.exe ./build.fsx
