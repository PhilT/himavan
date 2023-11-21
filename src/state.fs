[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.State
open System
open System.Text.Json

let jsonToString (str: string) = JsonSerializer.Deserialize<string> str
let currentFolder state = state.folders[state.currentFolder]
let currentEmails state =
  if state.emails.ContainsKey(currentFolder state) then
    Mail.asList state.emails[currentFolder state] state.windowHeight
  else
    []


let resetCurrentEmail state =
  let emailCount = max 0 (currentEmails state).Length - 1
  { state with currentEmail = min emailCount state.currentEmail }


let fetchEmails (agent: MailboxProcessor<Msg>) state =
  let folder = currentFolder state
  async {
    let emails = Mail.list folder (state.windowHeight - FIRST_EMAIL_START_Y - 1)
    agent.Post(NewEmails(folder, emails))
  } |> Async.Start
  state


let doEmail (action: string -> string -> ProcessResult) (agent: MailboxProcessor<Msg>) state =
  let folder = currentFolder state
  let emails = currentEmails state
  let id = emails[state.currentEmail].id
  async {
    let response = action id folder
    if response.exitCode = 0 then
      agent.Post(Notice(jsonToString response.out))
      fetchEmails agent state |> ignore
    else
      agent.Post(Error(jsonToString response.out))

  } |> Async.Start
  state


let addBody folder email body state =
  let emails = state.emails[currentFolder state]
  let emailsForFolder = Map.add email.id { email with body = body } emails
  let emails = Map.add folder emailsForFolder state.emails
  { state with emails = emails }


let getCurrentEmail state =
  let folder = currentFolder state
  let emails = currentEmails state
  emails[state.currentEmail]


let readEmail (agent: MailboxProcessor<Msg>) state =
  let folder = currentFolder state
  let emails = currentEmails state
  let email = emails[state.currentEmail]
  agent.Post(Opening(email.id))
  let state =
    if email.body = null then
      let response = Mail.read email.id folder
      if response.exitCode = 0 then
        addBody folder email (jsonToString response.out) state
      else
        agent.Post(Error(response.err))
        state
    else
      state


  let emailBody = (getCurrentEmail state).body

  if emailBody <> null then
    agent.Post(ReadEmail(emailBody))
    { state with nav = Nav.OPEN }
  else
    state


let update (ch: char) (state: State) agent =
  let keys = state.settings.keys
  let emails = currentEmails state

  let actions = Map.ofList [
    keys["down"], (
      (fun s -> s.currentEmail < emails.Length - 1),
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
    keys["delete"], (
      (fun s -> emails.Length > 0),
      (fun s -> doEmail (fun id folder -> Mail.delete id folder) agent s)
    )
    keys["archive"], (
      (fun s -> emails.Length > 0),
      (fun s -> doEmail (fun id folder -> Mail.mv id folder "Archive") agent s)
    )
    keys["spam"], (
      (fun s -> emails.Length > 0),
      (fun s -> doEmail (fun id folder -> Mail.mv id folder "Spam") agent s)
    )
    keys["read"], (
      (fun s -> emails.Length > 0),
      (fun s -> readEmail agent s)
    )
    keys["back"], (
      (fun s -> s.nav = Nav.OPEN),
      (fun s -> { s with nav = Nav.LIST })
    )
  ]

  if actions.ContainsKey(ch) then
    let pred, action = actions[ch]
    if (pred state) then (action state) else state
  else
    state
