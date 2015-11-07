// include Fake lib
#I "packages/FAKE/tools/"
#r "FakeLib.dll"
open Fake

//System.Environment.CurrentDirectory <- """C:\Users\alexchilcott\Desktop\automator"""

// Properties
let buildDir = "./build/"
let tempDir = combinePaths buildDir "temp"

// Targets
Target "Clean" (fun _ ->
    CleanDir buildDir
)

Target "BuildApp" (fun _ ->
    !! "**/*.fsproj"
      |> MSBuildRelease tempDir "Build"
      |> Log "AppBuild-Output: "
)

Target "IlMerge" (fun _ -> 
    ILMerge
        (fun x -> {x with TargetKind = TargetKind.Exe; Libraries = System.IO.Directory.EnumerateFiles(tempDir, "*.dll")})
        (combinePaths buildDir "Autonote-merged.exe")
        (combinePaths tempDir "Autonote.exe")
)

Target "Tidy" (fun _ -> 
    DeleteDir tempDir
)

Target "Default" (fun _ ->
    trace "Hello World from FAKE"
)

// Dependencies
"Clean" ==> "BuildApp" ==> "IlMerge" ==> "Tidy" ==> "Default"


// start build
RunTargetOrDefault "Default"
