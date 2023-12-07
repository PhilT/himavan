[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.Agent

let rec create (state: State) =
  MailboxProcessor.Start(fun inbox ->
    let rec loop state =
      async {
        Con.moveTo 0 0

        let! msg = inbox.Receive()
        match msg with
        | SetCurrentEmail(index) ->
          return! loop { state with currentEmail = index }
        | SetCurrentFolder(index, agent) ->
          let newState =
            { state with currentFolder = index }
            |> Nav.list
          Email.fetchList agent newState
          return! loop newState
        | Update ->
          if state |> Nav.onList then
            Renderer.All.update state (Email.currentList state)
          return! loop state
        | Opening(emailId) ->
          Renderer.StatusLine.notice $"Opening email {emailId}"
          Renderer.Body.prepare ()
          return! loop state
        | WriteEmail ->
          Con.teardown ()
          MailService.write ()
          // if success then write to HIMALAYA_DRAFT_PATH
          // then spawn text editor
          // if zero returned then send email using HIMALAYA_DRAFT_PATH
        | ReadEmail(body) ->
          Renderer.Body.render body
          return! loop (state |> Nav.read)
        | Back ->
          return! loop (state |> Nav.list)
        | Select(index) ->
          let emails = Email.currentList state
          let currentId = emails[state.currentEmail].id
          return! loop {
            state with
              selectedEmailIds =
                Email.toggle currentId state.selectedEmailIds
          }
        | ShowAddress ->
          let state =
            if Nav.isShowingAddress state then
              Nav.hideAddress state
            else
              Nav.showAddress state
          return! loop state
        | Info(message) ->
          Renderer.StatusLine.info message
          return! loop state
        | Notice(message) ->
          Renderer.StatusLine.notice message
          return! loop state
        | Error(message) ->
          Renderer.StatusLine.error message
          return! loop state
        | NewEmails(folder, emails) ->
          let newState =
            { state with emails = Map.add folder emails state.emails }
            |> Email.resetCurrent
          Renderer.All.update newState (Email.currentList newState)
          return! loop newState
        | Quit ->
          Renderer.All.teardown ()
          return! loop { state with nav = Set.ofList [Nav.QUITING] }
        | Fetch(channel) ->
          channel.Reply(state)
          return! loop state
      }

    loop state
  )

