namespace Himavan
open System

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
  body: string
}

type Folder = {
  name: string
  desc: string
}

type Emails = Map<string, Email>
type FolderName = string
type Folders = List<FolderName>
type Keys = Map<string, string>

type Colors = {
  selected: int * int
  normal: int * int
}

type Settings = {
  keys: Keys
  general: Map<string, string>
  debug: Map<string, string>
}

type Nav =
  | LIST
  | OPEN
  | QUITING

type State = {
  settings: Settings
  folders: Folders
  emails: Map<FolderName, Emails>
  currentFolder: int
  currentEmail: int
  selectedEmailIds: string Set
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
  | Quit
  | Fetch of AsyncReplyChannel<State>

type ProcessResult = {
  exitCode: int
  out: string
  err: string
}

type Style = {
  fg: int
  bg: int
  props: string
}

