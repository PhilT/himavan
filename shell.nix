with import <nixpkgs> {};

mkShell {
  name = "ncurses";
  packages = [
    ncurses
    gcc
  ];
}
