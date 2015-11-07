// include Fake lib
#I "packages/FAKE/tools/"
#r "FakeLib.dll"
open Fake

// Properties
let buildDir = "." </> "build" </> ""

// Targets
Target "Clean" (fun _ ->
    CleanDir buildDir
)

Target "Build" (fun _ ->
    !! "**/*.fsproj"
      |> MSBuildRelease buildDir "Build"
      |> Log "AppBuild-Output: "
)

Target "Default" (fun _ -> ())

// Dependencies
"Clean" ==> "Build" ==> "Default"

// start build
RunTargetOrDefault "Default"