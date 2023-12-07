use std::process::{Command, Stdio, ExitStatus};
use std::str;

pub const SEPARATOR: &str = "│";
const MAIL_PROG: &str = "himalaya";
const HIMALAYA_DRAFT_PATH: &str = "/tmp/himalaya-draft.eml";

pub fn folders() -> (ExitStatus, Vec<String>) {
  let (status, response) = run(MAIL_PROG, "folders", "", vec![], &[]);

  let folders: Vec<String> =
    response
    .split("\n")
    .skip(2)
    .filter(|line| line.trim() != "")
    .map(|line| line.split(SEPARATOR).collect::<Vec<&str>>()[0].trim().to_string())
    .collect::<Vec<String>>()
    .into_iter()
    .rev()
    .collect();

  (status, folders)
}

pub fn list(folder: &str, width: i32, limit: i32) -> (ExitStatus, String) {
  run(MAIL_PROG, "list", folder, vec![], &["-w", &width.to_string(), "-s", &limit.to_string()])
}

pub fn read(id: &str, folder: &str) -> (ExitStatus, String) {
  run(MAIL_PROG, "read", folder, vec![id], &[])
}

pub fn delete(id: &str, folder: &str) -> (ExitStatus, String) {
  run(MAIL_PROG, "delete", folder, vec![id], &[])
}

pub fn mv(id: &str, src: &str, dest: &str) -> (ExitStatus, String) {
  run(MAIL_PROG, "move", src, vec![id], &[dest])
}

pub fn write() -> Result<String, String> {
  let (status, response) =
    run(MAIL_PROG, "template", "", vec![], &["write"]);
  if status.success() {
    std::fs::write(HIMALAYA_DRAFT_PATH, response)
      // TODO: Proper error handling (don't panic!)
      .expect(&format!("Failed to write to {}", HIMALAYA_DRAFT_PATH));
    let result = spawn_editor::spawn_editor(None, &[HIMALAYA_DRAFT_PATH]);
    match result {
      Ok(_) => Ok(HIMALAYA_DRAFT_PATH.to_string()),
      Err(_) => Err("Failed to start $EDITOR".to_string()),
    }
  } else {
    Err(format!("Failed to get himalaya template '{}'", "write"))
  }
}

pub fn send(path: String) -> (ExitStatus, String) {
  let cat_cmd = Command::new("cat")
    .arg(&path)
    .stdout(Stdio::piped())
    .spawn()
    .unwrap();
  let mail_cmd = Command::new(MAIL_PROG)
    .arg("send")
    .stdin(Stdio::from(cat_cmd.stdout.unwrap()))
    .stdout(Stdio::piped())
    .spawn()
    .unwrap();
  let output = mail_cmd.wait_with_output().unwrap();

  (output.status, str::from_utf8(&output.stdout).unwrap().to_string())
}

fn run(exe: &str, cmd: &str, folder: &str, ids: Vec<&str>, options: &[&str]) -> (ExitStatus, String) {
  let mut args = vec![cmd];
  if folder != "" { args.extend(["-f", folder]) }
  args.extend(options.iter().cloned());
  args.extend(ids.iter().cloned());

  let output = Command::new(exe)
    .args(args)
    .output()
    .expect("could not run himalaya command. is it installed?");

  (output.status, String::from_utf8_lossy(&output.stdout).to_string())
}
