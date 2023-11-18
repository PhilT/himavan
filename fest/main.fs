module Fest

open System
open System.Numerics
open System.Reflection

type TestOutcome = {
  expected: string
  actual: string
  passed: bool
}

type TestResult = {
  description: string
  expected: string
  actual: string
  passed: bool
}

type Test = {
  description: string
  testFunc: unit -> TestOutcome
}

let test description testFunc =
  {
    description = description
    testFunc = testFunc
  }

let assertTrue actual =
  {
    expected = sprintf "%A" true
    actual = sprintf "%A" actual
    passed = actual = true
  }

let assertFalse actual =
  {
    expected = sprintf "%A" false
    actual = sprintf "%A" actual
    passed = actual = false
  }

let assertEqual expected actual =
  {
    expected = sprintf "%A" expected
    actual = sprintf "%A" actual
    passed = actual = expected
  }


let MAX_POSITION = 70
let assertPlot expected actual =
  let graph = [
    Array.create MAX_POSITION " "
    Array.create MAX_POSITION " "
  ]
  let toHex (i: int) = System.Convert.ToString(i, 16).ToUpper()
  let plotActual =
    actual
    |> List.iteri (fun i (vector: Vector3) ->
      [vector.X; vector.Y]
      |> List.iteri (fun v axis ->
        let position = int (System.Math.Round(decimal axis, 0))
        if position >= 0 && position < MAX_POSITION then
          graph[v][position] <- toHex i
      )
    )

    graph |> List.map (fun a -> (a |> String.concat "").TrimEnd())

  assertEqual expected plotActual


let runAll tests =
  tests |> List.map (fun test ->
    let result = test.testFunc ()
    {
      description = test.description
      expected = result.expected
      actual = result.actual
      passed = result.passed
    }
  )


let private failureMessages tests = List.filter (fun test -> test.passed = false) tests
let failureCount tests = failureMessages tests |> List.length
let summary seed result =
  let failureCount = failureCount result
  let status = if failureCount > 0 then "FAILED" else "PASSED"
  $"{status}! {List.length result} tests ran, {failureCount} failures. Seed: {seed}"

let expectationMessages (results: TestResult list) =
  (results |> List.map (fun result ->
    $"    {result.description}\n" +
    $"      Expected: '{result.expected}'\n" +
    $"        Actual: '{result.actual}'\n"
  ) |> String.concat "\n")


let failures results =
  let failures = failureMessages results
  if (failureCount results) > 0 then
    "  Failures:\n" + expectationMessages failures
  else
    ""


let documentation (results: TestResult list) =
  "  Tests:\n" + expectationMessages results


let shuffle (rand: System.Random) items =
  let ary = Array.ofList items
  let Swap i j =
      let item = ary[i]
      ary[i] <- ary[j]
      ary[j] <- item
  let ln = ary.Length
  [0..(ln - 2)]
  |> Seq.iter (fun i -> Swap i (rand.Next(i, ln)))
  ary
  |> List.ofArray


let testsFrom (assembly: Assembly) =
  assembly.GetTypes()
  |> Seq.map (fun t ->
    t.GetMethods()
    |> Array.tryFind(fun m -> m.Name = "get_tests" && m.IsPublic && m.IsStatic)
  )
  |> Seq.choose id
  |> Seq.map (fun methodInfo -> methodInfo.Invoke(null, [||]))
  |> Seq.filter (fun f -> f <> null)
  |> Seq.map (fun o -> o :?> list<(Test)>)
  |> Seq.concat
  |> List.ofSeq


let run (assembly: Assembly) testsPassedIn seed =
  let seed =
    match seed with
    | Some s -> s
    | None -> int System.DateTime.Now.Ticks &&& 0x0000ffff

  let tests =
    match testsPassedIn with
    | Some t -> t
    | None -> testsFrom assembly

  let results = tests |> shuffle (System.Random(seed)) |> runAll
  summary seed results |> Display.text
  failures results |> Display.text

