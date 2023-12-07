[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.State
open System

let update (action: string) (state: State) (agent: MailboxProcessor<Msg>) =
  let keys = state.settings.keys
  let emails = Email.currentList state

  let actions = Map.ofList [
    "down", (
      (fun s -> s.currentEmail < emails.Length - 1),
      (fun s -> agent.Post(SetCurrentEmail(s.currentEmail + 1)) )
    )
    "up", (
      (fun s -> s.currentEmail > 0),
      (fun s -> agent.Post(SetCurrentEmail(s.currentEmail - 1)) )
    )
    "bottom", (
      (fun s -> emails.Length > 0),
      (fun s -> agent.Post(SetCurrentEmail(emails.Length - 1 )) )
    )
    "top", (
      (fun s -> emails.Length > 0),
      (fun s -> agent.Post(SetCurrentEmail(0)) )
    )
    "next_folder", (
      (fun s -> s.currentFolder < state.folders.Length - 1),
      (fun s -> agent.Post(SetCurrentFolder(s.currentFolder + 1, agent)) )
    )
    "prev_folder", (
      (fun s -> s.currentFolder > 0),
      (fun s -> agent.Post(SetCurrentFolder(s.currentFolder - 1, agent)) )
    )
    "delete", (
      (fun s -> emails.Length > 0),
      (fun s ->
        Email.runFuncAndFetchList (fun id folder ->
          MailService.delete id folder) agent s
      )
    )
    "archive", (
      (fun s -> emails.Length > 0),
      (fun s ->
        Email.runFuncAndFetchList (fun id folder ->
          MailService.mv id folder "Archive") agent s
      )
    )
    "spam", (
      (fun s -> emails.Length > 0),
      (fun s ->
        Email.runFuncAndFetchList (fun id folder ->
          MailService.mv id folder "Spam") agent s
      )
    )
    "read", (
      (fun s -> emails.Length > 0),
      (fun s -> Email.read agent s)
    )
    "write", (
      (fun s -> true),
      (fun s -> agent.Post(WriteEmail))
    )
    "back", (
      (fun s -> Nav.isReading s),
      (fun s -> agent.Post(Back))
    )
    "select", (
      (fun s -> emails.Length > 0),
      (fun s -> agent.Post(Select(s.currentEmail)))
    )
    "show_addr", (
      (fun s -> emails.Length > 0),
      (fun s -> agent.Post(ShowAddress))
    )
    "quit", (
      (fun s -> true),
      (fun s -> agent.Post(Quit))
    )
  ]

  let pred, action = actions[action]
  if (pred state) then (action state)
