namespace Himavan

type Address = {
  name: string
  addr: string
}

type EmailIndexOf =
  | ID = 0
  | FLAGS = 1
  | SUBJECT = 2
  | FROM = 3
  | DATE = 4


type Email = {
  id: string
  flags: string list
  subject: string
  subjectCharWidths: int list
  from: Address
  date: string
  body: string
}

type Folder = {
  name: string
  desc: string
}

type Color = System.ConsoleColor
type Emails = Map<string, Email>
type FolderName = string
type Folders = List<FolderName>
type Keys = Map<string, char>

type Column = {
  name: string
  color: Color
}

type Colors = {
  selected: Color * Color
  normal: Color * Color
}

type Settings = {
  keys: Keys
  general: Map<string, string>
  debug: Map<string, string>
}

type Nav =
  | LIST
  | OPEN

type State = {
  settings: Settings
  folders: Folders
  emails: Map<FolderName, Emails>
  currentFolder: int
  currentEmail: int
  windowHeight: int
  nav: Nav
}

type Msg =
  | Update of State
  | Notice of string
  | Error of string
  | Opening of string
  | ReadEmail of string
  | NewEmails of FolderName * Emails
  | Fetch of AsyncReplyChannel<State>

type ProcessResult = {
  exitCode: int
  out: string
  err: string
}


