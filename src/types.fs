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
  | ADDR

type State = {
  settings: Settings
  folders: Folders
  emails: Map<FolderName, Emails>
  currentFolder: int
  currentEmail: int
  selectedEmailIds: string Set
  windowHeight: int
  nav: Nav Set
}

type Msg =
  | Update
  | SetCurrentEmail of int
  | SetCurrentFolder of int
  | Info of string
  | Notice of string
  | Error of string
  | Opening of string
  | ReadEmail of string
  | Back
  | NewEmails of FolderName * Emails
  | Select of int
  | ShowAddress
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

