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

const FIELD_COUNT: usize = 5;

fn setup_ncurses() {
  initscr();
  noecho();
  start_color();
  use_default_colors();

  init_pair(RED, COLOR_RED, COLOR_CURRENT);
  init_pair(GREEN, COLOR_GREEN, COLOR_CURRENT);
  init_pair(BLUE, COLOR_BLUE, COLOR_CURRENT);
  init_pair(YELLOW, COLOR_YELLOW, COLOR_CURRENT);
  init_pair(WHITE, COLOR_WHITE, COLOR_CURRENT);
  init_pair(BLACK, COLOR_BLACK, COLOR_CURRENT);
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
    let mut columns: [i32; 5] = [0, 0, 0, 0, 0];
    let mut i = 0;

    lines.next().expect("No line"); // Skip blank line
    let fields = lines.next().expect("No line").split("│");
    attron(A_UNDERLINE() | A_BOLD());
    for field in fields {
        attron(COLOR_PAIR(WHITE));
        mvprintw(y, x, field);
        attron(COLOR_PAIR(BLACK));
        addstr("│");
        columns[i] = x;
        x += 1 + field.len() as i32;
        i += 1;
    }
    attroff(A_UNDERLINE() | A_BOLD());
    y += 1;

    let colors = [RED, WHITE, GREEN, BLUE, YELLOW];

    for line in lines {
        let fields: Vec<&str> = line.split("│").collect();
        i = 0;
        if fields.len() > 1 {
            for field in &fields {
                if fields[1].contains("✷") {
                    attron(A_BOLD());
                } else {
                    attroff(A_BOLD());
                }
                attron(COLOR_PAIR(colors[i]));
                mvprintw(y, columns[i], field);
                i += 1;

                if i < FIELD_COUNT {
                    attron(COLOR_PAIR(BLACK));
                    addstr("│");
                }
            }
        }

        y += 1;
    }


    refresh();

    getch();
    endwin();
}
