[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.Email
open System
open System.Text.Json

let jsonToString (str: string) = JsonSerializer.Deserialize<string> str
let currentFolder state = state.folders[state.currentFolder]
let currentList state =
  if state.emails.ContainsKey(currentFolder state) then
    MailService.asList state.emails[currentFolder state] state.windowHeight
  else
    []


let resetCurrent state =
  let emailCount = max 0 (currentList state).Length - 1
  { state with currentEmail = min emailCount state.currentEmail }


let fetchList (agent: MailboxProcessor<Msg>) state =
  let folder = currentFolder state
  async {
    let emails = MailService.list folder (state.windowHeight - FIRST_EMAIL_START_Y - 1)
    agent.Post(NewEmails(folder, emails))
  } |> Async.Start
  state


let runFuncAndFetchList (action: string -> string -> ProcessResult) (agent: MailboxProcessor<Msg>) state =
  let folder = currentFolder state
  let emails = currentList state
  let ids =
    if Set.count state.selectedEmailIds > 0 then
      state.selectedEmailIds |> Set.toList
    else
      [emails[state.currentEmail].id]

  async {
    let response = action (String.Join(" ", ids)) folder
    if response.exitCode = 0 then
      agent.Post(Notice(jsonToString response.out))
      fetchList agent state |> ignore
    else
      agent.Post(Error(jsonToString response.out))

  } |> Async.Start
  state


let addBody folder email (body: string) state =
  let emails = state.emails[currentFolder state]
  let body =
    body.Replace("\r", "").Split("\n")
    |> Array.map(fun line -> line.Trim())
    |> (fun a -> String.Join("\n", a).Replace("\n\n\n", "\n\n"))
  let emailsForFolder = Map.add email.id { email with body = body } emails
  let emails = Map.add folder emailsForFolder state.emails
  { state with emails = emails }


let getCurrent state =
  let folder = currentFolder state
  let emails = currentList state
  emails[state.currentEmail]


let read (agent: MailboxProcessor<Msg>) state =
  let folder = currentFolder state
  let emails = currentList state
  let email = emails[state.currentEmail]
  agent.Post(Opening(email.id))
  let state =
    if email.body = null then
      let response = MailService.read email.id folder
      if response.exitCode = 0 then
        addBody folder email (jsonToString response.out) state
      else
        agent.Post(Error(response.err))
        state
    else
      state


  let emailBody = (getCurrent state).body

  if emailBody <> null then
    agent.Post(ReadEmail(emailBody))
    { state with nav = state.nav |> Set.add Nav.OPEN |> Set.remove Nav.LIST }
  else
    state


let toggle id emails =
  if emails |> Set.contains id then
    emails |> Set.remove id
  else emails |> Set.add id

