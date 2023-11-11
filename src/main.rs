extern crate ncurses;
use ncurses::*;
use core::str::Split;

pub mod screen;
pub mod mail;

const QUIT: char = 'q';
const LIST_DOWN: char = 'j';
const LIST_UP: char = 'k';
const LIST_DELETE: char = 'D';
const LIST_READ: char = '\n';
const LIST_ARCHIVE: char = 'A';
const LIST_SPAM: char = 'S';
const FOLDER_PREV: char = 'h';
const FOLDER_NEXT: char = 'l';

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
  for y in data.len() as i32..LINES() {
    wmove(stdscr(), y, 0);
    screen::wipe_line();
  }
}

fn lines_to_emails(lines: Split<'_, &str>) -> Vec<Email> {
  let mut data: Vec<Email> = Vec::new();
  for line in lines.into_iter() {
    let fields: Vec<&str> = line.split(mail::SEPARATOR).collect();
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

fn fetch_emails(folder: &str) -> (Vec<String>, Vec<Email>) {
  let (_status, response) = mail::list(folder, COLS(), LINES() - screen::FIRST_EMAIL_ROW);

  let mut lines = response.split("\n");
  lines.next();

  let headings =
    lines
    .next()
    .map(|line| line.split(mail::SEPARATOR))
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

  let (_, folders) = mail::folders();
  screen::render_folders(&folders, 0);

  let (headings, mut emails) = fetch_emails("INBOX");
  let columns = columns_from(&headings);
  screen::render_headings(&headings);

  // Initial render of rows
  render_list(&emails, &columns);

  // Get user input
  let mut curr_email = 0;
  let mut curr_folder = 0;
  loop {
    wmove(stdscr(), curr_email as i32 + screen::FIRST_EMAIL_ROW, 0);
    render_email(&emails[curr_email], true, &columns);
    refresh();

    let ch = getch();

    wmove(stdscr(), screen::STATUS_LINE, 0);
    screen::color(screen::BLACK, false);
    screen::wipe_line();
    match char::from_u32(ch as u32) {
      Some(QUIT) => break,
      Some(LIST_DOWN) => {
        if curr_email < emails.len() - 1 {
          wmove(stdscr(), curr_email as i32 + screen::FIRST_EMAIL_ROW, 0);
          render_email(&emails[curr_email], false, &columns);
          curr_email += 1;
        }
      },
      Some(LIST_UP) => {
        if curr_email > 0 {
          wmove(stdscr(), curr_email as i32 + screen::FIRST_EMAIL_ROW, 0);
          render_email(&emails[curr_email], false, &columns);
          curr_email -= 1;
        }
      },
      Some(FOLDER_PREV) => {
        if curr_folder > 0 {
          curr_email = 0;
          curr_folder -= 1;
          screen::render_folders(&folders, curr_folder);
          (_, emails) = fetch_emails(&folders[curr_folder]);
          render_list(&emails, &columns);
        }
      },
      Some(FOLDER_NEXT) => {
        if curr_folder < folders.len() - 1 {
          curr_email = 0;
          curr_folder += 1;
          screen::render_folders(&folders, curr_folder);
          (_, emails) = fetch_emails(&folders[curr_folder]);
          render_list(&emails, &columns);
        }
      },
      Some(LIST_DELETE) => {
        let (status, response) = mail::delete(&emails[curr_email].id, &folders[curr_folder]);
        wmove(stdscr(), screen::STATUS_LINE, 0);
        if status.success() {
          emails.remove(curr_email);
          screen::notice(&response);
          render_list(&emails, &columns);
        } else {
          screen::error(&response);
        }
      }
      Some(LIST_ARCHIVE) => {
        let (status, response) = mail::mv(&emails[curr_email].id, &folders[curr_folder], "Archive");
        if status.success() {
          emails.remove(curr_email);
          screen::notice(&response);
          render_list(&emails, &columns);
        } else {
          screen::error(&response);
        }
      },
      Some(LIST_SPAM) => {
        let (status, response) = mail::mv(&emails[curr_email].id, &folders[curr_folder], "Spam");
        if status.success() {
          emails.remove(curr_email);
          screen::notice(&response);
          render_list(&emails, &columns);
        } else {
          screen::error(&response);
        }
      },
      Some(LIST_READ) => {
        let (status, response) = mail::read(&emails[curr_email].id, &folders[curr_folder]);
        if status.success() {
          wmove(stdscr(), screen::HEADER_ROW, 0);
          screen::putfield(&response, screen::WHITE, false);
          getch();

          wmove(stdscr(), screen::HEADER_ROW, 0);
          screen::render_headings(&headings);
          render_list(&emails, &columns);
        } else {
          screen::error("Failed to read email!");
        }
      },
      _ => {},
    }
  }

  screen::teardown();
}
