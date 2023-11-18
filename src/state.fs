[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.State
open System

let currentFolder state = state.folders[state.currentFolder]
let currentEmails state =
  if state.emails.ContainsKey(currentFolder state) then
    state.emails[currentFolder state]
  else
    Map.empty


let fetchEmails folder (agent: MailboxProcessor<Msg>) =
  async {
    let emails = Mail.list folder (Con.height() - Renderer.All.FIRST_EMAIL_LINE - 1)
    agent.Post(NewEmails(folder, emails))
  } |> Async.Start


let update (ch: char) (state: State) agent =
  let keys = state.settings.keys

  let actions = Map.ofList [
    keys["down"], ((fun s -> s.currentEmail < (currentEmails state).Count - 1), (fun s ->
      { s with currentEmail = s.currentEmail + 1 }
    ))
    keys["up"], ((fun s -> s.currentEmail > 0), (fun s ->
      { s with currentEmail = s.currentEmail - 1 }
    ))
    keys["next_folder"], ((fun s -> s.currentFolder < state.folders.Length - 1), (fun s ->
      let newState = { s with currentFolder = s.currentFolder + 1 }
      fetchEmails (currentFolder newState) agent |> ignore
      newState
    ))
    keys["prev_folder"], ((fun s -> s.currentFolder > 0), (fun s ->
      let newState = { s with currentFolder = s.currentFolder - 1 }
      fetchEmails (currentFolder newState) agent |> ignore
      newState
    ))
  ]

  if actions.ContainsKey(ch) then
    let pred, action = actions[ch]
    if (pred state) then (action state) else state
  else
    state
