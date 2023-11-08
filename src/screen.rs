extern crate ncurses;
use ncurses::*;
use core::str::Split;

pub static COLOR_CURRENT: i16 = -1;
pub static RED: i16 = 1;
pub static GREEN: i16 = 2;
pub static BLUE: i16 = 3;
pub static YELLOW: i16 = 4;
pub static WHITE: i16 = 5;
pub static BLACK: i16 = 6;

pub static RED_ON_BLACK: i16 = 11;
pub static GREEN_ON_BLACK: i16 = 12;
pub static BLUE_ON_BLACK: i16 = 13;
pub static YELLOW_ON_BLACK: i16 = 14;
pub static WHITE_ON_BLACK: i16 = 15;
pub static BLACK_ON_BLACK: i16 = 16;

const FIELD_COUNT: usize = 5;
const HEADER_ROW: i32 = 2;
pub const FIRST_EMAIL_ROW: i32 = 3;

pub struct Email {
    pub id: String,
    pub flags: String,
    pub subject: String,
    pub from: String,
    pub date: String,
}

pub fn setup() {
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

pub fn teardown() {
    endwin();
}

pub fn putline(email: &Email, highlight: bool) {
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

pub fn render_headings(fields: Split<'_, &str>) {
    let mut x = 0;

    attron(A_UNDERLINE() | A_BOLD());
    for (i, field) in fields.enumerate() {
        attron(COLOR_PAIR(WHITE));
        mvprintw(HEADER_ROW, x, field);
        x += 1 + field.len() as i32;
        if i < FIELD_COUNT - 1 {
            attron(COLOR_PAIR(BLACK));
            addstr("│");
        }
    }
    attroff(A_UNDERLINE() | A_BOLD());
}

////////////////////////////////////////////////////////////////////////////////
/// private

fn putfield(field: &str, color: i16, highlight: bool) {
    if highlight {
        attron(COLOR_PAIR(color + 10));
    } else {
        attron(COLOR_PAIR(color));
    }
    addstr(field);
}

