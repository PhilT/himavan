namespace Himavan

type Address = {
  name: string
  addr: string
}

type Email = {
  id: string
  flags: string list
  subject: string
  subjectCharWidths: int list
  from: Address
  date: string
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

type State = {
  settings: Settings
  folders: Folders
  emails: Map<FolderName, Emails>
  currentFolder: int
  currentEmail: int
}

type Msg =
  | Update of char
  | NewEmails of FolderName * Emails
  | Fetch of AsyncReplyChannel<State>

