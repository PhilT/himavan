extern crate ncurses;
use std::process::Command;
use ncurses::*;

pub mod screen;
use crate::screen::Email;

fn main() {
    setlocale(LcCategory::all, "");
    screen::setup();

    let output = Command::new("himalaya")
        .args(["list", "-f", "Inbox", "-w", &COLS().to_string()])
        .output()
        .expect("Could not run himalaya command. Is it installed?")
        .stdout;

    let binding = String::from_utf8_lossy(&output);
    let mut lines = binding.split("\n");

    // Render headings
    lines.next().expect("No line"); // Skip blank line
    let fields = lines.next().expect("No line").split("│");
    screen::render_headings(fields);

    // Split data into structs
    let mut data: Vec<Email> = Vec::new();
    for line in lines {
        let fields: Vec<&str> = line.split("│").collect();
        if fields.len() > 1 {
            data.push(Email {
                id: fields[0].to_string(),
                flags: fields[1].to_string(),
                subject: fields[2].to_string(),
                from: fields[3].to_string(),
                date: fields[4].to_string(),
            });
        }
    }

    // Initial render of rows
    let mut y = screen::FIRST_EMAIL_ROW;
    for email in data.iter() {
        if email.flags.contains("✷") {
            attron(A_BOLD());
        } else {
            attroff(A_BOLD());
        }
        wmove(stdscr(), y, 0);
        screen::putline(email, false);

        y += 1;
    }

    // Render current row
    let mut curr_email = 0;

    // Get user input
    loop {
        wmove(stdscr(), curr_email as i32 + screen::FIRST_EMAIL_ROW, 0);
        screen::putline(&data[curr_email], true);
        refresh();

        match char::from_u32(getch() as u32) {
            Some('q') => break,
            Some('j') => {
                if curr_email < data.len() - 1 {
                    wmove(stdscr(), curr_email as i32 + screen::FIRST_EMAIL_ROW, 0);
                    screen::putline(&data[curr_email], false);
                    curr_email += 1;
                }
            },
            Some('k') => {
                if curr_email > 0 {
                    wmove(stdscr(), curr_email as i32 + screen::FIRST_EMAIL_ROW, 0);
                    screen::putline(&data[curr_email], false);
                    curr_email -= 1;
                }
            },
            Some('D') => {
                //delete_email(data[i])
            }
            Some('\n') => break,
            _ => {},
        }
    }

    screen::teardown();
}
