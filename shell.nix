with import <nixpkgs> {};

mkShell {
  name = "ncurses";
  packages = [
    ncurses
    rustup
    rust-analyzer
  ];
}
