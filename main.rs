extern crate ncurses;

use std::process::Command;
use ncurses::*;

const COLOR_CURRENT: i16 = -1;
const RED: i16 = 1;
const GREEN: i16 = 2;
const BLUE: i16 = 3;
const YELLOW: i16 = 4;
const WHITE: i16 = 5;
const BLACK: i16 = 6;

const RED_ON_BLACK: i16 = 11;
const GREEN_ON_BLACK: i16 = 12;
const BLUE_ON_BLACK: i16 = 13;
const YELLOW_ON_BLACK: i16 = 14;
const WHITE_ON_BLACK: i16 = 15;
const BLACK_ON_BLACK: i16 = 16;
//const colors = [RED, WHITE, GREEN, BLUE, YELLOW];

const FIELD_COUNT: usize = 5;

struct Email {
    id: String,
    flags: String,
    subject: String,
    from: String,
    date: String,
}

//let mut columns: [i32; 5] = [0, 0, 0, 0, 0];

fn setup_ncurses() {
  initscr();
  noecho();
  start_color();
  use_default_colors();
  curs_set(CURSOR_VISIBILITY::CURSOR_INVISIBLE);

  init_pair(RED, COLOR_RED, COLOR_CURRENT);
  init_pair(GREEN, COLOR_GREEN, COLOR_CURRENT);
  init_pair(BLUE, COLOR_BLUE, COLOR_CURRENT);
  init_pair(YELLOW, COLOR_YELLOW, COLOR_CURRENT);
  init_pair(WHITE, COLOR_WHITE, COLOR_CURRENT);
  init_pair(BLACK, COLOR_BLACK, COLOR_CURRENT);

  init_pair(RED_ON_BLACK, COLOR_RED, COLOR_BLACK);
  init_pair(GREEN_ON_BLACK, COLOR_GREEN, COLOR_BLACK);
  init_pair(BLUE_ON_BLACK, COLOR_BLUE, COLOR_BLACK);
  init_pair(YELLOW_ON_BLACK, COLOR_YELLOW, COLOR_BLACK);
  init_pair(WHITE_ON_BLACK, COLOR_WHITE, COLOR_BLACK);
  init_pair(BLACK_ON_BLACK, COLOR_BLACK, COLOR_BLACK);
}

fn putfield(field: &str, color: i16, highlight: bool) {
    if highlight {
        attron(COLOR_PAIR(color + 10));
    } else {
        attron(COLOR_PAIR(color));
    }
    addstr(field);
}

fn putline(email: &Email, highlight: bool) {
    putfield(&email.id, RED, highlight);
    putfield("│", BLACK, highlight);
    putfield(&email.flags, WHITE, highlight);
    putfield("│", BLACK, highlight);
    putfield(&email.subject, GREEN, highlight);
    putfield("│", BLACK, highlight);
    putfield(&email.from, BLUE, highlight);
    putfield("│", BLACK, highlight);
    putfield(&email.date, YELLOW, highlight);
}

fn main() {
    setlocale(LcCategory::all, "");
    setup_ncurses();

    let output = Command::new("himalaya")
        .args(["list", "-f", "Inbox", "-w", &COLS().to_string()])
        .output()
        .expect("Could not run himalaya command. Is it installed?")
        .stdout;

    let binding = String::from_utf8_lossy(&output);
    let mut lines = binding.split("\n");
    let mut y = 0;
    let mut x = 0;
    let mut i = 0;

    // Render headings
    lines.next().expect("No line"); // Skip blank line
    let fields = lines.next().expect("No line").split("│");
    attron(A_UNDERLINE() | A_BOLD());
    for field in fields {
        attron(COLOR_PAIR(WHITE));
        mvprintw(y, x, field);
        //columns[i] = x;
        x += 1 + field.len() as i32;
        i += 1;
        if i < FIELD_COUNT {
            attron(COLOR_PAIR(BLACK));
            addstr("│");
        }
    }
    attroff(A_UNDERLINE() | A_BOLD());
    y += 1;

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
    for email in data.iter() {
        if email.flags.contains("✷") {
            attron(A_BOLD());
        } else {
            attroff(A_BOLD());
        }
        wmove(stdscr(), y, 0);
        putline(email, false);

        y += 1;
    }

    // Render current row
    i = 0;

    // Get user input
    loop {
        wmove(stdscr(), i as i32 + 1, 0);
        putline(&data[i], true);
        refresh();

        match char::from_u32(getch() as u32) {
            Some('q') => break,
            Some('j') => {
                if i < data.len() - 1 {
                    wmove(stdscr(), i as i32 + 1, 0);
                    putline(&data[i], false);
                    i += 1;
                }
            },
            Some('k') => {
                if i > 0 {
                    wmove(stdscr(), i as i32 + 1, 0);
                    putline(&data[i], false);
                    i -= 1;
                }
            },
            Some('\n') => break,
            _ => {},
        }
    }
    endwin();
}
