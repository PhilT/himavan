use std::process::Command;
use std::process::ExitStatus;

pub static SEPARATOR: &str = "│";

pub fn folders() -> (ExitStatus, Vec<String>) {
  let (status, response) = run("folders", "", vec![], vec![]);

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
  run("list", folder, vec![], vec!["-w", &width.to_string(), "-s", &limit.to_string()])
}

pub fn read(id: &str, folder: &str) -> (ExitStatus, String) {
  run("read", folder, vec![id], vec![])
}

pub fn delete(id: &str, folder: &str) -> (ExitStatus, String) {
  run("delete", folder, vec![id], vec![])
}

pub fn mv(id: &str, src: &str, dest: &str) -> (ExitStatus, String) {
  run("move", src, vec![id], vec![dest])
}

fn run(cmd: &str, folder: &str, ids: Vec<&str>, options: Vec<&str>) -> (ExitStatus, String) {
  let mut args = vec![cmd];
  if folder != "" { args.extend(["-f", folder]) }
  args.extend(options.iter().cloned());
  args.extend(ids.iter().cloned());

  let output = Command::new("himalaya")
    .args(args)
    .output()
    .expect("could not run himalaya command. is it installed?");

  (output.status, String::from_utf8_lossy(&output.stdout).to_string())
}
