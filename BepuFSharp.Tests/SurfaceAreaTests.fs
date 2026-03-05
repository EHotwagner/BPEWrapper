module BepuFSharp.Tests.SurfaceAreaTests

open System.IO
open Expecto

let private projectDir =
    let rec find dir =
        if File.Exists(Path.Combine(dir, "BepuFSharp.slnx")) then dir
        else find (Directory.GetParent(dir).FullName)
    find (Directory.GetCurrentDirectory())

let private fsiDir = Path.Combine(projectDir, "BepuFSharp")
let private baselineDir = Path.Combine(projectDir, "BepuFSharp.Tests", "baselines")

let private compareModule name =
    testCase (sprintf "surface area of %s matches baseline" name) <| fun _ ->
        let fsiPath = Path.Combine(fsiDir, sprintf "%s.fsi" name)
        let baselinePath = Path.Combine(baselineDir, sprintf "%s.baseline" name)
        Expect.isTrue (File.Exists fsiPath) (sprintf "%s.fsi should exist" name)
        Expect.isTrue (File.Exists baselinePath) (sprintf "%s.baseline should exist" name)
        let fsiContent = File.ReadAllText(fsiPath).ReplaceLineEndings("\n").Trim()
        let baselineContent = File.ReadAllText(baselinePath).ReplaceLineEndings("\n").Trim()
        Expect.equal fsiContent baselineContent
            (sprintf "%s.fsi has diverged from baseline. Update baseline if intentional." name)

[<Tests>]
let tests = testList "SurfaceArea" [
    compareModule "Types"
    compareModule "Diagnostics"
    compareModule "Shapes"
    compareModule "Bodies"
    compareModule "Constraints"
    compareModule "PhysicsWorld"
]
