#include <stdlib.h>
#include <ncurses.h>
#include <string.h>
#include <stdio.h>
#include <locale.h>

#define BUFFER_SIZE 2000

int main() {
  setlocale(LC_ALL, "");

  initscr();
  refresh();
  noecho();

  int row, col;
  getmaxyx(stdscr, row, col);
  
  // Run external command (himalaya) and get the output
  // himalaya --output json
  char cmd[100];
  sprintf(cmd, "himalaya list -w %d 2>&1", col - 1);
  FILE *pipe = popen(cmd, "r");
  
  if (pipe == NULL) {
    endwin();
    perror("Error calling himalaya. Is it installed?");
    exit(1);
  }

  refresh();

  char response[BUFFER_SIZE];
  char *ignore;
  while (!feof(pipe)) {
    ignore = fgets(response, BUFFER_SIZE, pipe);
    printw("%s", response);
  }
  fclose(pipe);

  getch();
  endwin();
  return 0;
}
