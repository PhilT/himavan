[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.State
open System

let update (action: string) (state: State) agent =
  let keys = state.settings.keys
  let emails = Email.currentList state

  let actions = Map.ofList [
    "down", (
      (fun s -> s.currentEmail < emails.Length - 1),
      (fun s -> { s with currentEmail = s.currentEmail + 1 })
    )
    "up", (
      (fun s -> s.currentEmail > 0),
      (fun s -> { s with currentEmail = s.currentEmail - 1 })
    )
    "next_folder", (
      (fun s -> s.currentFolder < state.folders.Length - 1),
      (fun s ->
        { s with currentFolder = s.currentFolder + 1 }
        |> Email.resetCurrent
        |> Email.fetchList agent
      )
    )
    "prev_folder", (
      (fun s -> s.currentFolder > 0),
      (fun s ->
        { s with currentFolder = s.currentFolder - 1 }
        |> Email.resetCurrent
        |> Email.fetchList agent
      )
    )
    "bottom", (
      (fun s -> emails.Length > 0),
      (fun s -> { s with currentEmail = emails.Length - 1 })
    )
    "top", (
      (fun s -> emails.Length > 0),
      (fun s -> { s with currentEmail = 0 })
    )
    "delete", (
      (fun s -> emails.Length > 0),
      (fun s -> Email.runFunc (fun id folder -> Mail.delete id folder) agent s)
    )
    "archive", (
      (fun s -> emails.Length > 0),
      (fun s -> Email.runFunc (fun id folder -> Mail.mv id folder "Archive") agent s)
    )
    "spam", (
      (fun s -> emails.Length > 0),
      (fun s -> Email.runFunc (fun id folder -> Mail.mv id folder "Spam") agent s)
    )
    "read", (
      (fun s -> emails.Length > 0),
      (fun s -> Email.read agent s)
    )
    "back", (
      (fun s -> s.nav |> Set.contains Nav.OPEN),
      (fun s -> { s with nav = s.nav |> Set.add Nav.LIST |> Set.remove Nav.OPEN })
    )
    "select", (
      (fun s -> emails.Length > 0),
      (fun s -> { s with selectedEmailIds = Email.toggle emails[s.currentEmail].id s.selectedEmailIds })
    )
    "show_addr", (
      (fun s -> emails.Length > 0),
      (fun s ->
        if s.nav |> Set.contains Nav.ADDR then
          { s with nav = s.nav |> Set.remove Nav.ADDR }
        else
          { s with nav = s.nav |> Set.add Nav.ADDR }
      )
    )
    "quit", (
      (fun s -> true),
      (fun s -> agent.Post(Quit); { s with nav = Set.ofList [Nav.QUITING] })
    )
  ]

  let pred, action = actions[action]
  if (pred state) then (action state) else state
