module Himavan.Mail

open System
open System.Diagnostics
open System.Globalization
open System.IO
open System.Text
open System.Text.Json
open Wcwidth

let SEPARATOR = "│"
let MAIL_PROG = "himalaya"
let HIMALAYA_DRAFT_PATH = "/tmp/himalaya-draft.eml"

let editor = System.Environment.GetEnvironmentVariable("EDITOR")

type ProcessResult = {
  exitCode: int
  out: string
  err: string
}


let runCmd exe args (stdin: string option) =
  Logger.write "Mail" "runCmd" $"{exe} {args} {stdin}"

  let p = new Process()
  p.StartInfo.UseShellExecute <- false
  p.StartInfo.FileName <- exe
  p.StartInfo.CreateNoWindow <- true
  p.StartInfo.Arguments <- args
  p.StartInfo.RedirectStandardOutput <- true
  p.StartInfo.RedirectStandardError <- true
  p.StartInfo.RedirectStandardInput <- stdin.IsSome
  if not (p.Start()) then failwith $"Unable to start process {exe} {args}"

  let output = new StringBuilder()
  let error = new StringBuilder()

  if stdin.IsSome then
    p.StandardInput.Write(stdin.Value)
    p.StandardInput.Close()

  p.OutputDataReceived.Add(fun x -> output.AppendLine(x.Data) |> ignore)
  p.ErrorDataReceived.Add(fun x -> error.AppendLine(x.Data) |> ignore)
  p.BeginOutputReadLine()
  p.BeginErrorReadLine()

  p.WaitForExit()

  { exitCode = p.ExitCode; out = output.ToString(); err = error.ToString() }


let runHim cmd folder ids options (stdin: string option) =
  let args = [cmd; "-o"; "json"]
  let args = if folder <> "" then (List.append args ["-f"; folder]) else args
  let args = List.concat [args; options; ids]
  let args = (String.concat " " args)

  runCmd MAIL_PROG args stdin


let folders () =
  let response = runHim "folders" "" [] [] None

  //TODO: Handle response.exitCode <> 0
  JsonSerializer.Deserialize<Folder list> response.out
  |> List.map (fun folder -> folder.name)
  |> List.rev


let calculateCharWidths text =
  let en = StringInfo.GetTextElementEnumerator(text)

  let rec loop totalWidth (lst: int list) =
    let another = en.MoveNext()
    if another then
      let ch = en.GetTextElement()
      let width = UnicodeCalculator.GetWidth(Char.ConvertToUtf32(ch, 0))
      loop (totalWidth + width) (width :: lst)
    else
      totalWidth, List.rev lst

  loop 0 List.empty


let list folder limit =
  Logger.write "Mail" "list" $"{folder} {limit}"
  let response = runHim "list" folder [] ["-s"; $"{limit}"] None

  //TODO: Handle response.exitCode <> 0
  JsonSerializer.Deserialize<Email list> response.out
  |> List.fold (fun state email ->
    let subjectTotalWidth, subjectCharWidths = calculateCharWidths email.subject
    let email = {
      email with
        subjectCharWidths = subjectCharWidths
        subjectTotalWidth = subjectTotalWidth
        from = {
          name = if email.from.name = null then "" else email.from.name
          addr = if email.from.addr = null then "" else email.from.addr
        }
    }
    Map.add email.id email state
  ) Map.empty


let send path =
  let draft = File.ReadAllText(path)
  runHim "send" "" [] [] (Some draft)


let write () =
  let response = runHim "template" "" [] ["write"] None
  if response.exitCode = 0 then
    File.WriteAllText(HIMALAYA_DRAFT_PATH, response.out)
    runCmd editor HIMALAYA_DRAFT_PATH None
  else
    response


let mv id src dest = runHim "move" src [id] [dest] None
let delete id folder = runHim "delete" folder [id] [] None
let read id folder = runHim "read" folder [id] [] None

