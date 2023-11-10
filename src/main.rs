extern crate ncurses;
use ncurses::*;
use core::str::Split;

pub mod screen;
pub mod mail;

pub struct Email {
    pub id: String,
    pub flags: String,
    pub subject: String,
    pub from: String,
    pub date: String,
}

fn render_email(email: &Email, highlight: bool, columns: &Vec<i32>) {
    let fields = [
        email.id.clone(),
        email.flags.clone(),
        email.subject.clone(),
        email.from.clone(),
        email.date.clone(),
    ];
    screen::putline(fields, highlight, &columns);
}

fn render_list(data: &Vec<Email>, columns: &Vec<i32>) {
    for (i, email) in data.iter().enumerate() {
        wmove(stdscr(), i as i32 + screen::FIRST_EMAIL_ROW, 0);
        render_email(email, false, &columns);
    }
    for y in (data.len() as i32)..(LINES()) {
        wmove(stdscr(), y, 0);
        screen::wipe_line();
    }
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
    let (_status, response) = mail::list(COLS(), LINES() - screen::FIRST_EMAIL_ROW);

    let mut lines = response.split("\n");
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
    let mut columns: Vec<i32> = 
        row
        .iter()
        .map(|column| { i += 1 + column.len() as i32; i - 1 })
        .collect();
    columns.insert(0, 0);
    columns
}

fn main() {
    setlocale(LcCategory::all, "");
    screen::setup();

    // This needs to store the headings (or discard them?!)
    let (headings, mut emails) = fetch_emails();
    let columns = columns_from(&headings);
    screen::render_headings(&headings);

    // Initial render of rows
    render_list(&emails, &columns);

    // Get user input
    let mut curr_email = 0;
    loop {
        wmove(stdscr(), curr_email as i32 + screen::FIRST_EMAIL_ROW, 0);
        render_email(&emails[curr_email], true, &columns);
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
                    render_email(&emails[curr_email], false, &columns);
                    curr_email += 1;
                }
            },
            Some('k') => {
                if curr_email > 0 {
                    wmove(stdscr(), curr_email as i32 + screen::FIRST_EMAIL_ROW, 0);
                    render_email(&emails[curr_email], false, &columns);
                    curr_email -= 1;
                }
            },
            Some('D') => {
                let (status, response) = mail::delete(&emails[curr_email].id);
                wmove(stdscr(), screen::STATUS_LINE, 0);
                if status.success() {
                    emails.remove(curr_email);
                    screen::putfield(&response, screen::GREEN, false);
                    render_list(&emails, &columns);
                } else {
                    screen::putfield("Failed to delete email!", screen::RED, false);
                }
            }
            Some('\n') => {
                let (status, response) = mail::read(&emails[curr_email].id);
                if status.success() {
                    wmove(stdscr(), screen::HEADER_ROW, 0);
                    screen::putfield(&response, screen::WHITE, false);
                    getch();

                    wmove(stdscr(), screen::HEADER_ROW, 0);
                    screen::render_headings(&headings);
                    render_list(&emails, &columns);
                } else {
                    screen::putfield("Failed to read email!", screen::RED, false);
                }
            },
            _ => {},
        }
    }

    screen::teardown();
}
