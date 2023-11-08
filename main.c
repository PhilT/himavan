#include <stdlib.h>
#include <ncurses.h>
#include <string.h>
#include <stdio.h>
#include <locale.h>
#include <ctype.h>

#define BUFFER_SIZE 2000
#define COLOR_CURRENT -1
#define LIST "himalaya list -f %s -w %d 2>&1"
#define RED 1
#define GREEN 2
#define BLUE 3
#define YELLOW 4

bool starts_with(char* str, char* sub) {
  return strncmp(str, sub, strlen(sub)) == 0;
}

void sub(char* str, int position, char* dest) {
  int fields[5];
  fields[0] = 0;
  int next = 1;
  int i = 0;

  for(; i < strlen(str); i ++) {
    if (starts_with(&str[i], "│")) {
      fields[next++] = i + strlen("│");
    }
  }
  fields[4] = i;

  FILE *f = fopen("himavan.log", "w");
  for(i = 0; i < 5; i ++) {
    fprintf(f, "%d\n", fields[i]);
  }
  fclose(f);

  int len = fields[position + 1] - fields[position];
  strncpy(dest, &str[fields[position]], len);
}

void trim(char* const a) {
    char *p = a, *q = a;
    while (isspace(*q))            ++q;
    while (*q)                     *p++ = *q++;
    *p = '\0';
    while (p > a && isspace(*--p)) *p = '\0';
}

void setup_ncurses() {
  initscr();
  refresh();
  noecho();
  start_color();
  use_default_colors();

  init_pair(RED, COLOR_RED, COLOR_CURRENT);
  init_pair(GREEN, COLOR_GREEN, COLOR_CURRENT);
  init_pair(BLUE, COLOR_BLUE, COLOR_CURRENT);
  init_pair(YELLOW, COLOR_YELLOW, COLOR_CURRENT);
}

int main() {
  setlocale(LC_ALL, "");

  setup_ncurses();

  int row, col;
  getmaxyx(stdscr, row, col);
  
  char cmd[100];
  sprintf(cmd, LIST, "Inbox", col);
  FILE *pipe = popen(cmd, "r");
  
  if (pipe == NULL) {
    endwin();
    perror("Error calling himalaya. Is it installed?");
    exit(1);
  }

  refresh();

  char response[BUFFER_SIZE];
  char field[BUFFER_SIZE];
  char *ignore;
  int y = 0;
  while (!feof(pipe)) {
    ignore = fgets(response, BUFFER_SIZE, pipe);

    trim(response);

    if (starts_with(response, "ID   ")) {
      attron(A_UNDERLINE | A_BOLD);
      mvprintw(y ++, 0, "%s            ", response);
      attroff(A_UNDERLINE | A_BOLD);
    } else if (strcmp(response, "") ) {
      attron(COLOR_PAIR(RED));
      sub(response, 0, field);
      mvprintw(y ++, 0, "%s", field);

      attron(COLOR_PAIR(GREEN));
      sub(response, 1, field);
      printw("%s", field);


    }
  }
  fclose(pipe);

  getch();
  endwin();
  return 0;
}
