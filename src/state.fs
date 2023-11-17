[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.State
open System

let currentFolder state = state.folders[state.currentFolder]
let currentEmails state =
  if state.emails.ContainsKey(currentFolder state) then
    state.emails[currentFolder state]
  else
    Map.empty

let update (ch: char) (state: State) =
  let keys = state.settings.keys

  let actions = Map.ofList [
    keys["down"], ((fun s -> s.currentEmail < (currentEmails state).Count - 1), (fun s ->
      { s with currentEmail = s.currentEmail + 1 }
    ))
    keys["up"], ((fun s -> s.currentEmail > 0), (fun s ->
      { s with currentEmail = s.currentEmail - 1 }
    ))
    keys["next_folder"], ((fun s -> s.currentFolder < state.folders.Length - 1), (fun s ->
      let s = { s with currentFolder = s.currentFolder + 1 }
      let emails = Mail.list (currentFolder s) (Con.height() - Renderer.FIRST_EMAIL_LINE - 1)
      { s with
          emails = Map.add (currentFolder s) emails s.emails
      }
    ))
    keys["prev_folder"], ((fun s -> s.currentFolder > 0), (fun s ->
      let s = { s with currentFolder = s.currentFolder - 1 }
      let emails = Mail.list (currentFolder s) (Con.height() - Renderer.FIRST_EMAIL_LINE - 1)
      { s with
          emails = Map.add (currentFolder s) emails s.emails
      }
    ))
  ]

  if actions.ContainsKey(ch) then
    let pred, action = actions[ch]
    if (pred state) then (action state) else state
  else
    state
