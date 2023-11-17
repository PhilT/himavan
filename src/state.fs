[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.State
open System

let currentFolder state = state.folders[state.currentFolder]
let currentEmails state = state.emails[currentFolder state]

let update (ch: char) (state: State) =
  let keys = state.settings.keys

  let actions = Map.ofList [
    keys["down"], ((fun s -> s.currentEmail < (currentEmails state).Count - 1), (fun s ->
      { s with currentEmail = s.currentEmail + 1 }
    ))
    keys["up"], ((fun s -> s.currentEmail > 0), (fun s ->
      { s with currentEmail = s.currentEmail - 1 }
    ))
  ]

  if actions.ContainsKey(ch) then
    let pred, action = actions[ch]
    if (pred state) then (action state) else state
  else
    state
