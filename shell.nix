with import <nixpkgs> {};

mkShell {
  name = "ncurses";
  packages = [
    rustup
    ncurses
  ];
}
