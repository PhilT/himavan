[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.State
open System

let currentFolder state = state.folders[state.currentFolder]
let currentEmails state =
  if state.emails.ContainsKey(currentFolder state) then
    state.emails[currentFolder state]
  else
    Map.empty


let resetCurrentEmail state =
  let emailCount = max 0 ((currentEmails state).Count - 1)
  {
    state with
      currentEmail = min emailCount state.currentEmail
  }

let fetchEmails (agent: MailboxProcessor<Msg>) state =
  let folder = currentFolder state
  async {
    let emails = Mail.list folder (Con.height() - Renderer.Email.FIRST_EMAIL_START_Y - 1)
    agent.Post(NewEmails(folder, emails))
  } |> Async.Start
  state


let update (ch: char) (state: State) agent =
  let keys = state.settings.keys
  let emails = currentEmails state

  let actions = Map.ofList [
    keys["down"], (
      (fun s -> s.currentEmail < emails.Count - 1),
      (fun s -> { s with currentEmail = s.currentEmail + 1 })
    )
    keys["up"], (
      (fun s -> s.currentEmail > 0),
      (fun s -> { s with currentEmail = s.currentEmail - 1 })
    )
    keys["next_folder"], (
      (fun s -> s.currentFolder < state.folders.Length - 1),
      (fun s ->
        { s with currentFolder = s.currentFolder + 1 }
        |> resetCurrentEmail
        |> fetchEmails agent
      )
    )
    keys["prev_folder"], (
      (fun s -> s.currentFolder > 0),
      (fun s ->
        { s with currentFolder = s.currentFolder - 1 }
        |> resetCurrentEmail
        |> fetchEmails agent
      )
    )
  ]

  if actions.ContainsKey(ch) then
    let pred, action = actions[ch]
    if (pred state) then (action state) else state
  else
    state
