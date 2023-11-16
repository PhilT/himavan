open Mindmagma.Curses
type nc = NCurses

type CursesLibraryNamesW() =
  inherit CursesLibraryNames()
  override this.ReplaceLinuxDefaults = true;
  override this.NamesLinux = ResizeArray<string> [ "libncursesw.so" ]

let QUIT = 'q'
let LIST_DOWN = 'j'
let LIST_UP = 'k'
let LIST_DELETE = 'D'
let LIST_READ = '\n'
let LIST_ARCHIVE = 'A'
let LIST_SPAM = 'S'
let LIST_WRITE = 'w'
let FOLDER_PREV = 'h'
let FOLDER_NEXT = 'l'


type Email = {
  id: string
  flags: string
  subject: string
  from: string
  date: string
}


let render_email screen (email: Email) (highlight: bool) (columns: int list) =
  let fields = [
    email.id
    email.flags
    email.subject
    email.from
    email.date
  ]
  Screen.putline screen fields highlight columns


let render_list screen (emails: Email list) (columns: int list) =
  let emailsToDisplay = nc.Lines - Screen.FIRST_EMAIL_ROW |> max 0

  emails
  |> List.take (emailsToDisplay |> min emails.Length)
  |> List.iteri (fun i email ->
    nc.Move(i + Screen.FIRST_EMAIL_ROW, 0) |> ignore
    render_email screen email false columns
  )

  nc.ClearToBottom()


let lines_to_emails (lines: string list) =
  lines
  |> List.map (fun line -> line.Split(Mail.SEPARATOR))
  |> List.filter (fun fields -> fields.Length = 5)
  |> List.map (fun fields ->
    {
      id = fields[0].Trim()
      flags = fields[1].Trim()
      subject = fields[2].Trim()
      from = fields[3].Trim()
      date = fields[4].Trim()
    }
  )


let fetch_emails (folder: string) =
  let response = Mail.list folder nc.Columns (nc.Lines - Screen.FIRST_EMAIL_ROW)
  //failwith(sprintf "%A" response)
  let lines = response.out.Split("\n") |> List.ofArray |> List.skip 2
  let headings = lines[0].Split(Mail.SEPARATOR) |> List.ofArray

  (headings, lines_to_emails(lines))


let columns_from row =
  let mutable i = 0
  let columns =
    row
    |> List.map (fun column ->
      i <- i + 1 + (String.length column)
      i - 1
    )

  columns |> List.insertAt 0 0


printfn $"Checking UTF8: {Screen.FLAGGED_IMPORTANT}"
System.Console.ReadKey() |> ignore

let exceptions =
  try
    let screen = Screen.setup ()
    let _, folders = Mail.folders ()
    Screen.renderFolders screen folders 0
    let headings, emails = fetch_emails "INBOX"
    let columns = columns_from headings
    render_list screen emails columns


    let ch = nc.GetChar()

    System.Exception("none")
  with
  | e -> e

Screen.teardown ()

if exceptions.Message <> "none" then
  if exceptions.InnerException <> null then
    printfn "%A" exceptions.InnerException

  printfn "%A" (exceptions.ToString())
  printfn "%A" exceptions.StackTrace
