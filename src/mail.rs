use std::process::Command;
use std::process::ExitStatus;

pub fn list(width: i32) -> (ExitStatus, String) {
    run("list", "Inbox", vec![], vec!["-w", &width.to_string()])
}
pub fn read(id: &str) -> (ExitStatus, String) {
    run("read", "Inbox", vec![id], vec![])
}

pub fn delete(id: &str) -> (ExitStatus, String) {
    run("delete", "Inbox", vec![id], vec![])
}

fn run(cmd: &str, folder: &str, ids: Vec<&str>, options: Vec<&str>) -> (ExitStatus, String) {
    let mut args = vec![cmd, "-f", folder];
    args.extend(options.iter().cloned());
    args.extend(ids.iter().cloned());

    let output = Command::new("himalaya")
        .args(args)
        .output()
        .expect("could not run himalaya command. is it installed?");

    (output.status, String::from_utf8_lossy(&output.stdout).to_string())
}
