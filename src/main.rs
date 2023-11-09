extern crate ncurses;
use std::process::Command;
use ncurses::*;
use core::str::Split;

pub mod screen;
use crate::screen::Email;

fn render_list(data: &Vec<Email>, columns: &Vec<i32>) {
    for (i, email) in data.iter().enumerate() {
        wmove(stdscr(), i as i32 + screen::FIRST_EMAIL_ROW, 0);
        screen::putline(email, false, &columns);
    }
}

fn delete_email(email: &Email) -> String {
    let output = Command::new("himalaya")
        .args(["delete", "-f", "Inbox", &email.id])
        .output()
        .expect("Could not run himalaya command. Is it installed?")
        .stdout;

    String::from_utf8_lossy(&output).to_string()
}

fn lines_to_emails(lines: Split<'_, &str>) -> Vec<Email> {
    let mut data: Vec<Email> = Vec::new();
    for line in lines.into_iter() {
        let fields: Vec<&str> = line.split("│").collect();
        if fields.len() > 1 {
            data.push(Email {
                id: fields[0].trim().to_string(),
                flags: fields[1].trim().to_string(),
                subject: fields[2].trim().to_string(),
                from: fields[3].trim().to_string(),
                date: fields[4].trim().to_string(),
            });
        }
    }
    data
}

fn fetch_emails() -> (Vec<String>, Vec<Email>) {
    let output = Command::new("himalaya")
        .args(["list", "-f", "Inbox", "-w", &COLS().to_string()])
        .output()
        .expect("Could not run himalaya command. Is it installed?")
        .stdout;

    let binding = String::from_utf8_lossy(&output);
    let mut lines = binding.split("\n");
    lines.next();

    let headings = 
        lines
        .next()
        .map(|line| line.split("│"))
        .expect("No response from himalaya process")
        .map(str::to_string)
        .collect();

    (headings, lines_to_emails(lines))
}

fn columns_from(row: &Vec<String>) -> Vec<i32> {
    let mut i = 0;
    row.iter().map(|column| { i += 1 + column.len() as i32; i - 1 }).collect()
}

fn main() {
    setlocale(LcCategory::all, "");
    screen::setup();

    // This needs to store the headings (or discard them?!)
    let (headings, mut emails) = fetch_emails();
    let columns = columns_from(&headings);
    screen::render_headings(headings);

    // Initial render of rows
    render_list(&emails, &columns);

    // Get user input
    let mut curr_email = 0;
    loop {
        wmove(stdscr(), curr_email as i32 + screen::FIRST_EMAIL_ROW, 0);
        screen::putline(&emails[curr_email], true, &columns);
        refresh();

        let ch = getch();

        wmove(stdscr(), screen::STATUS_LINE, 0);
        screen::color(screen::BLACK, false);
        screen::wipe_line();
        match char::from_u32(ch as u32) {
            Some('q') => break,
            Some('j') => {
                if curr_email < emails.len() - 1 {
                    wmove(stdscr(), curr_email as i32 + screen::FIRST_EMAIL_ROW, 0);
                    screen::putline(&emails[curr_email], false, &columns);
                    curr_email += 1;
                }
            },
            Some('k') => {
                if curr_email > 0 {
                    wmove(stdscr(), curr_email as i32 + screen::FIRST_EMAIL_ROW, 0);
                    screen::putline(&emails[curr_email], false, &columns);
                    curr_email -= 1;
                }
            },
            Some('D') => {
                let response = delete_email(&emails[curr_email]);
                (_, emails) = fetch_emails();
                render_list(&emails, &columns);
                wmove(stdscr(), screen::STATUS_LINE, 0);
                screen::putfield(&response, screen::GREEN, false);
            }
            Some('\n') => break,
            _ => {},
        }
    }

    screen::teardown();
}
