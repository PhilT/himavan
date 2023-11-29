[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.Renderer.All
open System

open Himavan


let textWithColors text (fg, bg) = text, fg, bg

let setup () =
  Con.setup ()


let teardown () =
  Con.teardown ()

let update (state: State) emails =
  try
    Folders.render state.currentFolder state.folders

    state.nav
    |> Set.contains Nav.ADDR
    |> Renderer.Email.render state.currentEmail state.selectedEmailIds emails
  with
    | e ->
      Renderer.StatusLine.error $"ERROR: {e.Message} Turn on logging and check the log file."
      Logger.write "Renderer.All" "update" e.Message
      Logger.write "Renderer.All" "update" e.StackTrace
